namespace SearchSeed.Api.Services;

// 蓝图变换引擎（完整移植 edit-dspblue-print / 晨隐 的编辑功能）
public static class BpTransforms
{
    static BlueprintService.Blueprint Clone(BlueprintService.Blueprint bp) => new()
    {
        Version = bp.Version, CursorOffsetX = bp.CursorOffsetX, CursorOffsetY = bp.CursorOffsetY,
        CursorTargetArea = bp.CursorTargetArea, DragBoxX = bp.DragBoxX, DragBoxY = bp.DragBoxY,
        PrimaryAreaIdx = bp.PrimaryAreaIdx, GameVersion = bp.GameVersion,
        ShortDesc = bp.ShortDesc, Desc = bp.Desc,
        Areas = bp.Areas.Select(a => new BlueprintService.BpArea
        {
            Index = a.Index, ParentIndex = a.ParentIndex, TropicAnchor = a.TropicAnchor,
            AreaSegments = a.AreaSegments, AnchorLocalOffsetX = a.AnchorLocalOffsetX,
            AnchorLocalOffsetY = a.AnchorLocalOffsetY, Width = a.Width, Height = a.Height,
        }).ToList(),
        Buildings = bp.Buildings.Select(b => new BlueprintService.BpBuilding
        {
            Index = b.Index, ItemId = b.ItemId, ModelIndex = b.ModelIndex, AreaIndex = b.AreaIndex,
            X = b.X, Y = b.Y, Z = b.Z, Yaw = b.Yaw, Tilt = b.Tilt, Pitch = b.Pitch,
            X2 = b.X2, Y2 = b.Y2, Z2 = b.Z2, Yaw2 = b.Yaw2, Tilt2 = b.Tilt2, Pitch2 = b.Pitch2,
            TempOutputObjIdx = b.TempOutputObjIdx, TempInputObjIdx = b.TempInputObjIdx,
            OutputToSlot = b.OutputToSlot, InputFromSlot = b.InputFromSlot,
            OutputFromSlot = b.OutputFromSlot, InputToSlot = b.InputToSlot,
            OutputOffset = b.OutputOffset, InputOffset = b.InputOffset,
            RecipeId = b.RecipeId, FilterId = b.FilterId,
            Parameters = (int[])b.Parameters.Clone(), Content = b.Content,
        }).ToList(),
    };

    static BlueprintService.BpBuilding ByIndex(BlueprintService.Blueprint bp, int idx) =>
        bp.Buildings.FirstOrDefault(b => b.Index == idx)!;

