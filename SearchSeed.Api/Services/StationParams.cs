namespace SearchSeed.Api.Services;

// 物流塔/仓储类 parameters 编解码（布局参考 edit-dspblue-print paramParserFactory）
public class StationParams
{
    // ---- 塔类（2103 行星/2104 星际/2316 大型采矿机）parameters 长度 2048 ----
    public class StorageRow
    {
        public int ItemId;       // 物品
        public int LocalRole;    // 0仓储 1本地供应 2本地需求
        public int RemoteRole;   // 0仓储 1星际供应 2星际需求
        public int Max;          // 物品上限
        public int LockAmount;   // 0不锁 1满仓 2半仓
    }
    public class SlotRow
    {
        public int Dir;          // 0未接 1输出 2输入
        public int StorageIdx;   // 0不输出 1-5物品栏 6翘曲器
    }

    public static (int maxItemKind, int numSlots) Layout(int itemId) => itemId switch
    {
        2103 => (4, 12),   // 行星内物流运输站
        2104 => (5, 12),   // 星际物流运输站
        2316 => (1, 9),    // 大型采矿机
        _ => (0, 0),
    };

    public class StationData
    {
        public List<StorageRow> Storage = new();
        public List<SlotRow> Slots = new();
        public double WorkEnergyMW;        // 最大充能功率
        public double TripRangeDronesDeg;  // 运输机最远路程（度）
        public double TripRangeShipsLy;    // 运输船最远路程（ly，10000=无限）
        public bool IncludeOrbitCollector;
        public double WarpEnableAu;        // 曲速启用路程（AU）
        public bool WarperNecessary;
        public int DeliveryDronesPct;      // 运输机起送量 %
        public int DeliveryShipsPct;       // 运输船起送量 %
        public int PilerCount;             // 集装数量 0=科技上限
        public int MiningSpeed;
        public bool DroneAutoReplenish;
        public bool ShipAutoReplenish;
    }

    public static StationData DecodeStation(int[] p)
    {
        int Get(int i) => i < p.Length ? p[i] : 0;
        var d = new StationData();
        for (int i = 0; i < 5; i++)
            d.Storage.Add(new StorageRow
            {
                ItemId = Get(i * 6), LocalRole = Get(i * 6 + 1), RemoteRole = Get(i * 6 + 2),
                Max = Get(i * 6 + 3), LockAmount = Get(i * 6 + 4),
            });
        for (int i = 0; i < 12; i++)
            d.Slots.Add(new SlotRow { Dir = Get(192 + i * 4), StorageIdx = Get(192 + i * 4 + 1) });
        const int o = 320;
        d.WorkEnergyMW = Math.Round(Get(o) / 5000.0 * 3 / 10, 1);
        d.TripRangeDronesDeg = Get(o + 1) == 0 ? 0 : 90 - Math.Round(Math.Asin(Math.Min(1.0, Math.Max(-1.0, Get(o + 1) / 100000000.0))) * 180 / Math.PI);
        d.TripRangeShipsLy = Math.Round(Get(o + 2) / 24000.0, 2);
        d.IncludeOrbitCollector = Get(o + 3) != 0;
        d.WarpEnableAu = Math.Round(Get(o + 4) / 40000.0, 2);
        d.WarperNecessary = Get(o + 5) != 0;
        d.DeliveryDronesPct = Get(o + 6);
        d.DeliveryShipsPct = Get(o + 7);
        d.PilerCount = Get(o + 8);
        d.MiningSpeed = Get(o + 9);
        d.DroneAutoReplenish = Get(o + 10) != 0;
        d.ShipAutoReplenish = Get(o + 11) != 0;
        return d;
    }

    public static int[] EncodeStation(StationData d, int[]? orig)
    {
        var p = new int[2048];
        if (orig != null) Array.Copy(orig, p, Math.Min(orig.Length, 2048));
        for (int i = 0; i < 5; i++)
        {
            var s = i < d.Storage.Count ? d.Storage[i] : new StorageRow();
            p[i * 6] = s.ItemId; p[i * 6 + 1] = s.LocalRole; p[i * 6 + 2] = s.RemoteRole;
            p[i * 6 + 3] = s.Max; p[i * 6 + 4] = s.LockAmount;
        }
        for (int i = 0; i < 12; i++)
        {
            var s = i < d.Slots.Count ? d.Slots[i] : new SlotRow();
            p[192 + i * 4] = s.Dir; p[192 + i * 4 + 1] = s.StorageIdx;
        }
        const int o = 320;
        p[o] = (int)Math.Round(d.WorkEnergyMW * 50000 / 3);
        p[o + 1] = (int)Math.Round(Math.Sin((90 - d.TripRangeDronesDeg) * Math.PI / 180) * 100000000);
        p[o + 2] = (int)Math.Round(d.TripRangeShipsLy * 24000);
        p[o + 3] = d.IncludeOrbitCollector ? 1 : 0;
        p[o + 4] = (int)Math.Round(d.WarpEnableAu * 40000);
        p[o + 5] = d.WarperNecessary ? 1 : 0;
        p[o + 6] = d.DeliveryDronesPct;
        p[o + 7] = d.DeliveryShipsPct;
        p[o + 8] = d.PilerCount;
        p[o + 9] = d.MiningSpeed;
        p[o + 10] = d.DroneAutoReplenish ? 1 : 0;
        p[o + 11] = d.ShipAutoReplenish ? 1 : 0;
        return p;
    }
}
