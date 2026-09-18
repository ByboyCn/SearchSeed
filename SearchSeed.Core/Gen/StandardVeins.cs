using SearchSeed.Core.Data;
using SearchSeed.Core.Maths;
using static SearchSeed.Core.Data.ConstValue;

namespace SearchSeed.Core.Gen;

// Port of PlanetAlgorithm::GenerateVeins (standard/precise mode)
public static class StandardVeins
{
    public static void Generate(PlanetAlgorithm algo, GalaxyGen galaxy, StarClass star, PlanetClass planet)
    {
        if (planet.AlgoId == 0) return;
        var themeProto = Ldb.Select(planet.Theme);
        var rnd = new DotNet35Random(planet.Seed);
        rnd.Next(); rnd.Next(); rnd.Next(); rnd.Next();
        int birthSeed = rnd.Next();
        var rnd2 = new DotNet35Random(rnd.Next());
        float num = 2.1f / NormalPlanetRadius;
        int[] array = new int[15];
        float[] array2 = new float[15];
        float[] array3 = new float[15];
        for (int i = 0; i < themeProto.VeinSpot.Length; ++i) array[i + 1] = themeProto.VeinSpot[i];
        for (int i = 0; i < themeProto.VeinCount.Length; ++i) array2[i + 1] = themeProto.VeinCount[i];
        for (int i = 0; i < themeProto.VeinOpacity.Length; ++i) array3[i + 1] = themeProto.VeinOpacity[i];

        float p = 1.0f;
        if (star.Type == EStarType.MainSeqStar)
            p = star.Spectr switch
            {
                ESpectrType.M => 2.5f, ESpectrType.K => 1.0f, ESpectrType.G => 0.7f,
                ESpectrType.F => 0.6f, ESpectrType.A => 1.0f, ESpectrType.B => 0.4f,
                ESpectrType.O => 1.6f, _ => 1.0f
            };
        else if (star.Type == EStarType.GiantStar) p = 2.5f;
        else if (star.Type == EStarType.WhiteDwarf)
        {
            p = 3.5f;
            array[9] += 2;
            for (int j = 1; j < 12 && rnd.NextDouble() < 0.44999998807907104; ++j) array[9]++;
            array2[9] = 0.7f; array3[9] = 1.0f;
            array[10] += 2;
            for (int k = 1; k < 12 && rnd.NextDouble() < 0.44999998807907104; ++k) array[10]++;
            array2[10] = 0.7f; array3[10] = 1.0f;
            array[12]++;
            for (int l = 1; l < 12 && rnd.NextDouble() < 0.5; ++l) array[12]++;
            array2[12] = 0.7f; array3[12] = 0.3f;
        }
        else if (star.Type == EStarType.NeutronStar)
        {
            p = 4.5f;
            array[14]++;
            for (int m = 1; m < 12 && rnd.NextDouble() < 0.6499999761581421; ++m) array[14]++;
            array2[14] = 0.7f; array3[14] = 0.3f;
        }
        else if (star.Type == EStarType.BlackHole)
        {
            p = 5.0f;
            array[14]++;
            for (int i2 = 1; i2 < 12 && rnd.NextDouble() < 0.6499999761581421; ++i2) array[14]++;
            array2[14] = 0.7f; array3[14] = 0.3f;
        }

        for (int n = 0; n < themeProto.RareVeins.Length; ++n)
        {
            int num2 = themeProto.RareVeins[n];
            float num3 = star.Index == 0 ? themeProto.RareSettings[n * 4] : themeProto.RareSettings[n * 4 + 1];
            float num4 = themeProto.RareSettings[n * 4 + 2];
            float num5 = themeProto.RareSettings[n * 4 + 3];
            num3 = 1.0f - Mathf.Pow(1.0f - num3, p);
            num5 = 1.0f - Mathf.Pow(1.0f - num5, p);
            if (!(rnd.NextDouble() < num3)) continue;
            array[num2]++;
            array2[num2] = num5;
            array3[num2] = num5;
            for (int num7 = 1; num7 < 12 && rnd.NextDouble() < num4; ++num7) array[num2]++;
        }

        float num8 = star.ResourceCoef;
        bool flag = galaxy.BirthPlanetId == planet.Id;
        if (flag) num8 *= 2.0f / 3.0f;
        else if (galaxy.IsRareResource)
        {
            if (num8 > 1.0f) num8 = Mathf.Pow(num8, 0.8f);
            num8 *= 0.7f;
        }

        var veinVectors = new Vec3[512];
        var veinVectorTypes = new EVeinType[512];
        var tmpVecs = new List<Vec2>();
        int veinVectorCount = 0;
        Vec3 birthPoint;
        if (flag)
        {
            var (bp, r0, r1) = algo.GenBirthPoints(planet, birthSeed, star.UPosition);
            birthPoint = bp;
            veinVectors[0] = r0;
            veinVectors[1] = r1;
            birthPoint.Normalize();
            birthPoint *= 0.75f;
            veinVectorTypes[0] = EVeinType.Iron;
            veinVectorTypes[1] = EVeinType.Copper;
            veinVectorCount = 2;
        }
        else
        {
            birthPoint = new Vec3(
                (float)rnd2.NextDouble() * 2.0f - 1.0f,
                (float)rnd2.NextDouble() - 0.5f,
                (float)rnd2.NextDouble() * 2.0f - 1.0f);
            birthPoint.Normalize();
            birthPoint *= (float)(rnd2.NextDouble() * 0.4 + 0.2);
        }

        for (int veinTypeIndex = 1; veinTypeIndex < 15; veinTypeIndex++)
        {
            if (veinVectorCount >= veinVectors.Length) break;
            var eVeinType = (EVeinType)veinTypeIndex;
            int veinGroupNum = array[veinTypeIndex];
            if (veinGroupNum > 1) veinGroupNum += rnd2.Next(-1, 2);
            for (int veinGroupIndex = 0; veinGroupIndex < veinGroupNum; veinGroupIndex++)
            {
                int tryNum1 = 0;
                Vec3 targetPos = Vec3.Zero;
                bool flag2 = false;
                while (tryNum1++ < 200)
                {
                    targetPos.X = (float)rnd2.NextDouble() * 2.0f - 1.0f;
                    targetPos.Y = (float)rnd2.NextDouble() * 2.0f - 1.0f;
                    targetPos.Z = (float)rnd2.NextDouble() * 2.0f - 1.0f;
                    if (eVeinType != EVeinType.Oil) targetPos += birthPoint;
                    targetPos.Normalize();
                    if (algo.CheckVeinPosition(targetPos, eVeinType)) continue;
                    bool flag3 = false;
                    float num15 = eVeinType == EVeinType.Oil ? 100.0f : 196.0f;
                    for (int num16 = 0; num16 < veinVectorCount; num16++)
                    {
                        if ((veinVectors[num16] - targetPos).SqrMagnitude < num * num * num15)
                        {
                            flag3 = true;
                            break;
                        }
                    }
                    if (!flag3) { flag2 = true; break; }
                }
                if (flag2)
                {
                    veinVectors[veinVectorCount] = targetPos;
                    veinVectorTypes[veinVectorCount] = eVeinType;
                    veinVectorCount++;
                    if (veinVectorCount == veinVectors.Length) break;
                }
            }
        }

        for (int veinGroupIndex2 = 0; veinGroupIndex2 < veinVectorCount; veinGroupIndex2++)
        {
            tmpVecs.Clear();
            Vec3 normalized = veinVectors[veinGroupIndex2].Normalized;
            var eVeinType2 = veinVectorTypes[veinGroupIndex2];
            int veinPointType = (int)eVeinType2;
            var quaternion = Quat.FromToRotation(Vec3.Up, normalized);
            Vec3 vector = quaternion * Vec3.Right;
            Vec3 vector2 = quaternion * Vec3.Forward;
            tmpVecs.Add(Vec2.Zero);
            int veinPointNum = Mathf.RoundToInt(array2[veinPointType] * rnd2.Next(20, 25));
            if (eVeinType2 == EVeinType.Oil) veinPointNum = 1;
            float num20 = array3[veinPointType];
            if (flag && veinGroupIndex2 < 2)
            {
                veinPointNum = 6;
                num20 = 0.2f;
            }
            int tryNum2 = 0;
            while (tryNum2++ < 20)
            {
                int count = tmpVecs.Count;
                for (int veinPointIndex = 0; veinPointIndex < count; veinPointIndex++)
                {
                    if (tmpVecs.Count >= veinPointNum) break;
                    if (tmpVecs[veinPointIndex].SqrMagnitude > 36.0f) continue;
                    double num23 = rnd2.NextDouble() * System.Math.PI * 2.0;
                    Vec2 vector3 = new Vec2((float)System.Math.Cos(num23), (float)System.Math.Sin(num23));
                    vector3 += tmpVecs[veinPointIndex] * 0.2f;
                    vector3.Normalize();
                    Vec2 newVeinPointPos = tmpVecs[veinPointIndex] + vector3;
                    bool flag4 = false;
                    for (int num24 = 0; num24 < tmpVecs.Count; num24++)
                    {
                        if ((tmpVecs[num24] - newVeinPointPos).SqrMagnitude < 0.85f) { flag4 = true; break; }
                    }
                    if (!flag4) tmpVecs.Add(newVeinPointPos);
                }
                if (tmpVecs.Count >= veinPointNum) break;
            }

            float num25 = num8;
            if (eVeinType2 == EVeinType.Oil) num25 = Mathf.Pow(num8, 0.5f);
            int num26 = Mathf.RoundToInt(num20 * 100000.0f * num25);
            if (num26 < 20) num26 = 20;
            int num27 = num26 < 16000 ? Mathf.FloorToInt(num26 * 0.9375f) : 15000;
            int minValue = num26 - num27;
            int maxValue = num26 + num27 + 1;
            for (int veinPointIndex2 = 0; veinPointIndex2 < tmpVecs.Count; veinPointIndex2++)
            {
                Vec3 vector5 = (vector * tmpVecs[veinPointIndex2].X + vector2 * tmpVecs[veinPointIndex2].Y) * num;
                int veinAmount = Mathf.RoundToInt(rnd2.Next(minValue, maxValue) * 1.1f);
                if (eVeinType2 != EVeinType.Oil)
                    veinAmount = Mathf.RoundToInt(veinAmount * galaxy.ResourceMultiplier);
                else
                {
                    // 官方 GameDesc.oilAmountMultiplier：资源倍率<=0.1001 时 0.5，否则 1.0
                    // 注意：官方无 2500 石油储量下限（C++ 移植版自加，此处以游戏为准）
                    float oilResourceMultiplier = galaxy.ResourceMultiplier <= 0.1001f ? 0.5f : 1.0f;
                    veinAmount = Mathf.RoundToInt(veinAmount * oilResourceMultiplier);
                }
                if (veinAmount < 1) veinAmount = 1;
                if (galaxy.ResourceMultiplier >= 100.0f && eVeinType2 != EVeinType.Oil) veinAmount = 1000000000;

                Vec3 veinPos = normalized + vector5;
                float num29 = algo.QueryHeight(veinPos);
                if (planet.WaterItemId == 0 || num29 >= NormalPlanetRadius || planet.AlgoId == 7)
                {
                    planet.VeinsPoint[veinPointType - 1]++;
                    planet.VeinsAmount[veinPointType - 1] += (ulong)veinAmount;
                }
            }
        }
    }
}