    // ===== 线性变换（缩放/任意角度旋转/翻转，含插槽重映射）=====
    public static void LinearTransform(BlueprintService.Blueprint bp, double zoomX, double zoomY, double rotateDeg)
    {
        bool overturnX = zoomX < 0, overturnY = zoomY < 0;
        bool isOverturn = overturnX ^ overturnY;
        double rad = rotateDeg * Math.PI / 180;

        if (bp.Areas.Count > 0)
        {
            double w = Math.Abs(bp.Areas[0].Width * zoomX), h = Math.Abs(bp.Areas[0].Height * zoomY);
            int W = (int)Math.Ceiling(w * Math.Cos(Math.Abs(rad)) + h * Math.Sin(Math.Abs(rad)));
            int H = (int)Math.Ceiling(w * Math.Sin(Math.Abs(rad)) + h * Math.Cos(Math.Abs(rad)));
            bp.Areas[0].Width = (short)Math.Max(1, W);
            bp.Areas[0].Height = (short)Math.Max(1, H);
            bp.DragBoxX = W; bp.DragBoxY = H;
            bp.CursorOffsetX = W / 2; bp.CursorOffsetY = H / 2;
        }

        var beltSlotIdx = new HashSet<int>();
        if (isOverturn)
        {
            foreach (var b in bp.Buildings)
            {
                var bsi = BpItems.BeltSlotInfo(b.ItemId, b.ModelIndex);
                if (bsi is { } info)
                {
                    if (info.axis == "x") { b.Yaw -= 180; b.Yaw2 -= 180; }
                    // 交换 parameters 中的插槽：四向 priority(0-3) / 塔 slots storageIdx(192+i*4+1)
                    if (b.ItemId == 2020)
                    {
                        for (int i = 0; i < 4 && b.Parameters.Length > i; i++) { }
                        foreach (var pair in info.alterSlot)
                        {
                            int a = pair[0], c = pair[1];
                            if (b.Parameters.Length > a && b.Parameters.Length > c)
                                (b.Parameters[a], b.Parameters[c]) = (b.Parameters[c], b.Parameters[a]);
                        }
                    }
                    else if (BpItems.IsStation(b.ItemId) && b.Parameters.Length >= 192 + 12 * 4)
                    {
                        foreach (var pair in info.alterSlot)
                        {
                            int pa = 192 + pair[0] * 4 + 1, pc = 192 + pair[1] * 4 + 1;
                            (b.Parameters[pa], b.Parameters[pc]) = (b.Parameters[pc], b.Parameters[pa]);
                        }
                    }
                    beltSlotIdx.Add(b.Index);
                }
                if (BpItems.InserterSlotBuilds.TryGetValue(b.ItemId, out var iinfo) && iinfo.axis == "x")
                {
                    b.Yaw -= 180; b.Yaw2 -= 180;
                }
            }
        }

        foreach (var b in bp.Buildings)
        {
            double x = zoomX * b.X, y = zoomY * b.Y;
            double x2 = zoomX * b.X2, y2 = zoomY * b.Y2;
            b.X = (float)(x * Math.Cos(rad) - y * Math.Sin(rad));
            b.Y = (float)(x * Math.Sin(rad) + y * Math.Cos(rad));
            if (BpItems.IsInserter(b.ItemId))
            {
                b.X2 = (float)(x2 * Math.Cos(rad) - y2 * Math.Sin(rad));
                b.Y2 = (float)(x2 * Math.Sin(rad) + y2 * Math.Cos(rad));
            }
            else { b.X2 = b.X; b.Y2 = b.Y; }
            if (overturnX) { b.Yaw = -b.Yaw; b.Yaw2 = -b.Yaw2; }
            if (overturnY) { b.Yaw = 180 - b.Yaw; b.Yaw2 = 180 - b.Yaw2; }
            b.Yaw -= (float)rad; b.Yaw2 -= (float)rad;

            if (isOverturn)
            {
                double yr = b.Yaw * Math.PI / 180;
                if (b.ItemId is 2204 or 2211) // 火力/微型聚变：翻转偏移
                {
                    b.X += (float)(1 * Math.Cos(yr)); b.Y += (float)(-1 * Math.Sin(yr));
                }
                else if (b.ItemId is 2309 or 2317) // 化工厂/量化
                {
                    b.X += (float)(-1 * Math.Sin(yr)); b.Y += (float)(-1 * Math.Cos(yr));
                }
                else if (BpItems.IsBelt(b.ItemId))
                {
                    b.Tilt = -b.Tilt; b.Tilt2 = -b.Tilt2;
                    // 传送带接入建筑插槽重映射
                    if (b.TempInputObjIdx >= 0 && beltSlotIdx.Contains(b.TempInputObjIdx))
                    {
                        var ib = ByIndex(bp, b.TempInputObjIdx);
                        var ns = BpItems.AlterSlot((BpItems.BeltSlotInfo(ib.ItemId, ib.ModelIndex)?.alterSlot ?? Array.Empty<int[]>()), b.InputFromSlot);
                        if (ns != null) b.InputFromSlot = (sbyte)ns;
                    }
                    if (b.TempOutputObjIdx >= 0 && beltSlotIdx.Contains(b.TempOutputObjIdx))
                    {
                        var ob = ByIndex(bp, b.TempOutputObjIdx);
                        var ns = BpItems.AlterSlot((BpItems.BeltSlotInfo(ob.ItemId, ob.ModelIndex)?.alterSlot ?? Array.Empty<int[]>()), b.OutputToSlot);
                        if (ns != null) b.OutputToSlot = (sbyte)ns;
                    }
                }
                else if (BpItems.IsInserter(b.ItemId))
                {
                    // 分拣器接建筑插槽重映射
                    if (b.TempInputObjIdx >= 0)
                    {
                        var ib = ByIndex(bp, b.TempInputObjIdx);
                        if (BpItems.InserterSlotBuilds.TryGetValue(ib.ItemId, out var ii))
                        {
                            var ns = BpItems.AlterSlot(ii.alterSlot, b.InputFromSlot);
                            if (ns != null) b.InputFromSlot = (sbyte)ns;
                        }
                    }
                    if (b.TempOutputObjIdx >= 0)
                    {
                        var ob = ByIndex(bp, b.TempOutputObjIdx);
                        if (BpItems.InserterSlotBuilds.TryGetValue(ob.ItemId, out var oi))
                        {
                            var ns = BpItems.AlterSlot(oi.alterSlot, b.OutputToSlot);
                            if (ns != null) b.OutputToSlot = (sbyte)ns;
                        }
                    }
                }
            }
        }
        SnapToGrid(bp);
    }

