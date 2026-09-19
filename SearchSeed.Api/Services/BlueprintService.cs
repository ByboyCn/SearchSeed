using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SearchSeed.Api.Services;

// 蓝图解析：.blueprint 文件或游戏内复制的 BLUEPRINT:0 文本 → 结构化统计与流水线
// 二进制格式按官方 Assembly-CSharp 的 BlueprintData/BlueprintBuilding.Import 移植
public class BlueprintService
{
    public Dictionary<string, string> Items { get; }
    public Dictionary<string, RecipeInfo> Recipes { get; }

    public class RecipeInfo
    {
        public string name { get; set; } = "";
        public int timeSpend { get; set; }
        public List<int> items { get; set; } = new();
        public List<int> itemCounts { get; set; } = new();
        public List<int> results { get; set; } = new();
        public List<int> resultCounts { get; set; } = new();
    }

    public BlueprintService(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "BlueprintData");
        Items = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(dir, "items.json"))) ?? new();
        Recipes = JsonSerializer.Deserialize<Dictionary<string, RecipeInfo>>(File.ReadAllText(Path.Combine(dir, "recipes.json"))) ?? new();
    }

    string? _lastDesc;
    public string? DescOf(byte[] data) => _lastDesc;

    public string ItemName(int id) => Items.GetValueOrDefault(id.ToString(), "物品#" + id);

    // 定位并解出 gzip 内层原始字节；文本模式同时提取蓝图名
    (byte[] inner, string name) Unwrap(byte[] data)
    {
        string name = "";
        var head = Encoding.UTF8.GetString(data.Take(Math.Min(64, data.Length)).ToArray());
        if (head.StartsWith("BLUEPRINT:"))
        {
            var text = Encoding.UTF8.GetString(data).Trim();
            // BLUEPRINT:<layout>,"<version>","<name>","<base64>","<md5>"
            var m = Regex.Match(text, "\"([A-Za-z0-9+/=]{100,})\"");
            if (!m.Success) throw new ArgumentException("蓝图文本格式无法识别（未找到 base64 数据段）");
            var b64 = m.Groups[1].Value;
            if (b64.Length % 4 != 0)
                throw new ArgumentException($"base64 长度异常（蓝图文本很可能复制时丢了字符，请回到游戏重新全选复制）");
            data = Convert.FromBase64String(b64);
            (name, _lastDesc) = TryParseHeader(text);
        }
        else if (data.All(b => b is (>= 32 and <= 126) or 10 or 13 or 9))
        {
            try
            {
                var txt = Encoding.UTF8.GetString(data).Trim().Replace("\r", "").Replace("\n", "");
                if (txt.Length > 100 && txt.All(c => char.IsLetterOrDigit(c) || c is '+' or '/' or '='))
                    data = Convert.FromBase64String(txt);
            }
            catch { }
        }
        using var ms = new MemoryStream(data);
        using var gz = new GZipStream(ms, CompressionMode.Decompress);
        using var outMs = new MemoryStream();
        gz.CopyTo(outMs);
        return (outMs.ToArray(), name);
    }

    // 新版头：BLUEPRINT:0,30,icon..,bigint,游戏版本,URL名,URL描述+"base64"...
    // 旧版头：BLUEPRINT:0,"9","名字","base64","md5"
    (string name, string? desc) TryParseHeader(string text)
    {
        var parts = text.Split(',');
        if (parts.Length >= 12)
        {
            // 新版：第 11 段(索引10)=URL编码名；第 12 段(索引11)=URL编码描述+引号base64
            var name = Uri.UnescapeDataString(parts[10]);
            var desc = parts[11].Contains('"') ? Uri.UnescapeDataString(parts[11].Split('"')[0]) : "";
            if (name.Length > 0 && !name.StartsWith('%') && !name.All(char.IsDigit))
                return (name, string.IsNullOrEmpty(desc) ? null : desc);
        }
        // 旧版：第 3 段(索引2)=引号名
        if (parts.Length >= 3)
        {
            var n = parts[2].Trim().Trim('"');
            if (n.Length > 0 && !n.All(char.IsDigit)) return (Uri.UnescapeDataString(n), null);
        }
        return ("", null);
    }

    class BpBuilding
    {
        public int ItemId, RecipeId;
        public double X, Y;
    }

    // 官方二进制格式解析（BlueprintData.Import / BlueprintBuilding.Import）
    List<BpBuilding> ParseBinary(byte[] bin)
    {
        using var r = new BinaryReader(new MemoryStream(bin));
        int version = r.ReadInt32();
        for (int i = 0; i < 6; i++) r.ReadInt32(); // cursorOffset_x/y/targetArea/dragBox_x/y/primaryAreaIdx
        int areaCount = r.ReadByte();
        if (areaCount > 64) throw new InvalidDataException("蓝图区域数非法");
        r.ReadBytes(areaCount * 14); // BlueprintArea 每条 14 字节
        int buildingCount = r.ReadInt32();
        if (buildingCount < 0 || buildingCount > 1048576) throw new InvalidDataException("建筑数非法");
        var list = new List<BpBuilding>(buildingCount);
        for (int j = 0; j < buildingCount; j++)
        {
            int tag = r.ReadInt32();
            var b = new BpBuilding();
            if (tag <= -101)
            {
                r.ReadInt32();              // index
                b.ItemId = r.ReadInt16();
                r.ReadInt16();              // modelIndex
                r.ReadSByte();              // areaIndex
                b.X = r.ReadSingle(); b.Y = r.ReadSingle();
                r.ReadSingle(); r.ReadSingle(); // z, yaw
                if (b.ItemId > 2000 && b.ItemId < 2010) r.ReadSingle();               // 分拣器 tilt
                else if (b.ItemId > 2010 && b.ItemId < 2020) r.ReadBytes(8 * 4);      // 弯传送带 8 float
                r.ReadInt32(); r.ReadInt32();  // tempOutput/Input
                r.ReadBytes(6);                 // 6 × sbyte 插槽
                b.RecipeId = r.ReadInt16();
                r.ReadInt16();                  // filterId
                int pc = r.ReadInt16();
                r.ReadBytes(pc * 4);
                if (tag <= -102 && r.ReadInt32() > 0) _ = r.ReadString(); // content
            }
            else if (tag <= -100)
            {
                r.ReadInt32();              // index
                r.ReadSByte();              // areaIndex
                r.ReadSingle();             // yaw
                b.ItemId = r.ReadInt16();
                r.ReadInt16();
                r.ReadInt32(); r.ReadInt32();
                r.ReadBytes(6);
                b.RecipeId = r.ReadInt16();
                r.ReadInt16();
                int pc = r.ReadInt16();
                r.ReadBytes(pc * 4);
            }
            else throw new InvalidDataException("未知的建筑记录版本: " + tag);
            list.Add(b);
        }
        return list;
    }

    List<(int itemId, int recipeId, double x, double y)> ReadBuildings(byte[] inner)
    {
        if (inner.Length > 0 && inner[0] == (byte)'{')
        {
            var node = JsonNode.Parse(inner);
            if (node is JsonObject root)
            {
                if (root.ContainsKey("blueprints") && root["blueprints"] is JsonArray arr && arr.Count > 0)
                    root = (arr[0] as JsonObject)!;
                else if (root.ContainsKey("blueprint") && root["blueprint"] is JsonObject bn)
                    root = bn;
                var result = new List<(int, int, double, double)>();
                if (root.ContainsKey("buildings") && root["buildings"] is JsonArray bs)
                    foreach (var b in bs.OfType<JsonObject>())
                    {
                        int itemId = b.ContainsKey("itemId") ? (int)b["itemId"]! : 0;
                        int recipeId = b.ContainsKey("recipeId") && b["recipeId"] != null ? (int)b["recipeId"]! : 0;
                        double x = b.ContainsKey("localOffset_x") ? (double)b["localOffset_x"]! : 0;
                        double y = b.ContainsKey("localOffset_y") ? (double)b["localOffset_y"]! : 0;
                        result.Add((itemId, recipeId, x, y));
                    }
                return result;
            }
        }
        return ParseBinary(inner).Select(b => (b.ItemId, b.RecipeId, b.X, b.Y)).ToList();
    }

    public object Parse(byte[] data)
    {
        var (inner, name) = Unwrap(data);
        string? desc = DescOf(data);
        var buildings = ReadBuildings(inner);
        if (buildings.Count == 0) throw new InvalidDataException("蓝图内没有建筑");

        var buildingGroups = new Dictionary<string, int>();
        var recipeGroups = new Dictionary<string, RecipeAgg>();
        double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;

        foreach (var (itemId, recipeId, x, y) in buildings)
        {
            string bname = itemId > 0 ? ItemName(itemId) : "未知建筑";
            buildingGroups[bname] = buildingGroups.GetValueOrDefault(bname) + 1;
            minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);

            if (recipeId > 0 && Recipes.TryGetValue(recipeId.ToString(), out var rec))
            {
                if (!recipeGroups.TryGetValue(rec.name, out var agg))
                {
                    agg = new RecipeAgg
                    {
                        Name = rec.name,
                        Building = bname,
                        Inputs = rec.items.Zip(rec.itemCounts, (id, c) => new ItemQty { Name = ItemName(id), PerCraft = c }).ToList(),
                        Outputs = rec.results.Zip(rec.resultCounts, (id, c) => new ItemQty { Name = ItemName(id), PerCraft = c }).ToList(),
                    };
                    recipeGroups[rec.name] = agg;
                }
                agg.Buildings++;
            }
        }

        foreach (var agg in recipeGroups.Values)
        {
            var rec = Recipes.Values.First(r => r.name == agg.Name);
            double craftPerSec = rec.timeSpend > 0 ? 60.0 / rec.timeSpend : 0;
            agg.OutputRates = agg.Outputs.Select(o => new Rate { Name = o.Name, PerSec = Math.Round(agg.Buildings * o.PerCraft * craftPerSec, 2) }).ToList();
            agg.InputRates = agg.Inputs.Select(i => new Rate { Name = i.Name, PerSec = Math.Round(agg.Buildings * i.PerCraft * craftPerSec, 2) }).ToList();
        }

        return new
        {
            name, desc,
            buildingCount = buildings.Count,
            beltCount = buildingGroups.GetValueOrDefault("低速传送带") + buildingGroups.GetValueOrDefault("高速传送带") + buildingGroups.GetValueOrDefault("极速传送带"),
            area = new { width = Math.Round(maxX - minX, 1), height = Math.Round(maxY - minY, 1) },
            buildings = buildingGroups.OrderBy(kv => -kv.Value).Select(kv => new { name = kv.Key, count = kv.Value }).ToList(),
            recipes = recipeGroups.Values.OrderBy(r => -r.Buildings).ToList(),
        };
    }
}

public class RecipeAgg
{
    public string Name { get; set; } = "";
    public string Building { get; set; } = "";
    public int Buildings { get; set; }
    public List<ItemQty> Inputs { get; set; } = new();
    public List<ItemQty> Outputs { get; set; } = new();
    public List<Rate> InputRates { get; set; } = new();
    public List<Rate> OutputRates { get; set; } = new();
}

public class ItemQty
{
    public string Name { get; set; } = "";
    public int PerCraft { get; set; }
}

public class Rate
{
    public string Name { get; set; } = "";
    public double PerSec { get; set; }
}