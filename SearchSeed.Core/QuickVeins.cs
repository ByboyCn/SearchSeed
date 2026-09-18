using SearchSeed.Core.Data;
using SearchSeed.Core.Gen;
using SearchSeed.Core.Maths;

namespace SearchSeed.Core;

// Port of MyGenerateVeins (fast mode: theoretical upper bound of vein clusters)
public static class QuickVeins
{
    public static void Generate(GalaxyGen g, StarClass star, PlanetClass planet)
    {
        var themeProto = Ldb.Select(planet.Theme);
        var rnd = new DotNet35Random(planet.Seed);
        rnd.Next(); rnd.Next(); rnd.Next(); rnd.Next(); rnd.Next(); rnd.Next();

        int[] veinsGroup = new int[14];
        float[] veinsCountPercent = new float[14];
        float[] veinsAmountPercent = new float[14];

        for (int i = 0; i < themeProto.VeinSpot.Length; i++) veinsGroup[i] = themeProto.VeinSpot[i];
        for (int i = 0; i < themeProto.VeinCount.Length; i++) veinsCountPercent[i] = themeProto.VeinCount[i];
        for (int i = 0; i < themeProto.VeinOpacity.Length; i++) veinsAmountPercent[i] = themeProto.VeinOpacity[i];

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
            veinsGroup[8] += 2;
            for (int index = 1; index < 12 && rnd.NextDouble() < 0.449999988079071; ++index) ++veinsGroup[8];
            veinsCountPercent[8] = 0.7f; veinsAmountPercent[8] = 1.0f;
            veinsGroup[9] += 2;
            for (int index = 1; index < 12 && rnd.NextDouble() < 0.449999988079071; ++index) ++veinsGroup[9];
            veinsCountPercent[9] = 0.7f; veinsAmountPercent[9] = 1.0f;
            ++veinsGroup[11];
            for (int index = 1; index < 12 && rnd.NextDouble() < 0.5; ++index) ++veinsGroup[11];
            veinsCountPercent[11] = 0.7f; veinsAmountPercent[11] = 0.3f;
        }
        else if (star.Type == EStarType.NeutronStar)
        {
            p = 4.5f;
            ++veinsGroup[13];
            for (int index = 1; index < 12 && rnd.NextDouble() < 0.649999976158142; ++index) ++veinsGroup[13];
            veinsCountPercent[13] = 0.7f; veinsAmountPercent[13] = 0.3f;
        }
        else if (star.Type == EStarType.BlackHole)
        {
            p = 5.0f;
            ++veinsGroup[13];
            for (int index = 1; index < 12 && rnd.NextDouble() < 0.649999976158142; ++index) ++veinsGroup[13];
            veinsCountPercent[13] = 0.7f; veinsAmountPercent[13] = 0.3f;
        }

        for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
        {
            int rareVein = themeProto.RareVeins[index1];
            float rareSetting1 = star.Index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
            float rareSetting2 = themeProto.RareSettings[index1 * 4 + 2];
            float rareSetting3 = themeProto.RareSettings[index1 * 4 + 3];
            rareSetting1 = 1.0f - Mathf.Pow(1.0f - rareSetting1, p);
            rareSetting3 = 1.0f - Mathf.Pow(1.0f - rareSetting3, p);
            if (rnd.NextDouble() < rareSetting1)
            {
                ++veinsGroup[rareVein - 1];
                veinsCountPercent[rareVein - 1] = rareSetting3;
                veinsAmountPercent[rareVein - 1] = rareSetting3;
                for (int index2 = 1; index2 < 12 && rnd.NextDouble() < rareSetting2; ++index2)
                    ++veinsGroup[rareVein - 1];
            }
        }

        for (int i = 0; i < 14; i++)
        {
            if (veinsGroup[i] > 1) veinsGroup[i] += 1;
            planet.VeinsPoint[i] = Mathf.RoundToInt(veinsCountPercent[i] * 24.0f) * veinsGroup[i];
        }
        planet.VeinsPoint[6] = veinsGroup[6]; // 油井单独处理

        bool flag = g.BirthPlanetId == planet.Id;
        float num8 = star.ResourceCoef;
        if (flag) num8 *= 2.0f / 3.0f;
        else if (g.IsRareResource)
        {
            if (num8 > 1.0f) num8 = Mathf.Pow(num8, 0.8f);
            num8 *= 0.7f;
        }
        for (int i = 0; i < 14; i++)
        {
            planet.VeinsAmount[i] = (ulong)Mathf.RoundToInt(veinsAmountPercent[i] * 100000.0f * num8);
            if (planet.VeinsAmount[i] < 20) planet.VeinsAmount[i] = 20;
            planet.VeinsAmount[i] += planet.VeinsAmount[i] < 16000
                ? (ulong)Mathf.FloorToInt(planet.VeinsAmount[i] * 0.9375f)
                : 15000;
            planet.VeinsAmount[i] = (ulong)Mathf.RoundToInt(planet.VeinsAmount[i] * 1.1f);
            if (i == 6)
            {
                float oilMult = g.IsRareResource ? 0.5f : 1.0f;
                planet.VeinsAmount[i] = (ulong)Mathf.RoundToInt(planet.VeinsAmount[i] * oilMult);
                if (planet.VeinsAmount[i] < 2500) planet.VeinsAmount[i] = 2500;
            }
            else
                planet.VeinsAmount[i] = (ulong)Mathf.RoundToInt(planet.VeinsAmount[i] * g.ResourceMultiplier);
            if (planet.VeinsAmount[i] < 1) planet.VeinsAmount[i] = 1;
            planet.VeinsAmount[i] *= (ulong)planet.VeinsPoint[i];
        }
        if (g.ResourceMultiplier >= 100.0f)
        {
            for (int i = 0; i < 14; i++)
            {
                if (i == 6) continue;
                planet.VeinsAmount[i] = (ulong)planet.VeinsPoint[i] * 1000000000;
            }
        }
    }
}