    // 格点吸附：非分拣器建筑坐标取整（游戏合法格点），保证可放置
    public static void SnapToGrid(BlueprintService.Blueprint bp)
    {
        foreach (var b in bp.Buildings)
        {
            if (BpItems.IsInserter(b.ItemId)) continue;
            b.X = (float)Math.Round(b.X); b.Y = (float)Math.Round(b.Y);
            b.X2 = (float)Math.Round(b.X2); b.Y2 = (float)Math.Round(b.Y2);
        }
    }

    // ===== 垂直偏移（悬空建筑自动卡地基底）=====
    public static void VerticalOffsetZ(BlueprintService.Blueprint bp, double dz)
    {
        bool needBase = false, changedOrder = false;
        var newBuildings = new List<BlueprintService.BpBuilding>();
        int baseIndex = bp.Buildings.Count;
        foreach (var b in bp.Buildings)
        {
            b.Z += (float)dz; b.Z2 += (float)dz;
            if (b.ItemId == 1131) { b.Z = -10; b.Z2 = -10; }
            else if ((b.Z > 0.22f || b.Z2 > 0.22f) && b.TempInputObjIdx == -1 && !BpItems.Hanging.Contains(b.ItemId))
            {
                b.TempInputObjIdx = baseIndex; // 指向即将添加的地基
                needBase = true;
                if (BpItems.InserterSlotBuilds.ContainsKey(b.ItemId))
                {
                    newBuildings.Insert(0, b); // 确保比分拣器先创建
                    changedOrder = true;
                    continue;
                }
            }
            newBuildings.Add(b);
        }
        if (needBase)
        {
            newBuildings.Add(new BlueprintService.BpBuilding
            {
                Index = baseIndex, ItemId = 1131, ModelIndex = 37,
                Z = -10, Z2 = -10,
                TempOutputObjIdx = -1, TempInputObjIdx = -1,
                OutputToSlot = 0, InputFromSlot = 0, OutputFromSlot = 0, InputToSlot = 1,
                Parameters = Array.Empty<int>(),
            });
        }
        bp.Buildings = newBuildings;
        if (changedOrder) Renumber(bp);
    }

    static void Renumber(BlueprintService.Blueprint bp)
    {
        var map = new Dictionary<int, int>();
        for (int i = 0; i < bp.Buildings.Count; i++) map[bp.Buildings[i].Index] = i + 1;
        foreach (var b in bp.Buildings)
        {
            b.Index = map[b.Index];
            if (b.TempOutputObjIdx != -1 && map.TryGetValue(b.TempOutputObjIdx, out var o)) b.TempOutputObjIdx = o;
            if (b.TempInputObjIdx != -1 && map.TryGetValue(b.TempInputObjIdx, out var v)) b.TempInputObjIdx = v;
        }
    }

    static int FindUppermost(BlueprintService.Blueprint bp, int fromIndex, int itemId, int index)
    {
        var b = ByIndex(bp, index);
        if (b.TempInputObjIdx != -1)
            return FindUppermost(bp, fromIndex, itemId, b.TempInputObjIdx);
        return fromIndex;
    }

