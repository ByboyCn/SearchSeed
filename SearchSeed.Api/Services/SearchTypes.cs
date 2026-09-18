using System.Collections.Concurrent;
using SearchSeed.Core;

namespace SearchSeed.Api.Services;

// ===== 条件树（对齐原项目：星区 → 恒星系 → 行星 → 卫星）=====

public class VeinCond
{
    public Dictionary<string, long> Point { get; set; } = new();
    public Dictionary<string, ulong> Amount { get; set; } = new();
}

public class PlanetCond
{
    public string Name { get; set; } = "行星条件";
    public bool Checked { get; set; } = true;
    public List<string> Types { get; set; } = new();
    public List<string> Singularity { get; set; } = new();
    public string Liquid { get; set; } = "";
    public float DspLevelMin { get; set; } = 0f;
    public int SatisfyNum { get; set; } = 1;
    public VeinCond Veins { get; set; } = new();
    public List<PlanetCond> Moons { get; set; } = new();
}

public class StarCond
{
    public string Name { get; set; } = "恒星系条件";
    public bool Checked { get; set; } = true;
    public List<string> Types { get; set; } = new();
    public float MinLumino { get; set; } = 0f;
    public float MaxDistance { get; set; } = -1f;
    public int SatisfyNum { get; set; } = 1;
    public VeinCond Veins { get; set; } = new();
    public List<PlanetCond> Planets { get; set; } = new();
}

public class GalaxyCond
{
    public VeinCond Veins { get; set; } = new();
    public List<StarCond> Stars { get; set; } = new();
    public List<PlanetCond> Planets { get; set; } = new();
}

// ===== 匹配 =====
public static class CondEval
{
    static bool VeinsOk(Dictionary<string, long> p, Dictionary<string, ulong> a, VeinCond c)
    {
        foreach (var (k, v) in c.Point)
            if (!p.TryGetValue(k, out var have) || have < v) return false;
        foreach (var (k, v) in c.Amount)
            if (!a.TryGetValue(k, out var have) || have < v) return false;
        return true;
    }

    public static bool CheckPlanet(PlanetResult p, PlanetCond c)
    {
        if (!c.Checked) return true;
        if (c.Types.Count > 0 && !c.Types.Contains(p.Type)) return false;
        foreach (var s in c.Singularity)
            if (!p.Singularity.Contains(s)) return false;
        if (!string.IsNullOrEmpty(c.Liquid) && p.Liquid != c.Liquid) return false;
        if (c.DspLevelMin > 0 && p.RawDspDegree < c.DspLevelMin) return false;
        if (!VeinsOk(p.VeinsPoint, p.VeinsAmount, c.Veins)) return false;
        foreach (var mc in c.Moons)
        {
            if (!mc.Checked) continue;
            int n = p.Moons.Count(m => CheckPlanet(m, mc));
            if (n < mc.SatisfyNum) return false;
        }
        return true;
    }

    public static bool CheckStar(StarResult s, StarCond c)
    {
        if (!c.Checked) return true;
        if (c.Types.Count > 0 && !c.Types.Contains(s.Type)) return false;
        if (c.MaxDistance >= 0 && s.Distance > c.MaxDistance) return false;
        if (s.DysonLumino < c.MinLumino) return false;
        if (!VeinsOk(s.VeinsPoint, s.VeinsAmount, c.Veins)) return false;
        foreach (var pc in c.Planets)
        {
            if (!pc.Checked) continue;
            int n = s.Planets.Count(p => !p.IsSatellite && CheckPlanet(p, pc));
            if (n < pc.SatisfyNum) return false;
        }
        return true;
    }

    public static bool Check(GalaxyResult g, GalaxyCond c, out List<StarResult> hitStars)
    {
        hitStars = new List<StarResult>();
        if (!VeinsOk(g.VeinsPoint, g.VeinsAmount, c.Veins)) return false;
        foreach (var sc in c.Stars)
        {
            if (!sc.Checked) continue;
            var hits = g.Stars.Where(s => CheckStar(s, sc)).ToList();
            if (hits.Count < sc.SatisfyNum) return false;
            hitStars.AddRange(hits.Take(3));
        }
        foreach (var pc in c.Planets)
        {
            if (!pc.Checked) continue;
            var all = g.Stars.SelectMany(s => s.Planets).ToList();
            int n = all.Count(p => CheckPlanet(p, pc));
            if (n < pc.SatisfyNum) return false;
        }
        return true;
    }
}

// ===== 任务 =====
public class SearchJob
{
    public string Id { get; } = Guid.NewGuid().ToString("N")[..12];
    public GalaxyCond Conditions { get; set; } = new();
    public int StarNum { get; set; } = 64;
    public int ResourceIndex { get; set; } = 4;
    public bool FastMode { get; set; } = true;
    public long FromSeed { get; set; }
    public volatile bool Stopped;
    public volatile bool Finished;
    public long Scanned;
    public long Skipped;
    public DateTime Started { get; } = DateTime.Now;
    public readonly ConcurrentQueue<SearchMatch> Matches = new();
    public int MaxResults { get; set; } = 100;
}

public class SearchMatch
{
    public int Seed { get; set; }
    public List<MatchedStar> Stars { get; set; } = new();
}

public class MatchedStar
{
    public int Index { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public double Distance { get; set; }
    public float DysonLumino { get; set; }
}
