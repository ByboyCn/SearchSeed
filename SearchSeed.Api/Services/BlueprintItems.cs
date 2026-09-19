namespace SearchSeed.Api.Services;

// 建筑分类与插槽翻转数据表（移植自 edit-dspblue-print itemsUtil.js，GPL-3.0 上游）
public static class BpItems
{
    public static readonly HashSet<int> Belts = new() { 2001, 2002, 2003 };
    public static readonly HashSet<int> Inserters = new() { 2011, 2012, 2013, 2014 };
    // 可悬空建造（不需要地基底）
    public static readonly HashSet<int> Hanging = new() { 2001, 2002, 2003, 2011, 2012, 2013, 2014, 2030, 2313 };
    // 可堆叠建造（叠加时建立游戏内堆叠关系）
    public static readonly HashSet<int> Stackable = new() { 2020, 2040, 2101, 2102, 2106, 2901, 2902, 2313 };
    public static bool IsBelt(int id) => Belts.Contains(id);
    public static bool IsInserter(int id) => Inserters.Contains(id);
    public static bool IsStation(int id) => id is 2103 or 2104 or 2316;

    // (axis, 交换槽对)：axis=模型 yaw=0 时的对称轴；alterSlot=翻转时需调换的分拣器插槽
    public static readonly Dictionary<int, (string axis, int[][] alterSlot)> InserterSlotBuilds = new()
    {
        [2101] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2102] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2204] = ("y", new[] { new[] { 0, 4 }, new[] { 1, 3 } }),
        [2211] = ("y", new[] { new[] { 0, 4 }, new[] { 1, 3 } }),
        [2302] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2315] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2319] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2303] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2304] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2305] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2318] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2308] = ("y", new[] { new[] { 0, 5 }, new[] { 1, 4 }, new[] { 2, 3 }, new[] { 6, 8 } }),
        [2309] = ("x", new[] { new[] { 0, 6 }, new[] { 1, 5 }, new[] { 2, 4 }, new[] { 3, 7 } }),
        [2317] = ("x", new[] { new[] { 0, 6 }, new[] { 1, 5 }, new[] { 2, 4 }, new[] { 3, 7 } }),
        [2310] = ("x", new[] { new[] { 0, 8 }, new[] { 1, 7 }, new[] { 2, 6 }, new[] { 3, 5 } }),
        [2311] = ("y", new[] { new[] { 0, 1 } }),
        [2210] = ("y", new[] { new[] { 1, 3 } }),
        [2312] = ("y", new[] { new[] { 1, 2 } }),
        [2901] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2902] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [3009] = ("y", new[] { new[] { 0, 8 }, new[] { 1, 7 }, new[] { 2, 6 }, new[] { 3, 5 } }),
    };

    // 带传送带插槽的建筑：四向分流器按 modelIndex 区分
    public static readonly Dictionary<int, (string axis, int[][] alterSlot)> BeltSlotBuilds = new()
    {
        [2103] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2104] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 11 }, new[] { 4, 10 }, new[] { 5, 9 }, new[] { 6, 8 } }),
        [2316] = ("y", new[] { new[] { 0, 2 }, new[] { 3, 8 }, new[] { 4, 7 }, new[] { 5, 6 } }),
    };
    // 四向分流器多模型
    public static readonly Dictionary<int, (string axis, int[][] alterSlot)> SplitterModels = new()
    {
        [38] = ("y", new[] { new[] { 1, 3 } }),   // 十字单层
        [39] = ("y", Array.Empty<int[]>()),        // 一字双层
        [40] = ("y", new[] { new[] { 1, 3 } }),   // 十字双层
    };

    public static (string axis, int[][] alterSlot)? BeltSlotInfo(int itemId, int modelIndex)
    {
        if (itemId == 2020) return SplitterModels.GetValueOrDefault(modelIndex);
        return BeltSlotBuilds.GetValueOrDefault(itemId);
    }

    public static int? AlterSlot(int[][] alterSlot, int origin)
    {
        foreach (var a in alterSlot)
        {
            if (a[0] == origin) return a[1];
            if (a[1] == origin) return a[0];
        }
        return null;
    }
}
