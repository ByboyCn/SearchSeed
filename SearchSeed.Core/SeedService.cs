using SearchSeed.Core.Data;
using SearchSeed.Core.Gen;

namespace SearchSeed.Core;

public class SeedService
{
    // 由宿主注入的持久化钩子（API 侧设置）
    public Func<int, int, int, bool, GalaxyResult, bool>? OnComputed { get; set; }
    public Func<int, int, int, bool, GalaxyResult?>? TryLoad { get; set; }

    static readonly Dictionary<(int, int, int, bool), GalaxyResult> Cache = new();

    public GalaxyResult GetGalaxy(int seedId, int starNum = 64, int resourceIndex = 4, bool fastMode = false)
    {
        var key = (seedId, starNum, resourceIndex, fastMode);
        lock (Cache)
        {
            if (Cache.TryGetValue(key, out var cached)) return cached;
        }
        var loaded = TryLoad?.Invoke(seedId, starNum, resourceIndex, fastMode);
        if (loaded != null)
        {
            lock (Cache) Cache[key] = loaded;
            return loaded;
        }
        var g = new GalaxyGen();
        g.CreateStars(seedId, starNum, ConstValue.ResourceRates[resourceIndex]);
        g.CreatePlanets();
        foreach (var star in g.Stars)
            foreach (var planet in star.Planets)
                planet.StarRef = star;

        if (fastMode)
        {
            foreach (var star in g.Stars)
                foreach (var planet in star.Planets)
                    if (planet.GasItems.Count == 0)
                        QuickVeins.Generate(g, star, planet);
        }
        else
        {
            PlanetAlgorithm.DoInit();
            var tasks = new List<(StarClass, PlanetClass)>();
            foreach (var star in g.Stars)
                foreach (var planet in star.Planets)
                    if (planet.GasItems.Count == 0)
                        tasks.Add((star, planet));
            Parallel.ForEach(tasks, t =>
            {
                var (star, planet) = t;
                var algo = PlanetAlgorithmFactory.Create(planet.AlgoId);
                algo.GenerateTerrain(planet, false);
                StandardVeins.Generate(algo, g, star, planet);
                planet.LandPercent = algo.CalcLandPercent(planet);
            });
        }

        var result = BuildResult(g, seedId, starNum, resourceIndex);
        OnComputed?.Invoke(seedId, starNum, resourceIndex, fastMode, result);
        lock (Cache)
        {
            if (Cache.Count > 200) Cache.Clear();
            Cache[key] = result;
        }
        return result;
    }

    static GalaxyResult BuildResult(GalaxyGen g, int seedId, int starNum, int resourceIndex)
    {
        var res = new GalaxyResult
        {
            SeedId = seedId, StarNum = starNum, ResourceIndex = resourceIndex,
            ResourceRate = g.ResourceMultiplier, BirthPlanetId = g.BirthPlanetId,
        };
        long[] galaxyVeinsPoint = new long[14];
        ulong[] galaxyVeinsAmount = new ulong[14];
        foreach (var star in g.Stars)
        {
            var sd = new StarResult
            {
                Index = star.Index, Name = star.Name, Type = star.TypeString, TypeId = star.TypeId(),
                Seed = star.Seed, Luminosity = star.Luminosity, DysonLumino = star.DysonLumino,
                DysonRadius = (float)(System.Math.Round(star.DysonRadius * 800) * 100),
                PosLy = [star.Position.X, star.Position.Y, star.Position.Z],
                Distance = (star.Position - g.Stars[0].Position).Magnitude,
                PlanetCount = star.PlanetCount,
                SafetyFactor = star.SafetyFactor, HivePatternLevel = star.HivePatternLevel,
                StarMass = star.Mass, Spectr = star.Spectr.ToString(),
                StarRadius = star.Radius, Temperature = star.Temperature, Age = star.Age,
            };
            long[] starVeins = new long[14]; ulong[] starAmount = new ulong[14];
            foreach (var planet in star.Planets)
            {
                var veinNames = new Dictionary<string, long>();
                var amountNames = new Dictionary<string, ulong>();
                for (int i = 0; i < 14; i++)
                {
                    if (planet.VeinsPoint[i] <= 0) continue;
                    string vn = VeinNames[i];
                    veinNames[vn] = planet.VeinsPoint[i];
                    amountNames[vn] = planet.VeinsAmount[i];
                    starVeins[i] += planet.VeinsPoint[i];
                    starAmount[i] += planet.VeinsAmount[i];
                    galaxyVeinsPoint[i] += planet.VeinsPoint[i];
                    galaxyVeinsAmount[i] += planet.VeinsAmount[i];
                }
                float needDot = 1.0f / 24.0f - 0.00002f * sd.DysonRadius / planet.MaxOrbitRadius;
                float rawDspDegree;
                if (needDot <= -1.0f) rawDspDegree = 0.0f;
                else if (needDot > 0.0f) rawDspDegree = 90.0f;
                else rawDspDegree = MathF.Min(MathF.Acos(-needDot) * 57.29578f + MathF.Abs(planet.Obliquity), 90.0f);

                sd.Planets.Add(new PlanetResult
                {
                    Index = planet.Index, Id = planet.Id, Name = planet.Name, Type = planet.DisplayName,
                    TypeId = planet.TypeId, Theme = planet.Theme, Seed = planet.Seed,
                    Singularity = planet.GetPlanetSingularityVector(),
                    Luminosity = planet.Luminosity, Wind = planet.WindStrength,
                    OrbitRadius = planet.OrbitRadius, Obliquity = planet.Obliquity,
                    IonHeight = planet.IonHeight, IsSatellite = planet.OrbitAround > 0,
                    Liquid = planet.WaterItemId > 0 ? LiquidNames.GetValueOrDefault(planet.WaterItemId, planet.WaterItemId.ToString()) : "",
                    RawDspDegree = rawDspDegree,
                    GasItems = planet.GasItems, GasSpeeds = planet.GasSpeeds,
                    Gas = planet.GasItems.Zip(planet.GasSpeeds, (id, sp) => GasNames.GetValueOrDefault(id, id.ToString()) + " " + sp.ToString("0.00") + "/s").ToList(),
                    VeinsPoint = veinNames, VeinsAmount = amountNames,
                    IsGas = planet.GasItems.Count > 0,
                    OrbitalPeriodSec = planet.OrbitalPeriod,
                    RotationPeriodSec = planet.RotationPeriod,
                    OrbitInclination = planet.OrbitInclination,
                    OrbitLongitude = planet.OrbitLongitude,
                    LandPercent = planet.GasItems.Count > 0 ? 0f : planet.LandPercent,
                });
                // 卫星挂到母行星下
                if (planet.OrbitAroundPlanet != null)
                {
                    var parent = sd.Planets.Find(q => q.Index == planet.OrbitAroundPlanet.Index);
                    if (parent != null) parent.Moons.Add(sd.Planets[^1]);
                }
            }
            foreach (var p in sd.Planets.Concat(sd.Planets.SelectMany(m => m.Moons)))
                if (!string.IsNullOrEmpty(p.Liquid) && !sd.Liquids.Contains(p.Liquid)) sd.Liquids.Add(p.Liquid);
            sd.VeinsPoint = ToDict(starVeins);
            sd.VeinsAmount = ToDictU(starAmount);
            res.Stars.Add(sd);
        }
        res.VeinsPoint = ToDict(galaxyVeinsPoint);
        res.VeinsAmount = ToDictU(galaxyVeinsAmount);
        return res;
    }

