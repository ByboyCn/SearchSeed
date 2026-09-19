using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace SearchSeed.Api.Services;

// 蓝图解析：.blueprint 文件或游戏内复制的 BLUEPRINT:0 文本 → 结构化统计与流水线
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

    public string ItemName(int id) => Items.GetValueOrDefault(id.ToString(), "物品#" + id);

    // 输入支持：.blueprint 文件原始字节 / 粘贴的 BLUEPRINT:0,... 文本 / 纯 base64
    public JsonDocument Decompress(byte[] data)
    {
        // 尝试当文本（BLUEPRINT:0,"9","名字","base64"）
        var head = Encoding.UTF8.GetString(data.Take(Math.Min(data.Length, 64)).ToArray());
        if (head.StartsWith("BLUEPRINT:"))
        {
            var text = Encoding.UTF8.GetString(data).Trim();
            // 取最后一个引号包住的长 base64 段
            var m = System.Text.RegularExpressions.Regex.Match(text, "\"([A-Za-z0-9+/=]{100,})\"");
            if (!m.Success) throw new ArgumentException("蓝图文本格式无法识别");
            data = Convert.FromBase64String(m.Groups[1].Value);
        }
        else if (data.All(b => b is (>= 32 and <= 126) or 10 or 13 or 9))
        {
            // 可能是纯 base64 文本
            try
            {
                var txt = Encoding.UTF8.GetString(data).Trim().Replace("\n", "").Replace("\r", "");
                if (txt.Length > 100 && txt.All(c => char.IsLetterOrDigit(c) || c is '+' or '/' or '='))
                    data = Convert.FromBase64String(txt);
            }
            catch { }
        }
        // gzip 解压
        using var ms = new MemoryStream(data);
        using var gz = new GZipStream(ms, CompressionMode.Decompress);
        using var outMs = new MemoryStream();
        gz.CopyTo(outMs);
        return JsonDocument.Parse(outMs.ToArray());
    }

    public object Parse(byte[] data)
    {
        using var doc = Decompress(data);
        var root = doc.RootElement;

        // 兼容单蓝图/蓝图文件夹
        var bp = root;
        if (root.TryGetProperty("blueprints", out var bps) && bps.ValueKind == JsonValueKind.Array && bps.GetArrayLength() > 0)
            bp = bps[0];
        else if (root.TryGetProperty("blueprint", out var bpn) && bpn.ValueKind == JsonValueKind.Object)
            bp = bpn;

        string name = "";
        if (bp.TryGetProperty("header", out var header) && header.TryGetProperty("name", out var hn) && hn.ValueKind == JsonValueKind.String)
            name = hn.GetString() ?? "";
        if (string.IsNullOrEmpty(name) && bp.TryGetProperty("name", out var n2) && n2.ValueKind == JsonValueKind.String)
            name = n2.GetString() ?? "";

        var buildings = bp.TryGetProperty("buildings", out var b) && b.ValueKind == JsonValueKind.Array ? b : default;
        var belts = bp.TryGetProperty("belt_data", out var bd) && bd.ValueKind == JsonValueKind.Array ? bd : default;

        var buildingGroups = new Dictionary<string, int>();       // 建筑名 → 数量
        var recipeGroups = new Dictionary<string, RecipeAgg>();   // 配方名 → 聚合
        int buildingCount = 0, beltCount = belts.ValueKind == JsonValueKind.Array ? belts.GetArrayLength() : 0;
        double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;

        if (buildings.ValueKind == JsonValueKind.Array)
        {
            foreach (var bld in buildings.EnumerateArray())
            {
                buildingCount++;
                int itemId = bld.TryGetProperty("itemId", out var ii) ? ii.GetInt32() : 0;
                string bname = itemId > 0 ? ItemName(itemId) : "未知建筑";
                buildingGroups[bname] = buildingGroups.GetValueOrDefault(bname) + 1;

                if (bld.TryGetProperty("localOffset_x", out var lx) && bld.TryGetProperty("localOffset_y", out var ly))
                {
                    double x = lx.GetDouble(), y = ly.GetDouble();
                    minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                }

                if (bld.TryGetProperty("recipeId", out var ri) && ri.TryGetInt32(out var recipeId) && recipeId > 0
                    && Recipes.TryGetValue(recipeId.ToString(), out var rec))
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
        }

        // 每秒速率：产物速率 = 建筑数 × resultCount × 60 / timeSpend（帧@60fps）
        foreach (var agg in recipeGroups.Values)
        {
            var rec = Recipes.Values.First(r => r.name == agg.Name);
            double craftPerSec = rec.timeSpend > 0 ? 60.0 / rec.timeSpend : 0;
            agg.OutputRates = agg.Outputs.Select(o => new Rate { Name = o.Name, PerSec = Math.Round(agg.Buildings * o.PerCraft * craftPerSec, 2) }).ToList();
            agg.InputRates = agg.Inputs.Select(i2 => new Rate { Name = i2.Name, PerSec = Math.Round(agg.Buildings * i2.PerCraft * craftPerSec, 2) }).ToList();
        }

        return new
        {
            name,
            buildingCount,
            beltCount,
            area = buildingCount > 0 && minX != double.MaxValue ? new { width = Math.Round(maxX - minX, 1), height = Math.Round(maxY - minY, 1) } : null,
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