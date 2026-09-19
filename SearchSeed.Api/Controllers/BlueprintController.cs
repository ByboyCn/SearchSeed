using Microsoft.AspNetCore.Mvc;
using SearchSeed.Api.Services;
using System.Text.Json;

namespace SearchSeed.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlueprintController : ControllerBase
{
    readonly BlueprintService _bp;
    public BlueprintController(BlueprintService bp) => _bp = bp;

    async Task<byte[]> ReadBody()
    {
        var ct = Request.ContentType ?? "";
        if (ct.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) && Request.Form.Files.Count > 0)
        {
            using var ms = new MemoryStream();
            await Request.Form.Files[0].CopyToAsync(ms);
            return ms.ToArray();
        }
        using var ms2 = new MemoryStream();
        await Request.Body.CopyToAsync(ms2);
        return ms2.ToArray();
    }

    // 解析：返回统计 + JSON 模型（供前端编辑）
    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Parse()
    {
        try
        {
            var data = await ReadBody();
            if (data.Length == 0) return BadRequest("空的蓝图数据");
            var bp = _bp.Parse(data);
            return Ok(new { summary = _bp.Summarize(bp), json = ToJson(bp) });
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidDataException ex) { return BadRequest("蓝图数据无效: " + ex.Message); }
        catch (FormatException) { return BadRequest("base64 解析失败"); }
        catch (Exception ex) { return StatusCode(500, "解析失败: " + ex.Message); }
    }

    public class EditRequest
    {
        public string? Json { get; set; }          // 编辑后的蓝图 JSON
        public string? RawText { get; set; }       // 第二份蓝图文本（叠加时用）
        public string? Op { get; set; }            // translate / mirrorH / mirrorV / rotate / removeItems / stack
        public double Dx { get; set; } public double Dy { get; set; }
        public int Quarter { get; set; }
        public List<int> ItemIds { get; set; } = new();
        public string? NewName { get; set; }
        public string? NewDesc { get; set; }
        // 物流塔编辑：Op = "station" / "swapStation"
        public int? BuildingIndex { get; set; }
        public StationParams.StationData? StationData { get; set; }
        public bool ToInterstellar { get; set; }
    }

    // 编辑/变换 → 导出新蓝图文本
    [HttpPost("edit")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Edit([FromBody] EditRequest req)
    {
        try
        {
            BlueprintService.Blueprint bp;
            byte[]? rawBytes = null;
            if (!string.IsNullOrEmpty(req.RawText))
                rawBytes = System.Text.Encoding.UTF8.GetBytes(req.RawText);
            if (!string.IsNullOrEmpty(req.Json))
                bp = FromJson(req.Json);
            else if (rawBytes is { Length: > 0 })
                bp = _bp.Parse(rawBytes);
            else return BadRequest("缺少蓝图数据（json 或 rawText）");

            switch (req.Op)
            {
                case "translate": _bp.Translate(bp, req.Dx, req.Dy); break;
                case "mirrorH": _bp.Mirror(bp, true); break;
                case "mirrorV": _bp.Mirror(bp, false); break;
                case "rotate": _bp.Rotate(bp, req.Quarter); break;
                case "removeItems":
                    if (req.ItemIds.Count == 0) return BadRequest("未指定要删除的建筑物品 id");
                    _bp.RemoveItems(bp, req.ItemIds.ToHashSet());
                    break;
                case "stack":
                    if (rawBytes is not { Length: > 0 }) return BadRequest("叠加需要第二份蓝图（rawText）");
                    var second = _bp.Parse(rawBytes);
                    bp = BlueprintService.StackVertical(bp, second);
                    break;
                case "station":
                {
                    if (req.BuildingIndex is not int bi || req.StationData is null) return BadRequest("缺少建筑编号或塔参数");
                    var target = bp.Buildings.FirstOrDefault(x => x.Index == bi)
                        ?? bp.Buildings.FirstOrDefault(x => bp.Buildings.IndexOf(x) + 1 == bi);
                    if (target == null || target.ItemId is not (2103 or 2104 or 2316)) return BadRequest("指定的建筑不是物流塔");
                    target.Parameters = StationParams.EncodeStation(req.StationData, target.Parameters);
                    break;
                }
                case "swapStation":
                {
                    if (req.BuildingIndex is not int bi2) return BadRequest("缺少建筑编号");
                    var target2 = bp.Buildings.FirstOrDefault(x => x.Index == bi2)
                        ?? bp.Buildings.FirstOrDefault(x => bp.Buildings.IndexOf(x) + 1 == bi2);
                    if (target2 == null || target2.ItemId is not (2103 or 2104)) return BadRequest("指定的建筑不是物流运输站");
                    bool toInter = req.ToInterstellar;
                    int newId = toInter ? 2104 : 2103;
                    if (target2.ItemId == newId) break;
                    target2.ItemId = newId;
                    // 模型互换
                    target2.ModelIndex += toInter ? 1 : -1;
                    if (!toInter && target2.Parameters.Length >= 30)
                        for (int si = 0; si < 6; si++) target2.Parameters[si * 6 + 2] = 0; // 星际→行星：清空星际供需
                    break;
                }
                case null: break; // 仅改名/导出
                default: return BadRequest("未知操作: " + req.Op);
            }
            if (bp.Buildings.Count == 0) return BadRequest("编辑后蓝图为空");
            var text = _bp.Export(bp, req.NewName, req.NewDesc);
            return Ok(new { blueprint = text, summary = _bp.Summarize(bp), json = ToJson(bp) });
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidDataException ex) { return BadRequest("蓝图数据无效: " + ex.Message); }
        catch (JsonException) { return BadRequest("蓝图 JSON 不合法"); }
        catch (Exception ex) { return StatusCode(500, "编辑失败: " + ex.Message); }
    }

    string ToJson(BlueprintService.Blueprint bp) =>
        JsonSerializer.Serialize(new
        {
            version = bp.Version,
            cursorOffsetX = bp.CursorOffsetX, cursorOffsetY = bp.CursorOffsetY,
            cursorTargetArea = bp.CursorTargetArea,
            dragBoxX = bp.DragBoxX, dragBoxY = bp.DragBoxY, primaryAreaIdx = bp.PrimaryAreaIdx,
            gameVersion = bp.GameVersion, shortDesc = bp.ShortDesc, desc = bp.Desc,
            areas = bp.Areas,
            buildings = bp.Buildings.Select(b => new
            {
                index = b.Index, itemId = b.ItemId, name = _bp.ItemName(b.ItemId),
                modelIndex = b.ModelIndex, areaIndex = b.AreaIndex,
                x = b.X, y = b.Y, z = b.Z, yaw = b.Yaw,
                recipeId = b.RecipeId,
                filterId = b.FilterId,
                outputObjIdx = b.TempOutputObjIdx, inputObjIdx = b.TempInputObjIdx,
                parameters = b.Parameters, content = b.Content,
                station = (b.ItemId is 2103 or 2104 or 2316 && b.Parameters.Length >= 332)
                    ? StationParams.DecodeStation(b.Parameters) : null,
            }),
        }, new JsonSerializerOptions { IncludeFields = true });

    BlueprintService.Blueprint FromJson(string json)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var bp = new BlueprintService.Blueprint
        {
            CursorOffsetX = GetInt(root, "cursorOffsetX"),
            CursorOffsetY = GetInt(root, "cursorOffsetY"),
            CursorTargetArea = GetInt(root, "cursorTargetArea"),
            DragBoxX = GetInt(root, "dragBoxX"),
            DragBoxY = GetInt(root, "dragBoxY"),
            PrimaryAreaIdx = GetInt(root, "primaryAreaIdx", -1),
            GameVersion = GetString(root, "gameVersion") ?? "",
            ShortDesc = GetString(root, "shortDesc") ?? "",
            Desc = GetString(root, "desc") ?? "",
        };
        if (root.TryGetProperty("areas", out var areas))
            foreach (var a in areas.EnumerateArray())
                bp.Areas.Add(new BlueprintService.BpArea
                {
                    Index = (sbyte)GetInt(a, "Index"), ParentIndex = (sbyte)GetInt(a, "ParentIndex"),
                    TropicAnchor = (short)GetInt(a, "TropicAnchor"), AreaSegments = (short)GetInt(a, "AreaSegments"),
                    AnchorLocalOffsetX = (short)GetInt(a, "AnchorLocalOffsetX"), AnchorLocalOffsetY = (short)GetInt(a, "AnchorLocalOffsetY"),
                    Width = (short)GetInt(a, "Width"), Height = (short)GetInt(a, "Height"),
                });
        if (root.TryGetProperty("buildings", out var bs))
            foreach (var b in bs.EnumerateArray())
            {
                var nb = new BlueprintService.BpBuilding
                {
                    Index = GetInt(b, "index"),
                    ItemId = GetInt(b, "itemId"), ModelIndex = GetInt(b, "modelIndex"), AreaIndex = GetInt(b, "areaIndex"),
                    X = GetFloat(b, "x"), Y = GetFloat(b, "y"), Z = GetFloat(b, "z"), Yaw = GetFloat(b, "yaw"),
                    RecipeId = GetInt(b, "recipeId"), FilterId = GetInt(b, "filterId"),
                    TempOutputObjIdx = GetInt(b, "outputObjIdx", -1), TempInputObjIdx = GetInt(b, "inputObjIdx", -1),
                };
                if (b.TryGetProperty("parameters", out var ps))
                    nb.Parameters = ps.EnumerateArray().Select(p => p.GetInt32()).ToArray();
                if (b.TryGetProperty("content", out var cs) && cs.ValueKind == JsonValueKind.String)
                    nb.Content = cs.GetString();
                bp.Buildings.Add(nb);
            }
        return bp;
    }

    static int GetInt(JsonElement e, string name, int def = 0) =>
        e.TryGetProperty(name, out var v) && v.ValueKind != JsonValueKind.Null ? v.GetInt32() : def;
    static float GetFloat(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind != JsonValueKind.Null ? v.GetSingle() : 0f;
    static string? GetString(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}