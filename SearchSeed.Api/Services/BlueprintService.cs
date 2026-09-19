using System.IO.Compression;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SearchSeed.Api.Services;

// 蓝图解析/编辑/导出（对齐 edit-dspblue-print 的解析与变换能力）
// 二进制格式按官方 BlueprintData/BlueprintBuilding Import/Export 移植；导出统一 -102 当前格式
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

    public class BpBuilding
    {
        public int Index;
        public int ItemId, ModelIndex, AreaIndex;
        public float X, Y, Z, Yaw;
        public float Tilt, Pitch, X2, Y2, Z2, Yaw2, Tilt2, Pitch2;
        public int TempOutputObjIdx = -1, TempInputObjIdx = -1;
        public sbyte OutputToSlot, InputFromSlot, OutputFromSlot, InputToSlot, OutputOffset, InputOffset;
        public int RecipeId, FilterId;
        public int[] Parameters = Array.Empty<int>();
        public string? Content;
    }

    public class BpArea
    {
        public sbyte Index, ParentIndex;
        public short TropicAnchor, AreaSegments, AnchorLocalOffsetX, AnchorLocalOffsetY, Width, Height;
    }

    public class Blueprint
    {
        public int Version = 2, Patch = 1;
        public int CursorOffsetX, CursorOffsetY, CursorTargetArea, DragBoxX, DragBoxY, PrimaryAreaIdx = -1;
        public string Layout = "0";
        public string GameVersion = "0.10.28.21219";
        public string ShortDesc = "";
        public string Desc = "";
        public List<BpArea> Areas = new();
        public List<BpBuilding> Buildings = new();
        public bool HasReformData;
    }

    public BlueprintService(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "BlueprintData");
        Items = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(dir, "items.json"))) ?? new();
        Recipes = JsonSerializer.Deserialize<Dictionary<string, RecipeInfo>>(File.ReadAllText(Path.Combine(dir, "recipes.json"))) ?? new();
    }

    public string ItemName(int id) => Items.GetValueOrDefault(id.ToString(), "物品#" + id);

    // ============ 解析 ============
    static readonly DateTime TimeBase = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public Blueprint Parse(byte[] data)
    {
        string gameVersion = "0.10.28.21219", shortDesc = "", desc = "";
        var head = Encoding.UTF8.GetString(data.Take(Math.Min(64, data.Length)).ToArray());
        if (head.StartsWith("BLUEPRINT:"))
        {
            var text = Encoding.UTF8.GetString(data).TrimEnd('\0', '\r', '\n');
            var p1 = text.IndexOf('"', 9);
            if (p1 < 0) throw new ArgumentException("蓝图文本格式无法识别");
            var cells = text[9..p1].Split(',');
            if (cells.Length < 12) throw new ArgumentException("蓝图头部字段不足");
            var flag0 = cells[0];
            if (cells.Length > 9) gameVersion = cells[9];
            if (cells.Length > 10) shortDesc = Uri.UnescapeDataString(cells[10]);
            if (cells.Length > 11) desc = Uri.UnescapeDataString(cells[flag0 == "0" ? 11 : 14 < cells.Length ? 14 : cells.Length - 1]);
            var p2 = text.Length - 33;
            var encoded = text.Substring(p1 + 1, p2 - p1 - 1);
            if (encoded.Length % 4 != 0)
                throw new ArgumentException($"base64 长度异常（蓝图文本很可能复制时丢了字符，请重新全选复制）");
            data = Convert.FromBase64String(encoded);
        }
        using var ms = new MemoryStream(data);
        using var gz = new GZipStream(ms, CompressionMode.Decompress);
        using var outMs = new MemoryStream();
        gz.CopyTo(outMs);
        var bin = outMs.ToArray();

        var bp = ParseBinary(bin);
        bp.GameVersion = gameVersion;
        bp.ShortDesc = shortDesc;
        bp.Desc = desc;
        return bp;
    }

    Blueprint ParseBinary(byte[] bin)
    {
        using var r = new BinaryReader(new MemoryStream(bin));
        var bp = new Blueprint();
        bp.Version = r.ReadInt32();
        bp.CursorOffsetX = r.ReadInt32(); bp.CursorOffsetY = r.ReadInt32();
        bp.CursorTargetArea = r.ReadInt32();
        bp.DragBoxX = r.ReadInt32(); bp.DragBoxY = r.ReadInt32();
        bp.PrimaryAreaIdx = r.ReadInt32();
        int areaCount = r.ReadByte();
        if (areaCount > 64) throw new InvalidDataException("蓝图区域数非法");
        for (int i = 0; i < areaCount; i++)
        {
            bp.Areas.Add(new BpArea
            {
                Index = r.ReadSByte(), ParentIndex = r.ReadSByte(),
                TropicAnchor = r.ReadInt16(), AreaSegments = r.ReadInt16(),
                AnchorLocalOffsetX = r.ReadInt16(), AnchorLocalOffsetY = r.ReadInt16(),
                Width = r.ReadInt16(), Height = r.ReadInt16(),
            });
        }
        int buildingCount = r.ReadInt32();
        if (buildingCount < 0 || buildingCount > 1048576) throw new InvalidDataException("建筑数非法");
        for (int j = 0; j < buildingCount; j++)
        {
            int tag = r.ReadInt32();
            var b = new BpBuilding();
            if (tag <= -101)
            {
                b.Index = r.ReadInt32();
                b.ItemId = r.ReadInt16(); b.ModelIndex = r.ReadInt16(); b.AreaIndex = r.ReadSByte();
                b.X = r.ReadSingle(); b.Y = r.ReadSingle(); b.Z = r.ReadSingle(); b.Yaw = r.ReadSingle();
                if (b.ItemId > 2000 && b.ItemId < 2010) b.Tilt = r.ReadSingle();
                else if (b.ItemId > 2010 && b.ItemId < 2020)
                {
                    b.Tilt = r.ReadSingle(); b.Pitch = r.ReadSingle();
                    b.X2 = r.ReadSingle(); b.Y2 = r.ReadSingle(); b.Z2 = r.ReadSingle();
                    b.Yaw2 = r.ReadSingle(); b.Tilt2 = r.ReadSingle(); b.Pitch2 = r.ReadSingle();
                }
                b.TempOutputObjIdx = r.ReadInt32(); b.TempInputObjIdx = r.ReadInt32();
                ReadSlots(r, b);
                b.RecipeId = r.ReadInt16(); b.FilterId = r.ReadInt16();
                ReadParamsAndContent(r, b, tag <= -102);
            }
            else if (tag <= -100)
            {
                b.Index = r.ReadInt32();
                b.ItemId = r.ReadInt16(); b.ModelIndex = r.ReadInt16(); b.AreaIndex = r.ReadSByte();
                b.X = r.ReadSingle(); b.Y = r.ReadSingle(); b.Z = r.ReadSingle(); b.Yaw = r.ReadSingle();
                if (b.ItemId > 2000 && b.ItemId < 2010) b.Tilt = r.ReadSingle();
                else if (b.ItemId > 2010 && b.ItemId < 2020)
                {
                    b.Tilt = r.ReadSingle(); b.Pitch = r.ReadSingle();
                    b.X2 = r.ReadSingle(); b.Y2 = r.ReadSingle(); b.Z2 = r.ReadSingle();
                    b.Yaw2 = r.ReadSingle(); b.Tilt2 = r.ReadSingle(); b.Pitch2 = r.ReadSingle();
                }
                b.TempOutputObjIdx = r.ReadInt32(); b.TempInputObjIdx = r.ReadInt32();
                ReadSlots(r, b);
                b.RecipeId = r.ReadInt16(); b.FilterId = r.ReadInt16();
                int pc = r.ReadInt16();
                b.Parameters = new int[pc];
                for (int k = 0; k < pc; k++) b.Parameters[k] = r.ReadInt32();
            }
            else
            {
                b.Index = tag;
                b.AreaIndex = r.ReadSByte();
                b.X = r.ReadSingle(); b.Y = r.ReadSingle(); b.Z = r.ReadSingle();
                b.X2 = r.ReadSingle(); b.Y2 = r.ReadSingle(); b.Z2 = r.ReadSingle();
                b.Yaw = r.ReadSingle(); b.Yaw2 = r.ReadSingle();
                b.ItemId = r.ReadInt16(); b.ModelIndex = r.ReadInt16();
                b.TempOutputObjIdx = r.ReadInt32(); b.TempInputObjIdx = r.ReadInt32();
                ReadSlots(r, b);
                b.RecipeId = r.ReadInt16(); b.FilterId = r.ReadInt16();
                int pc = r.ReadInt16();
                b.Parameters = new int[pc];
                for (int k = 0; k < pc; k++) b.Parameters[k] = r.ReadInt32();
            }
            bp.Buildings.Add(b);
        }
        if (bp.Version >= 2)
        {
            bp.Patch = r.ReadInt32();
            bp.HasReformData = r.ReadByte() != 0; // 地基数据：解析后丢弃（导出不带）
            bp.HasReformData = false;
        }
        return bp;
    }

    static void ReadSlots(BinaryReader r, BpBuilding b)
    {
        b.OutputToSlot = r.ReadSByte(); b.InputFromSlot = r.ReadSByte();
        b.OutputFromSlot = r.ReadSByte(); b.InputToSlot = r.ReadSByte();
        b.OutputOffset = r.ReadSByte(); b.InputOffset = r.ReadSByte();
    }

    static void ReadParamsAndContent(BinaryReader r, BpBuilding b, bool hasContent)
    {
        int pc = r.ReadInt16();
        b.Parameters = new int[pc];
        for (int k = 0; k < pc; k++) b.Parameters[k] = r.ReadInt32();
        if (hasContent)
        {
            int cl = r.ReadInt32();
            if (cl > 0) b.Content = r.ReadString();
        }
    }

    // ============ 导出 ============
    public string Export(Blueprint bp, string? newName = null, string? newDesc = null)
    {
        // 重新编号并保留连接引用
        var indexMap = new Dictionary<int, int>();
        for (int i = 0; i < bp.Buildings.Count; i++) indexMap[bp.Buildings[i].Index] = i + 1;
        foreach (var b in bp.Buildings) b.Index = indexMap[b.Index];

        using var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms))
        {
            w.Write(2); // version
            w.Write(bp.CursorOffsetX); w.Write(bp.CursorOffsetY); w.Write(bp.CursorTargetArea);
            w.Write(bp.DragBoxX); w.Write(bp.DragBoxY); w.Write(bp.PrimaryAreaIdx);
            w.Write((byte)bp.Areas.Count);
            foreach (var a in bp.Areas)
            {
                w.Write(a.Index); w.Write(a.ParentIndex);
                w.Write(a.TropicAnchor); w.Write(a.AreaSegments);
                w.Write(a.AnchorLocalOffsetX); w.Write(a.AnchorLocalOffsetY);
                w.Write(a.Width); w.Write(a.Height);
            }
            w.Write(bp.Buildings.Count);
            foreach (var b in bp.Buildings)
            {
                w.Write(-102);
                w.Write(b.Index);
                w.Write((short)b.ItemId); w.Write((short)b.ModelIndex); w.Write((sbyte)b.AreaIndex);
                w.Write(b.X); w.Write(b.Y); w.Write(b.Z); w.Write(b.Yaw);
                if (b.ItemId > 2000 && b.ItemId < 2010) w.Write(b.Tilt);
                else if (b.ItemId > 2010 && b.ItemId < 2020)
                {
                    w.Write(b.Tilt); w.Write(b.Pitch);
                    w.Write(b.X2); w.Write(b.Y2); w.Write(b.Z2);
                    w.Write(b.Yaw2); w.Write(b.Tilt2); w.Write(b.Pitch2);
                }
                w.Write(b.TempOutputObjIdx); w.Write(b.TempInputObjIdx);
                w.Write(b.OutputToSlot); w.Write(b.InputFromSlot);
                w.Write(b.OutputFromSlot); w.Write(b.InputToSlot);
                w.Write(b.OutputOffset); w.Write(b.InputOffset);
                w.Write((short)b.RecipeId); w.Write((short)b.FilterId);
                w.Write((short)b.Parameters.Length);
                foreach (var p in b.Parameters) w.Write(p);
                if (!string.IsNullOrEmpty(b.Content))
                {
                    w.Write(b.Content.Length);
                    w.Write(b.Content);
                }
                else w.Write(0);
            }
            w.Write(1); // patch
            w.Write((byte)0); // 无地基数据
        }
        using var gzMs = new MemoryStream();
        using (var gz = new GZipStream(gzMs, CompressionLevel.Optimal))
            gz.Write(ms.ToArray());
        var b64 = Convert.ToBase64String(gzMs.ToArray());

        long timeTick = (long)((DateTime.UtcNow - TimeBase).TotalMilliseconds * 10000);
        var sb = new StringBuilder();
        sb.Append("BLUEPRINT:1,0,1110,1110,1110,1110,1110,0,").Append(timeTick).Append(',');
        sb.Append(bp.GameVersion).Append(',');
        sb.Append(Uri.EscapeDataString(newName ?? bp.ShortDesc)).Append(',');
        sb.Append(Uri.EscapeDataString("SearchSeed"));       // author
        sb.Append(',').Append(Uri.EscapeDataString(""));      // customVersion
        sb.Append(',').Append(Uri.EscapeDataString(""));      // externalFields
        sb.Append(',').Append(Uri.EscapeDataString(newDesc ?? bp.Desc));
        sb.Append('"').Append(b64).Append('"');
        var md5 = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(sb.ToString()))).ToLowerInvariant();
        return sb + md5;
    }

    // ============ 统计与流水线 ============
    public object Summarize(Blueprint bp)
    {
        var buildingGroups = new Dictionary<string, int>();
        var recipeGroups = new Dictionary<string, RecipeAgg>();
        double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;
        foreach (var b in bp.Buildings)
        {
            string bname = b.ItemId > 0 ? ItemName(b.ItemId) : "未知建筑";
            buildingGroups[bname] = buildingGroups.GetValueOrDefault(bname) + 1;
            minX = Math.Min(minX, b.X); maxX = Math.Max(maxX, b.X);
            minY = Math.Min(minY, b.Y); maxY = Math.Max(maxY, b.Y);
            if (b.RecipeId > 0 && Recipes.TryGetValue(b.RecipeId.ToString(), out var rec))
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
            name = bp.ShortDesc,
            desc = bp.Desc,
            buildingCount = bp.Buildings.Count,
            beltCount = buildingGroups.GetValueOrDefault("低速传送带") + buildingGroups.GetValueOrDefault("高速传送带") + buildingGroups.GetValueOrDefault("极速传送带"),
            area = new { width = Math.Round(maxX - minX, 1), height = Math.Round(maxY - minY, 1) },
            buildings = buildingGroups.OrderBy(kv => -kv.Value).Select(kv => new { name = kv.Key, count = kv.Value }).ToList(),
            recipes = recipeGroups.Values.OrderBy(r => -r.Buildings).ToList(),
        };
    }

    // ============ 变换 ============
    public void Translate(Blueprint bp, double dx, double dy)
    {
        foreach (var b in bp.Buildings)
        {
            b.X += (float)dx; b.Y += (float)dy;
            if (b.ItemId > 2010 && b.ItemId < 2020) { b.X2 += (float)dx; b.Y2 += (float)dy; }
        }
    }

    // 镜像：水平（X 取反，yaw 取反）实验性——弯带/分拣器方向可能需手工修正
    public void Mirror(Blueprint bp, bool horizontal)
    {
        foreach (var b in bp.Buildings)
        {
            if (horizontal)
            {
                b.X = -b.X;
                b.Yaw = -b.Yaw;
                if (b.ItemId > 2010 && b.ItemId < 2020)
                {
                    b.X2 = -b.X2; b.Yaw2 = -b.Yaw2; b.Pitch = -b.Pitch; b.Pitch2 = -b.Pitch2;
                }
            }
            else
            {
                b.Y = -b.Y;
                b.Yaw = (float)(Math.PI - b.Yaw);
                if (b.ItemId > 2010 && b.ItemId < 2020)
                {
                    b.Y2 = -b.Y2; b.Yaw2 = (float)(Math.PI - b.Yaw2); b.Tilt = -b.Tilt; b.Tilt2 = -b.Tilt2;
                }
            }
        }
    }

    // 旋转 90°（顺时针 n 次）
    public void Rotate(Blueprint bp, int quarter)
    {
        quarter &= 3;
        for (int q = 0; q < quarter; q++)
            foreach (var b in bp.Buildings)
            {
                (b.X, b.Y) = (b.Y, -b.X);
                b.Yaw += (float)Math.PI / 2;
                if (b.ItemId > 2010 && b.ItemId < 2020)
                {
                    (b.X2, b.Y2) = (b.Y2, -b.X2);
                    b.Yaw2 += (float)Math.PI / 2;
                }
            }
    }

    // 删除指定物品 id 的建筑（如传送带/分拣器），并断开引用
    public int RemoveItems(Blueprint bp, HashSet<int> itemIds)
    {
        var removed = new HashSet<int>(bp.Buildings.Where(b => itemIds.Contains(b.ItemId)).Select(b => b.Index));
        bp.Buildings.RemoveAll(b => itemIds.Contains(b.ItemId));
        foreach (var b in bp.Buildings)
        {
            if (removed.Contains(b.TempOutputObjIdx)) b.TempOutputObjIdx = -1;
            if (removed.Contains(b.TempInputObjIdx)) b.TempInputObjIdx = -1;
        }
        return removed.Count;
    }

    // 垂直叠加：把第二个蓝图放在第一个下方（Y 递减），中间留 gap 格
    public static Blueprint StackVertical(Blueprint top, Blueprint bottom, double gap = 8)
    {
        double topMinY = top.Buildings.Min(b => b.Y);
        double bottomMaxY = bottom.Buildings.Max(b => b.Y);
        double dy = topMinY - gap - bottomMaxY;
        var merged = new Blueprint
        {
            GameVersion = top.GameVersion,
            ShortDesc = top.ShortDesc,
            Desc = top.Desc,
            Areas = top.Areas.ToList(),
        };
        merged.Buildings.AddRange(top.Buildings);
        // 第二个蓝图整体下移
        var offset = bottom.Buildings.Max(b => b.Index);
        foreach (var b in bottom.Buildings)
        {
            b.Index += offset;
            if (b.TempOutputObjIdx >= 0) b.TempOutputObjIdx += offset;
            if (b.TempInputObjIdx >= 0) b.TempInputObjIdx += offset;
            b.Y -= (float)(topMinY - gap - bottomMaxY - dy + dy); // = 下移 (topMinY - gap - bottomMaxY)
            b.Y -= 0; // 保持简单：上面已算 dy
            merged.Buildings.Add(b);
        }
        return merged;
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