    public static readonly Dictionary<int, string> LiquidNames = new()
    {
        [1] = "水", [2] = "硫酸", [3] = "重氢", [4] = "液氢", [5] = "原油", [6] = "液态甲烷",
    };

    public static readonly Dictionary<int, string> GasNames = new()
    {
        [1120] = "氢", [1121] = "重氢", [1011] = "可燃冰", [1116] = "氢", [1117] = "重氢",
    };

    public static readonly string[] VeinNames =
        ["铁","铜","硅","钛","石","煤","油","可燃冰","金伯利矿石","分形硅石","有机晶体","光栅石","刺笋结晶","单极磁石"];

    static Dictionary<string, long> ToDict(long[] a)
    {
        var d = new Dictionary<string, long>();
        for (int i = 0; i < 14; i++) if (a[i] > 0) d[VeinNames[i]] = a[i];
        return d;
    }
    static Dictionary<string, ulong> ToDictU(ulong[] a)
    {
        var d = new Dictionary<string, ulong>();
        for (int i = 0; i < 14; i++) if (a[i] > 0) d[VeinNames[i]] = a[i];
        return d;
    }
}

public class GalaxyResult
{
    public int SeedId, StarNum, ResourceIndex;
    public float ResourceRate;
    public int BirthPlanetId;
    public Dictionary<string, long> VeinsPoint = new();
    public Dictionary<string, ulong> VeinsAmount = new();
    public List<StarResult> Stars = new();
    public int StarCount() => Stars.Count;
}

public class StarResult
{
    public int Index, Seed, TypeId, PlanetCount;
    public string Name = "", Type = "";
    public float Luminosity, DysonLumino, DysonRadius;
    public double Distance;
    public double[] PosLy = new double[3];
    public float SafetyFactor = 1.0f;
    public int HivePatternLevel;
    public float StarMass;          // 质量（太阳质量）
    public string Spectr = "";      // 光谱型
    public float StarRadius;        // 恒星半径（太阳半径）
    public float Temperature;       // 表面温度（K）
    public float Age;               // 年龄（占主序寿命比例）
    public List<string> Liquids = new();   // 该恒星系内出现的海洋类型
    public Dictionary<string, long> VeinsPoint = new();
    public Dictionary<string, ulong> VeinsAmount = new();
    public List<PlanetResult> Planets = new();
}

public class PlanetResult
{
    public int Index, Id, TypeId, Theme, Seed;
    public string Name = "", Type = "";
    public List<string> Singularity = new();
    public float Luminosity, Wind, OrbitRadius, Obliquity, IonHeight;
    public bool IsSatellite;
    public List<int> GasItems = new();
    public List<float> GasSpeeds = new();
    public List<string> Gas = new();
    public Dictionary<string, long> VeinsPoint = new();
    public Dictionary<string, ulong> VeinsAmount = new();
    public List<PlanetResult> Moons = new();
    public float RawDspDegree;
    public bool IsGas;
    public string Liquid = "";
    public double OrbitalPeriodSec;     // 公转周期（秒）
    public double RotationPeriodSec;    // 自转周期（秒，负值=反向自转）
    public float OrbitInclination;      // 轨道倾角（度）
    public float OrbitLongitude;        // 升交点经度（度）
    public float LandPercent = -1f;     // 适建区域（0~1，-1=快速模式未计算）
}