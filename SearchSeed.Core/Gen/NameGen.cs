using SearchSeed.Core.Data;

namespace SearchSeed.Core.Gen;

public static class NameGen
{
    public static string RandomName(int seed)
    {
        var rnd = new DotNet35Random(seed);
        int num = (int)(rnd.NextDouble() * 1.8 + 2.3);
        string text = "";
        for (int i = 0; i < num; i++)
        {
            if (!(rnd.NextDouble() < 0.05000000074505806) || i != 0)
            {
                text = (!(rnd.NextDouble() < 0.97000002861022949) && num < 4)
                    ? text + NameTables.con1[rnd.Next(NameTables.con1.Length)]
                    : text + NameTables.con0[rnd.Next(NameTables.con0.Length)];
                text = (i == num - 1 && rnd.NextDouble() < 0.89999997615814209)
                    ? text + NameTables.ending[rnd.Next(NameTables.ending.Length)]
                    : (!(rnd.NextDouble() < 0.97000002861022949)
                        ? text + NameTables.vow2[rnd.Next(NameTables.vow2.Length)]
                        : text + NameTables.vow1[rnd.Next(NameTables.vow1.Length)]);
            }
            else
            {
                text += NameTables.vow0[rnd.Next(NameTables.vow0.Length)];
            }
        }
        text = text.Replace("uu", "u").Replace("ooo", "oo").Replace("eee", "ee").Replace("eea", "ea")
            .Replace("aa", "a").Replace("yy", "y");
        return char.ToUpper(text[0]) + text[1..];
    }

    public static string RandomStarName(int seed, StarClass starData, HashSet<string> starnames)
    {
        var rnd = new DotNet35Random(seed);
        for (int i = 0; i < 256; i++)
        {
            string text = _RandomStarName(rnd.Next(), starData);
            if (!starnames.Add(text)) continue;
            return text;
        }
        return "XStar";
    }

    static string _RandomStarName(int seed, StarClass starData)
    {
        var rnd = new DotNet35Random(seed);
        int seed2 = rnd.Next();
        double num = rnd.NextDouble();
        double num2 = rnd.NextDouble();
        if (starData.Type == EStarType.GiantStar)
        {
            if (num2 < 0.40000000596046448) return RandomGiantStarNameFromRawNames(seed2);
            if (num2 < 0.699999988079071) return RandomGiantStarNameWithConstellationAlpha(seed2);
            return RandomGiantStarNameWithFormat(seed2);
        }
        if (starData.Type == EStarType.NeutronStar) return RandomNeutronStarNameWithFormat(seed2);
        if (starData.Type == EStarType.BlackHole) return RandomBlackHoleNameWithFormat(seed2);
        if (num < 0.60000002384185791) return RandomStarNameFromRawNames(seed2);
        if (num < 0.93000000715255737) return RandomStarNameWithConstellationAlpha(seed2);
        return RandomStarNameWithConstellationNumber(seed2);
    }

    static string RandomStarNameFromRawNames(int seed) => NameTables.raw_star_names[new DotNet35Random(seed).Next() % NameTables.raw_star_names.Length];

    static string RandomStarNameWithConstellationAlpha(int seed)
    {
        var rnd = new DotNet35Random(seed);
        int num = rnd.Next() % NameTables.constellations.Length;
        int num2 = rnd.Next() % NameTables.alphabeta.Length;
        string text = NameTables.constellations[num];
        if (text.Length > 10) return NameTables.alphabeta_letter[num2] + " " + text;
        return NameTables.alphabeta[num2] + " " + text;
    }

    static string RandomStarNameWithConstellationNumber(int seed)
    {
        var rnd = new DotNet35Random(seed);
        int num = rnd.Next() % NameTables.constellations.Length;
        int num2 = rnd.Next(27, 75);
        return num2 + " " + NameTables.constellations[num];
    }

    static string RandomGiantStarNameFromRawNames(int seed) => NameTables.raw_giant_names[new DotNet35Random(seed).Next() % NameTables.raw_giant_names.Length];

    static string RandomGiantStarNameWithConstellationAlpha(int seed)
    {
        var rnd = new DotNet35Random(seed);
        int num = rnd.Next() % NameTables.constellations.Length;
        int num2 = rnd.Next(15, 26);
        int num3 = rnd.Next(0, 26);
        ushort num4 = (ushort)(65 + num2);
        char c = (char)(65 + num3);
        return (num4 + c) + " " + NameTables.constellations[num];
    }

    static string RandomGiantStarNameWithFormat(int seed)
    {
        var rnd = new DotNet35Random(seed);
        int num = rnd.Next() % NameTables.giant_name_formats.Length;
        int num2 = rnd.Next(10000);
        int num3 = rnd.Next(100);
        return NameTables.giant_name_formats[num] switch
        {
            "HD %04d%02d" => $"HD {num2:D4}{num3:D2}",
            "HDE %04d%02d" => $"HDE {num2:D4}{num3:D2}",
            "HR %04d" => $"HR {num2:D4}",
            "HV %04d" => $"HV {num2:D4}",
            "LBV %04d-%02d" => $"LBV {num2:D4}-{num3:D2}",
            "NSV %04d" => $"NSV {num2:D4}",
            _ => $"YSC {num2:D4}-{num3:D2}"
        };
    }

    static string RandomNeutronStarNameWithFormat(int seed)
    {
        var rnd = new DotNet35Random(seed);
        int num = rnd.Next() % NameTables.neutron_star_name_formats.Length;
        int num2 = rnd.Next(24), num3 = rnd.Next(60), num4 = rnd.Next(0, 60);
        return NameTables.neutron_star_name_formats[num] == "NTR J%02d%02d+%02d"
            ? $"NTR J{num2:D2}{num3:D2}+{num4:D2}"
            : $"NTR J{num2:D2}{num3:D2}-{num4:D2}";
    }

    static string RandomBlackHoleNameWithFormat(int seed)
    {
        var rnd = new DotNet35Random(seed);
        int num = rnd.Next() % NameTables.black_hole_name_formats.Length;
        int num2 = rnd.Next(24), num3 = rnd.Next(60), num4 = rnd.Next(0, 60);
        return NameTables.black_hole_name_formats[num] == "DSR J%02d%02d+%02d"
            ? $"DSR J{num2:D2}{num3:D2}+{num4:D2}"
            : $"DSR J{num2:D2}{num3:D2}-{num4:D2}";
    }
}