    // ===== 垂直叠加（Z 轴多层 + 堆叠关系）=====
    public static void VerticalCopy(BlueprintService.Blueprint bp, int floors, double spacing, bool isPile)
    {
        var prevFloor = bp.Buildings.ToList();
        int offset = 0;
        foreach (var f in prevFloor) offset = Math.Max(offset, f.Index);
        for (int i = 2; i <= floors; i++)
        {
            var nextFloor = new List<BlueprintService.BpBuilding>();
            foreach (var v0 in prevFloor)
            {
                var v = Clone(new BlueprintService.Blueprint { Buildings = new List<BlueprintService.BpBuilding> { v0 } }).Buildings[0];
                v.Index = v0.Index + offset;
                v.Z += (float)spacing; v.Z2 += (float)spacing;
                if (v0.ItemId == 1131) { v.Z = -10; v.Z2 = -10; }
                else
                {
                    if (v0.TempOutputObjIdx != -1) v.TempOutputObjIdx = v0.TempOutputObjIdx + offset;
                    if (v0.TempInputObjIdx != -1) v.TempInputObjIdx = v0.TempInputObjIdx + offset;
                    else if (isPile && BpItems.Stackable.Contains(v0.ItemId))
                        v.TempInputObjIdx = FindUppermost(bp, v0.Index, v0.ItemId, v0.Index);
                }
                nextFloor.Add(v);
                bp.Buildings.Add(v);
            }
            prevFloor = nextFloor;
            offset = bp.Buildings.Max(b => b.Index);
        }
        Renumber(bp);
        VerticalOffsetZ(bp, 0); // 悬空补地基
    }

    // ===== 水平拼接（左右对齐）=====
    public static void StackHorizontal(BlueprintService.Blueprint left, BlueprintService.Blueprint right, string align, double gap = 4)
    {
        double leftMaxX = left.Buildings.Max(b => b.X);
        double rightMinX = right.Buildings.Min(b => b.X);
        double rightMaxY = right.Buildings.Max(b => b.Y);
        double rightMinY = right.Buildings.Min(b => b.Y);
        double leftMaxY = left.Buildings.Max(b => b.Y);
        double leftMinY = left.Buildings.Min(b => b.Y);
        double dx = leftMaxX + gap - rightMinX;
        double dy = align switch
        {
            "top" => leftMaxY - rightMaxY,       // 顶对齐
            "bottom" => leftMinY - rightMinY,    // 底对齐
            _ => (leftMaxY + leftMinY) / 2 - (rightMaxY + rightMinY) / 2, // 居中
        };
        int offset = left.Buildings.Max(b => b.Index);
        foreach (var b in right.Buildings)
        {
            b.Index += offset;
            if (b.TempOutputObjIdx >= 0) b.TempOutputObjIdx += offset;
            if (b.TempInputObjIdx >= 0) b.TempInputObjIdx += offset;
            b.X += (float)dx; b.Y += (float)dy;
            if (BpItems.IsInserter(b.ItemId)) { b.X2 += (float)dx; b.Y2 += (float)dy; }
            left.Buildings.Add(b);
        }
    }

    // ===== 无中生有：无褶皱垂直传送带 =====
    public static BlueprintService.Blueprint CreateVBelts(double startZ, double endZ)
    {
        var bp = new BlueprintService.Blueprint { ShortDesc = $"无褶皱垂直传送带-{startZ}-{endZ}" };
        bool isUp = startZ <= endZ;
        const double skew = 0.0002;
        double curX = 0;
        BlueprintService.BpBuilding Belt(int idx, double z, int outputIdx)
        {
            curX += skew;
            return new BlueprintService.BpBuilding
            {
                Index = idx, ItemId = 2003, Z = (float)z, Z2 = (float)z, X = (float)curX, Y = 0,
                X2 = (float)curX, Y2 = 0, TempOutputObjIdx = outputIdx, TempInputObjIdx = -1,
                OutputFromSlot = 0, InputToSlot = 1, Parameters = Array.Empty<int>(),
            };
        }
        bp.Buildings.Add(Belt(0, endZ, -1));
        double prevZ = endZ;
        double end = isUp ? Math.Floor(endZ * 2) / 2 : Math.Ceiling(endZ * 2) / 2;
        double start = isUp ? Math.Ceiling(startZ * 2) / 2 : Math.Floor(startZ * 2) / 2;
        for (double z = end; isUp ? z > start : z < start; z += isUp ? -0.5 : 0.5)
        {
            if (z != prevZ)
            {
                prevZ = z;
                int len = bp.Buildings.Count;
                bp.Buildings.Add(Belt(len, prevZ, len - 1));
            }
        }
        int last = bp.Buildings.Count;
        bp.Buildings.Add(Belt(last, startZ, last - 1));
        return bp;
    }
}