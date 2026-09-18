using SearchSeed.Core.Data;
using SearchSeed.Core.Maths;

namespace SearchSeed.Core.Gen;

public class PlanetClass
{
    public StarClass? StarRef;
    public int Seed, InfoSeed, Id, Index, OrbitAround, Number, OrbitIndex;
    public string Name = "", OverrideName = "";
    public float OrbitRadius = 1.0f, MaxOrbitRadius, OrbitInclination, OrbitLongitude;
    public double OrbitalPeriod = 3600.0;
    public float OrbitPhase, Obliquity;
    public double RotationPeriod = 480.0;
    public float RotationPhase, Radius = 200.0f, Scale = 1.0f, SunDistance;
    public float HabitableBias, TemperatureBias, IonHeight, WindStrength, Luminosity, LandPercent;
    public double ModX, ModY;
    public float WaterHeight;
    public int WaterItemId;
    public bool Levelized;
    public int IceFlag;
    public EPlanetType Type;
    public EPlanetSingularity Singularity;
    public int Theme, AlgoId, Style;
    public PlanetClass? OrbitAroundPlanet;
    public List<int> GasItems = [];
    public List<float> GasSpeeds = [];
    public string DisplayName = "";
    public Quat RuntimeOrbitRotation, RuntimeSystemRotation;
    public Vec3 BirthPoint, BirthResourcePoint0, BirthResourcePoint1;
    public int[] VeinsPoint = new int[14];
    public ulong[] VeinsAmount = new ulong[14];
    public int TypeId;

    public float RealRadius => Radius * Scale;
    public float GetIonEnhance()
    {
        float realRadius = RealRadius;
        float temp = realRadius + IonHeight * 0.6f;
        return Mathf.Sqrt(temp * temp - realRadius * realRadius) / temp;
    }

    public List<string> GetPlanetSingularityVector()
    {
        var v = new List<string>();
        if (Singularity.HasFlag(EPlanetSingularity.TidalLocked)) v.Add("潮汐锁定永昼永夜");
        if (Singularity.HasFlag(EPlanetSingularity.TidalLocked2)) v.Add("轨道共振1:2");
        if (Singularity.HasFlag(EPlanetSingularity.TidalLocked4)) v.Add("轨道共振1:4");
        if (Singularity.HasFlag(EPlanetSingularity.LaySide)) v.Add("横躺自转");
        if (Singularity.HasFlag(EPlanetSingularity.ClockwiseRotate)) v.Add("反向自转");
        if (Singularity.HasFlag(EPlanetSingularity.MultipleSatellites)) v.Add("多卫星");
        if (Singularity.HasFlag(EPlanetSingularity.Satellite)) v.Add("卫星");
        return v;
    }
}

public class StarClass
{
    public static readonly string[] TypeNames = ["红巨星","黄巨星","蓝巨星","白巨星","白矮星","中子星","黑洞","A型恒星","B型恒星","F型恒星","G型恒星","K型恒星","M型恒星","O型恒星","未知恒星类型"];

    public int Seed, Index, Id;
    public string Name = "", OverrideName = "";
    public VecLF3 Position, UPosition;
    public float Mass = 1.0f, Lifetime = 50.0f, Age;
    public EStarType Type;
    public float Temperature = 8500.0f;
    public ESpectrType Spectr;
    public float ClassFactor, Color, Luminosity = 1.0f, Radius = 1.0f, AcdiskRadius;
    public float HabitableRadius = 1.0f, LightBalanceRadius = 1.0f, DysonRadius = 10.0f, OrbitScaler = 1.0f;
    public float AsterBelt1OrbitIndex, AsterBelt2OrbitIndex, AsterBelt1Radius, AsterBelt2Radius;
    public int PlanetCount;
    public float Level, ResourceCoef = 1.0f;
    public float SafetyFactor = 1.0f;   // 官方 safetyFactor，越高越安全
    public int HivePatternLevel;         // 0=安全(>=0.7) 1=中等(>=0.3) 2=高危

    public List<PlanetClass> Planets = [];

    public float PhysicsRadius => Radius * 1200;
    public float DysonLumino => Mathf.Round(Mathf.Pow(Luminosity, 0.33000001311302185f) * 1000.0f) / 1000.0f;
    public string TypeString => TypeNames[TypeId()];

    public int TypeId()
    {
        if (Type == EStarType.GiantStar)
        {
            if (Spectr <= ESpectrType.K) return 0;
            if (Spectr <= ESpectrType.F) return 1;
            if (Spectr != ESpectrType.A) return 2;
            return 3;
        }
        if (Type == EStarType.WhiteDwarf) return 4;
        if (Type == EStarType.NeutronStar) return 5;
        if (Type == EStarType.BlackHole) return 6;
        if (Type == EStarType.MainSeqStar)
        {
            return Spectr switch
            {
                ESpectrType.A => 7, ESpectrType.B => 8, ESpectrType.F => 9, ESpectrType.G => 10,
                ESpectrType.K => 11, ESpectrType.M => 12, ESpectrType.O => 13, _ => 14
            };
        }
        return 14;
    }
}