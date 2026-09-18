using SearchSeed.Core.Data;
using SearchSeed.Core.Maths;

namespace SearchSeed.Core.Gen;

// Port of DSPGen.hpp (GalaxyClass) - Original 2022 Copyright https://github.com/crazyyao0.
// Modified by https://github.com/botany233 on 2025.10, C# port 2025.
public class GalaxyGen
{
    public int Seed, StarCount;
    public float ResourceMultiplier;
    public bool IsRareResource, IsInfiniteResource;
    public List<StarClass> Stars = [];
    public int HabitableCount;
    public int BirthPlanetId;
    public HashSet<string> Starnames = [];

    void SetPlanetTheme(StarClass star, PlanetClass planet, double rand1, double rand2, double rand3, double rand4, int themeSeed)
    {
        var tmpTheme = new List<int>(25);
        int[] themeIds = [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25];
        int length1 = themeIds.Length;
        for (int index1 = 0; index1 < length1; ++index1)
        {
            var themeProto = Ldb.Select(themeIds[index1]);
            bool flag1 = false;
            if (star.Index == 0 && planet.Type == EPlanetType.Ocean)
            {
                if (themeProto.Distribute == EThemeDistribute.Birth) flag1 = true;
            }
            else
            {
                bool flag2 = (double)themeProto.Temperature * planet.TemperatureBias >= -0.100000001490116;
                if (Mathf.Abs(themeProto.Temperature) < 0.5f && themeProto.PlanetType == EPlanetType.Desert)
                    flag2 = System.Math.Abs(planet.TemperatureBias) < System.Math.Abs(themeProto.Temperature) + 0.100000001490116;
                if (themeProto.PlanetType == planet.Type && flag2)
                {
                    if (star.Index == 0)
                    {
                        if (themeProto.Distribute == EThemeDistribute.Default) flag1 = true;
                    }
                    else if (themeProto.Distribute is EThemeDistribute.Default or EThemeDistribute.Interstellar)
                        flag1 = true;
                }
            }
            if (flag1)
            {
                for (int index2 = 0; index2 < planet.Index; ++index2)
                {
                    if (star.Planets[index2].Theme == themeProto.ID) { flag1 = false; break; }
                }
            }
            if (flag1) tmpTheme.Add(themeProto.ID);
        }
        if (tmpTheme.Count == 0)
        {
            for (int index3 = 0; index3 < length1; ++index3)
            {
                var themeProto = Ldb.Select(themeIds[index3]);
                bool flag = themeProto.PlanetType == EPlanetType.Desert;
                if (flag)
                {
                    for (int index4 = 0; index4 < planet.Index; ++index4)
                    {
                        if (star.Planets[index4].Theme == themeProto.ID) { flag = false; break; }
                    }
                }
                if (flag) tmpTheme.Add(themeProto.ID);
            }
        }
        if (tmpTheme.Count == 0)
        {
            for (int index = 0; index < length1; ++index)
            {
                var themeProto = Ldb.Select(themeIds[index]);
                if (themeProto.PlanetType == EPlanetType.Desert) tmpTheme.Add(themeProto.ID);
            }
        }
        planet.Theme = tmpTheme[(int)(rand1 * tmpTheme.Count) % tmpTheme.Count];
        var themeProto1 = Ldb.Select(planet.Theme);
        planet.TypeId = themeProto1.TypeId;
        planet.AlgoId = themeProto1.Algos[(int)(rand2 * themeProto1.Algos.Length) % themeProto1.Algos.Length];
        planet.ModX = themeProto1.ModX.X + rand3 * (themeProto1.ModX.Y - themeProto1.ModX.X);
        planet.ModY = themeProto1.ModY.X + rand4 * (themeProto1.ModY.Y - themeProto1.ModY.X);

        planet.DisplayName = themeProto1.DisplayName;
        planet.Style = themeSeed % 60;
        planet.Type = themeProto1.PlanetType;
        planet.IonHeight = themeProto1.IonHeight;
        planet.WindStrength = themeProto1.Wind;
        planet.WaterHeight = themeProto1.WaterHeight;
        planet.WaterItemId = themeProto1.WaterItemId;
        planet.Levelized = themeProto1.UseHeightForBuild;
        if (themeProto1.Distribute == EThemeDistribute.Birth) BirthPlanetId = planet.Id;
        if (planet.Type != EPlanetType.Gas) return;
        var rnd = new DotNet35Random(themeSeed);
        for (int index = 0; index < themeProto1.GasSpeeds.Length; ++index)
        {
            float num2 = themeProto1.GasSpeeds[index] * (float)(rnd.NextDouble() * 0.190909147262573 + 0.909090876579285);
            num2 *= IsRareResource ? 0.8f : 1.0f;
            planet.GasItems.Add(themeProto1.GasItems[index]);
            planet.GasSpeeds.Add(num2 * Mathf.Pow(star.ResourceCoef, 0.3f));
        }
    }

    PlanetClass CreatePlanet(StarClass star, int index, int orbitAround, int orbitIndex, int number, bool gasGiant, int infoSeed, int genSeed)
    {
        var planet = star.Planets[index];
        var rnd = new DotNet35Random(infoSeed);
        planet.Index = index;
        planet.Seed = genSeed;
        planet.InfoSeed = infoSeed;
        planet.OrbitAround = orbitAround;
        planet.OrbitIndex = orbitIndex;
        planet.Number = number;
        planet.Id = star.Id * 100 + index + 1;

        int num1 = 0;
        for (int index1 = 0; index1 < star.Index; ++index1) num1 += Stars[index1].PlanetCount;
        int num2 = num1 + index;
        if (orbitAround > 0)
        {
            planet.Singularity |= EPlanetSingularity.Satellite;
            for (int index2 = 0; index2 < star.PlanetCount; ++index2)
            {
                if (orbitAround == star.Planets[index2].Number && star.Planets[index2].OrbitAround == 0)
                {
                    planet.OrbitAroundPlanet = star.Planets[index2];
                    if (orbitIndex > 1)
                    {
                        planet.OrbitAroundPlanet.Singularity |= EPlanetSingularity.MultipleSatellites;
                        break;
                    }
                    break;
                }
            }
        }
        planet.Name = star.Name + " " + NameTables.roman[index + 1];
        double num3 = rnd.NextDouble();
        double num4 = rnd.NextDouble();
        double num5 = rnd.NextDouble();
        double num6 = rnd.NextDouble();
        double num7 = rnd.NextDouble();
        double num8 = rnd.NextDouble();
        double num9 = rnd.NextDouble();
        double num10 = rnd.NextDouble();
        double num11 = rnd.NextDouble();
        double num12 = rnd.NextDouble();
        double num13 = rnd.NextDouble();
        double num14 = rnd.NextDouble();
        double rand1 = rnd.NextDouble();
        double num15 = rnd.NextDouble();
        double rand2 = rnd.NextDouble();
        double rand3 = rnd.NextDouble();
        double rand4 = rnd.NextDouble();
        int themeSeed = rnd.Next();
        float a = Mathf.Pow(1.2f, (float)(num3 * (num4 - 0.5) * 0.5));
        float f1;
        if (orbitAround == 0)
        {
            float b = ConstValue.OrbitRadius[orbitIndex] * star.OrbitScaler;
            float num16 = (float)(((double)a - 1.0) / (double)Mathf.Max(1.0f, b) + 1.0);
            f1 = b * num16;
        }
        else
            f1 = (float)(((1600.0 * orbitIndex + 200.0) * (double)Mathf.Pow(star.OrbitScaler, 0.3f) * (double)Mathf.Lerp(a, 1.0f, 0.5f) + (double)planet.OrbitAroundPlanet!.RealRadius) / 40000.0);
        planet.OrbitRadius = f1;
        planet.OrbitInclination = (float)(num5 * 16.0 - 8.0);
        if (orbitAround > 0) planet.OrbitInclination *= 2.2f;
        planet.OrbitLongitude = (float)(num6 * 360.0);
        if (star.Type >= EStarType.NeutronStar)
        {
            if (planet.OrbitInclination > 0.0) planet.OrbitInclination += 3.0f;
            else planet.OrbitInclination -= 3.0f;
        }
        planet.OrbitalPeriod = planet.OrbitAroundPlanet != null
            ? System.Math.Sqrt(39.4784176043574 * (double)f1 * (double)f1 * (double)f1 / 1.08308421068537E-08)
            : System.Math.Sqrt(39.4784176043574 * (double)f1 * (double)f1 * (double)f1 / (1.35385519905204E-06 * star.Mass));
        planet.OrbitPhase = (float)(num7 * 360.0);
        if (num15 < 0.0399999991059303)
        {
            planet.Obliquity = (float)(num8 * (num9 - 0.5) * 39.9);
            if (planet.Obliquity < 0.0) planet.Obliquity -= 70.0f;
            else planet.Obliquity += 70.0f;
            planet.Singularity |= EPlanetSingularity.LaySide;
        }
        else if (num15 < 0.100000001490116)
        {
            planet.Obliquity = (float)(num8 * (num9 - 0.5) * 80.0);
            if (planet.Obliquity < 0.0) planet.Obliquity -= 30.0f;
            else planet.Obliquity += 30.0f;
        }
        else
            planet.Obliquity = (float)(num8 * (num9 - 0.5) * 60.0);
        planet.RotationPeriod = (num10 * num11 * 1000.0 + 400.0) * (orbitAround == 0 ? (double)Mathf.Pow(f1, 0.25f) : 1.0) * (gasGiant ? 0.200000002980232 : 1.0);
        if (!gasGiant)
        {
            if (star.Type == EStarType.WhiteDwarf) planet.RotationPeriod *= 0.5;
            else if (star.Type == EStarType.NeutronStar) planet.RotationPeriod *= 0.200000002980232;
            else if (star.Type == EStarType.BlackHole) planet.RotationPeriod *= 0.150000005960464;
        }
        planet.RotationPhase = (float)(num12 * 360.0);
        planet.SunDistance = orbitAround == 0 ? planet.OrbitRadius : planet.OrbitAroundPlanet!.OrbitRadius;
        planet.Scale = 1.0f;
        planet.MaxOrbitRadius = orbitAround == 0 ? planet.OrbitRadius : planet.OrbitRadius + planet.SunDistance;

        double num17 = orbitAround == 0 ? planet.OrbitalPeriod : planet.OrbitAroundPlanet!.OrbitalPeriod;
        planet.RotationPeriod = 1.0 / (1.0 / num17 + 1.0 / planet.RotationPeriod);
        if (orbitAround == 0 && orbitIndex <= 4 && !gasGiant)
        {
            if (num15 > 0.959999978542328)
            {
                planet.Obliquity *= 0.01f;
                planet.RotationPeriod = planet.OrbitalPeriod;
                planet.Singularity |= EPlanetSingularity.TidalLocked;
            }
            else if (num15 > 0.930000007152557)
            {
                planet.Obliquity *= 0.1f;
                planet.RotationPeriod = planet.OrbitalPeriod * 0.5;
                planet.Singularity |= EPlanetSingularity.TidalLocked2;
            }
            else if (num15 > 0.899999976158142)
            {
                planet.Obliquity *= 0.2f;
                planet.RotationPeriod = planet.OrbitalPeriod * 0.25;
                planet.Singularity |= EPlanetSingularity.TidalLocked4;
            }
        }
        if (num15 > 0.85 && num15 <= 0.9)
        {
            planet.RotationPeriod = -planet.RotationPeriod;
            planet.Singularity |= EPlanetSingularity.ClockwiseRotate;
        }
        planet.RuntimeOrbitRotation = Quat.AngleAxis(planet.OrbitLongitude, Vec3.Up) * Quat.AngleAxis(planet.OrbitInclination, Vec3.Forward);
        if (planet.OrbitAroundPlanet != null)
            planet.RuntimeOrbitRotation = planet.OrbitAroundPlanet.RuntimeOrbitRotation * planet.RuntimeOrbitRotation;
        planet.RuntimeSystemRotation = planet.RuntimeOrbitRotation * Quat.AngleAxis(planet.Obliquity, Vec3.Forward);
        float habitableRadius = star.HabitableRadius;
        if (gasGiant)
        {
            planet.Type = EPlanetType.Gas;
            planet.Radius = 80.0f;
            planet.Scale = 10.0f;
            planet.HabitableBias = 100.0f;
        }
        else
        {
            float num18 = Mathf.Ceil(StarCount * 0.29f);
            if (num18 < 11.0) num18 = 11.0f;
            double num19 = num18 - HabitableCount;
            float num20 = StarCount - star.Index;
            float sunDistance = planet.SunDistance;
            float num21 = 1000.0f;
            float f2 = 1000.0f;
            if (habitableRadius > 0.0 && sunDistance > 0.0)
            {
                f2 = sunDistance / habitableRadius;
                num21 = Mathf.Abs(Mathf.Log(f2));
            }
            float num22 = Mathf.Clamp(Mathf.Sqrt(habitableRadius), 1.0f, 2.0f) - 0.04f;
            double num23 = num20;
            float num24 = Mathf.Clamp(Mathf.Lerp((float)(num19 / num23), 0.35f, 0.5f), 0.08f, 0.8f);
            planet.HabitableBias = num21 * num22;
            planet.TemperatureBias = (float)(1.20000004768372 / ((double)f2 + 0.200000002980232) - 1.0);
            float num25 = Mathf.Pow(Mathf.Clamp01(planet.HabitableBias / num24), num24 * 10.0f);
            if (num13 > num25 && star.Index > 0 || planet.OrbitAround > 0 && planet.OrbitIndex == 1 && star.Index == 0)
            {
                planet.Type = EPlanetType.Ocean;
                ++HabitableCount;
            }
            else if (f2 < 0.833333015441895)
            {
                float num26 = Mathf.Max(0.15f, (float)((double)f2 * 2.5 - 0.850000023841858));
                planet.Type = num14 >= num26 ? EPlanetType.Vocano : EPlanetType.Desert;
            }
            else if (f2 < 1.20000004768372)
                planet.Type = EPlanetType.Desert;
            else
            {
                float num27 = (float)(0.899999976158142 / f2 - 0.100000001490116);
                planet.Type = num14 >= num27 ? EPlanetType.Ice : EPlanetType.Desert;
            }
            planet.Radius = 200.0f;
        }
        planet.Luminosity = Mathf.Pow(star.LightBalanceRadius / (planet.SunDistance + 0.01f), 0.6f);
        if (planet.Luminosity > 1.0)
        {
            planet.Luminosity = Mathf.Log(planet.Luminosity) + 1.0f;
            planet.Luminosity = Mathf.Log(planet.Luminosity) + 1.0f;
            planet.Luminosity = Mathf.Log(planet.Luminosity) + 1.0f;
        }
        planet.Luminosity = Mathf.Round(planet.Luminosity * 100.0f) / 100.0f;
        SetPlanetTheme(star, planet, rand1, rand2, rand3, rand4, themeSeed);
        return planet;
    }

    void CreateStarPlanets(StarClass star)
    {
        var rnd1 = new DotNet35Random(star.Seed);
        rnd1.Next(); rnd1.Next(); rnd1.Next();
        var rnd2 = new DotNet35Random(rnd1.Next());
        double num1 = rnd2.NextDouble();
        double num2 = rnd2.NextDouble();
        double num3 = rnd2.NextDouble();
        double num4 = rnd2.NextDouble();
        double num5 = rnd2.NextDouble();
        double num6 = rnd2.NextDouble() * 0.2 + 0.9;
        double num7 = rnd2.NextDouble() * 0.2 + 0.9;
        if (star.Type is EStarType.BlackHole or EStarType.NeutronStar)
        {
            star.PlanetCount = 1;
            star.Planets = new List<PlanetClass>(star.PlanetCount);
            star.Planets.Add(new PlanetClass());
            star.Planets[0] = CreatePlanet(star, 0, 0, 3, 1, false, rnd2.Next(), rnd2.Next());
        }
        else if (star.Type == EStarType.WhiteDwarf)
        {
            if (num1 < 0.699999988079071)
            {
                star.PlanetCount = 1;
                star.Planets.Add(new PlanetClass());
                star.Planets[0] = CreatePlanet(star, 0, 0, 3, 1, false, rnd2.Next(), rnd2.Next());
            }
            else
            {
                star.PlanetCount = 2;
                star.Planets.Add(new PlanetClass()); star.Planets.Add(new PlanetClass());
                if (num2 < 0.300000011920929)
                {
                    star.Planets[0] = CreatePlanet(star, 0, 0, 3, 1, false, rnd2.Next(), rnd2.Next());
                    star.Planets[1] = CreatePlanet(star, 1, 0, 4, 2, false, rnd2.Next(), rnd2.Next());
                }
                else
                {
                    star.Planets[0] = CreatePlanet(star, 0, 0, 4, 1, true, rnd2.Next(), rnd2.Next());
                    star.Planets[1] = CreatePlanet(star, 1, 1, 1, 1, false, rnd2.Next(), rnd2.Next());
                }
            }
        }
        else if (star.Type == EStarType.GiantStar)
        {
            if (num1 < 0.300000011920929)
            {
                star.PlanetCount = 1;
                star.Planets.Add(new PlanetClass());
                star.Planets[0] = CreatePlanet(star, 0, 0, num3 > 0.5 ? 3 : 2, 1, false, rnd2.Next(), rnd2.Next());
            }
            else if (num1 < 0.800000011920929)
            {
                star.PlanetCount = 2;
                star.Planets.Add(new PlanetClass()); star.Planets.Add(new PlanetClass());
                if (num2 < 0.25)
                {
                    star.Planets[0] = CreatePlanet(star, 0, 0, num3 > 0.5 ? 3 : 2, 1, false, rnd2.Next(), rnd2.Next());
                    star.Planets[1] = CreatePlanet(star, 1, 0, num3 > 0.5 ? 4 : 3, 2, false, rnd2.Next(), rnd2.Next());
                }
                else
                {
                    star.Planets[0] = CreatePlanet(star, 0, 0, 3, 1, true, rnd2.Next(), rnd2.Next());
                    star.Planets[1] = CreatePlanet(star, 1, 1, 1, 1, false, rnd2.Next(), rnd2.Next());
                }
            }
            else
            {
                star.PlanetCount = 3;
                star.Planets.Add(new PlanetClass()); star.Planets.Add(new PlanetClass()); star.Planets.Add(new PlanetClass());
                if (num2 < 0.150000005960464)
                {
                    star.Planets[0] = CreatePlanet(star, 0, 0, num3 > 0.5 ? 3 : 2, 1, false, rnd2.Next(), rnd2.Next());
                    star.Planets[1] = CreatePlanet(star, 1, 0, num3 > 0.5 ? 4 : 3, 2, false, rnd2.Next(), rnd2.Next());
                    star.Planets[2] = CreatePlanet(star, 2, 0, num3 > 0.5 ? 5 : 4, 3, false, rnd2.Next(), rnd2.Next());
                }
                else if (num2 < 0.75)
                {
                    star.Planets[0] = CreatePlanet(star, 0, 0, num3 > 0.5 ? 3 : 2, 1, false, rnd2.Next(), rnd2.Next());
                    star.Planets[1] = CreatePlanet(star, 1, 0, 4, 2, true, rnd2.Next(), rnd2.Next());
                    star.Planets[2] = CreatePlanet(star, 2, 2, 1, 1, false, rnd2.Next(), rnd2.Next());
                }
                else
                {
                    star.Planets[0] = CreatePlanet(star, 0, 0, num3 > 0.5 ? 4 : 3, 1, true, rnd2.Next(), rnd2.Next());
                    star.Planets[1] = CreatePlanet(star, 1, 1, 1, 1, false, rnd2.Next(), rnd2.Next());
                    star.Planets[2] = CreatePlanet(star, 2, 1, 2, 2, false, rnd2.Next(), rnd2.Next());
                }
            }
        }
        else
        {
            var pGas = new double[6];
            if (star.Index == 0)
            {
                star.PlanetCount = 4;
                pGas[0] = 0.0; pGas[1] = 0.0; pGas[2] = 0.0;
            }
            else if (star.Spectr == ESpectrType.M)
            {
                star.PlanetCount = num1 >= 0.1 ? (num1 >= 0.3 ? (num1 >= 0.8 ? 4 : 3) : 2) : 1;
                if (star.PlanetCount <= 3) { pGas[0] = 0.2; pGas[1] = 0.2; }
                else { pGas[0] = 0.0; pGas[1] = 0.2; pGas[2] = 0.3; }
            }
            else if (star.Spectr == ESpectrType.K)
            {
                star.PlanetCount = num1 >= 0.1 ? (num1 >= 0.2 ? (num1 >= 0.7 ? (num1 >= 0.95 ? 5 : 4) : 3) : 2) : 1;
                if (star.PlanetCount <= 3) { pGas[0] = 0.18; pGas[1] = 0.18; }
                else { pGas[0] = 0.0; pGas[1] = 0.18; pGas[2] = 0.28; pGas[3] = 0.28; }
            }
            else if (star.Spectr == ESpectrType.G)
            {
                star.PlanetCount = num1 >= 0.4 ? (num1 >= 0.9 ? 5 : 4) : 3;
                if (star.PlanetCount <= 3) { pGas[0] = 0.18; pGas[1] = 0.18; }
                else { pGas[0] = 0.0; pGas[1] = 0.2; pGas[2] = 0.3; pGas[3] = 0.3; }
            }
            else if (star.Spectr == ESpectrType.F)
            {
                star.PlanetCount = num1 >= 0.35 ? (num1 >= 0.8 ? 5 : 4) : 3;
                if (star.PlanetCount <= 3) { pGas[0] = 0.2; pGas[1] = 0.2; }
                else { pGas[0] = 0.0; pGas[1] = 0.22; pGas[2] = 0.31; pGas[3] = 0.31; }
            }
            else if (star.Spectr == ESpectrType.A)
            {
                star.PlanetCount = num1 >= 0.3 ? (num1 >= 0.75 ? 5 : 4) : 3;
                if (star.PlanetCount <= 3) { pGas[0] = 0.2; pGas[1] = 0.2; }
                else { pGas[0] = 0.1; pGas[1] = 0.28; pGas[2] = 0.3; pGas[3] = 0.35; }
            }
            else if (star.Spectr == ESpectrType.B)
            {
                star.PlanetCount = num1 >= 0.3 ? (num1 >= 0.75 ? 6 : 5) : 4;
                if (star.PlanetCount <= 3) { pGas[0] = 0.2; pGas[1] = 0.2; }
                else { pGas[0] = 0.1; pGas[1] = 0.22; pGas[2] = 0.28; pGas[3] = 0.35; pGas[4] = 0.35; }
            }
            else if (star.Spectr == ESpectrType.O)
            {
                star.PlanetCount = num1 >= 0.5 ? 6 : 5;
                pGas[0] = 0.1; pGas[1] = 0.2; pGas[2] = 0.25; pGas[3] = 0.3; pGas[4] = 0.32; pGas[5] = 0.35;
            }
            else star.PlanetCount = 1;
            star.Planets = new List<PlanetClass>(star.PlanetCount);
            for (int i = 0; i < star.PlanetCount; i++) star.Planets.Add(new PlanetClass());
            int num8 = 0, num9 = 0, orbitAround = 0, num10 = 1;
            for (int index = 0; index < star.PlanetCount; ++index)
            {
                int infoSeed = rnd2.Next();
                int genSeed = rnd2.Next();
                double num11 = rnd2.NextDouble();
                double num12 = rnd2.NextDouble();
                bool gasGiant = false;
                if (orbitAround == 0)
                {
                    ++num8;
                    if (index < star.PlanetCount - 1 && num11 < pGas[index])
                    {
                        gasGiant = true;
                        if (num10 < 3) num10 = 3;
                    }
                    for (; star.Index != 0 || num10 != 3; ++num10)
                    {
                        int num13 = star.PlanetCount - index;
                        int num14 = 9 - num10;
                        if (num14 > num13)
                        {
                            float a = (float)num13 / num14;
                            float num15 = num10 <= 3 ? Mathf.Lerp(a, 1.0f, 0.15f) + 0.01f : Mathf.Lerp(a, 1.0f, 0.45f) + 0.01f;
                            if (rnd2.NextDouble() < num15) goto label62;
                        }
                        else goto label62;
                    }
                    gasGiant = true;
                }
                else
                {
                    ++num9;
                    gasGiant = false;
                }
            label62:
                star.Planets[index] = CreatePlanet(star, index, orbitAround, orbitAround == 0 ? num10 : num9, orbitAround == 0 ? num8 : num9, gasGiant, infoSeed, genSeed);
                ++num10;
                if (gasGiant) { orbitAround = num8; num9 = 0; }
                if (num9 >= 1 && num12 < 0.8) { orbitAround = 0; num9 = 0; }
            }
        }
        int num16 = 0, num17 = 0, index1 = 0;
        for (int index2 = 0; index2 < star.PlanetCount; ++index2)
        {
            if (star.Planets[index2].Type == EPlanetType.Gas) { num16 = star.Planets[index2].OrbitIndex; break; }
        }
        for (int index3 = 0; index3 < star.PlanetCount; ++index3)
        {
            if (star.Planets[index3].OrbitAround == 0) num17 = star.Planets[index3].OrbitIndex;
        }
        if (num16 > 0)
        {
            int num18 = num16 - 1;
            bool flag = true;
            for (int index4 = 0; index4 < star.PlanetCount; ++index4)
            {
                if (star.Planets[index4].OrbitAround == 0 && star.Planets[index4].OrbitIndex == num16 - 1) { flag = false; break; }
            }
            if (flag && num4 < 0.2 + (double)num18 * 0.2) index1 = num18;
        }
        int index5 = num5 >= 0.2 ? (num5 >= 0.4 ? (num5 >= 0.8 ? 0 : num17 + 1) : num17 + 2) : num17 + 3;
        if (index5 != 0 && index5 < 5) index5 = 5;

        star.AsterBelt1OrbitIndex = index1;
        star.AsterBelt2OrbitIndex = index5;
        if (index1 > 0) star.AsterBelt1Radius = ConstValue.OrbitRadius[index1] * (float)num6 * star.OrbitScaler;
        if (index5 > 0) star.AsterBelt2Radius = ConstValue.OrbitRadius[index5] * (float)num7 * star.OrbitScaler;
    }

    static float RandNormal(float averageValue, float standardDeviation, double r1, double r2)
        => averageValue + standardDeviation * (float)(System.Math.Sqrt(-2.0 * System.Math.Log(1.0 - r1)) * System.Math.Sin(2.0 * System.Math.PI * r2));

    static void SetStarAge(StarClass star, float age, double rn, double rt)
    {
        float num1 = (float)(rn * 0.1 + 0.95);
        float num2 = (float)(rt * 0.4 + 0.8);
        float num3 = (float)(rt * 9.0 + 1.0);
        star.Age = age;
        if (age >= 1.0)
        {
            if (star.Mass >= 18.0)
            {
                star.Type = EStarType.BlackHole; star.Spectr = ESpectrType.X;
                star.Mass *= 2.5f * num2; star.Radius *= 1.0f;
                star.AcdiskRadius = star.Radius * 5.0f; star.Temperature = 0.0f;
                star.Luminosity *= 1.0f / 1000.0f * num1; star.HabitableRadius = 0.0f;
                star.LightBalanceRadius *= 0.4f * num1; star.Color = 1.0f;
            }
            else if (star.Mass >= 7.0)
            {
                star.Type = EStarType.NeutronStar; star.Spectr = ESpectrType.X;
                star.Mass *= 0.2f * num1; star.Radius *= 0.15f;
                star.AcdiskRadius = star.Radius * 9.0f; star.Temperature = num3 * 1E+07f;
                star.Luminosity *= 0.1f * num1; star.HabitableRadius = 0.0f;
                star.LightBalanceRadius *= 3.0f * num1; star.OrbitScaler *= 1.5f * num1;
                star.Color = 1.0f;
            }
            else
            {
                star.Type = EStarType.WhiteDwarf; star.Spectr = ESpectrType.X;
                star.Mass *= 0.2f * num1; star.Radius *= 0.2f;
                star.AcdiskRadius = 0.0f; star.Temperature = num2 * 150000.0f;
                star.Luminosity *= 0.04f * num2; star.HabitableRadius *= 0.15f * num2;
                star.LightBalanceRadius *= 0.2f * num1; star.Color = 0.7f;
            }
        }
        else
        {
            if (age < 0.959999978542328) return;
            float num4 = (float)(System.Math.Pow(5.0, System.Math.Abs(System.Math.Log10(star.Mass) - 0.7)) * 5.0);
            if (num4 > 10.0) num4 = (float)(((double)Mathf.Log(num4 * 0.1f) + 1.0) * 10.0);
            float num5 = (float)(1.0 - Mathf.Pow(star.Age, 30.0f) * 0.5);
            star.Type = EStarType.GiantStar;
            star.Mass = num5 * star.Mass;
            star.Radius = num4 * num2;
            star.AcdiskRadius = 0.0f;
            star.Temperature = num5 * star.Temperature;
            star.Luminosity = 1.6f * star.Luminosity;
            star.HabitableRadius = 9.0f * star.HabitableRadius;
            star.LightBalanceRadius = 3.0f * star.HabitableRadius;
            star.OrbitScaler = 3.3f * star.OrbitScaler;
        }
    }

    void CreateStar(VecLF3 pos, int id, int seed, EStarType needtype, ESpectrType needSpectr)
    {
        var star = Stars[id - 1];
        star.Index = id - 1;
        star.Level = StarCount <= 1 ? 0.0f : (float)star.Index / (StarCount - 1);
        star.Id = id;
        star.Seed = seed;
        var rnd1 = new DotNet35Random(seed);
        int seed1 = rnd1.Next();
        int Seed = rnd1.Next();
        star.Position = pos;
        float num1 = (float)pos.Magnitude / 32.0f;
        if (num1 > 1.0)
            num1 = Mathf.Log(Mathf.Log(Mathf.Log(Mathf.Log(Mathf.Log(num1) + 1.0f) + 1.0f) + 1.0f) + 1.0f) + 1.0f;
        star.ResourceCoef = Mathf.Pow(7.0f, num1) * 0.6f;
        var rnd2 = new DotNet35Random(Seed);
        double r1 = rnd2.NextDouble();
        double r2 = rnd2.NextDouble();
        double num2 = rnd2.NextDouble();
        double rn = rnd2.NextDouble();
        double rt = rnd2.NextDouble();
        double num3 = (rnd2.NextDouble() - 0.5) * 0.2;
        double num4 = rnd2.NextDouble() * 0.2 + 0.9;
        double y = rnd2.NextDouble() * 0.4 - 0.2;
        double num5 = System.Math.Pow(2.0, y);
        float num6 = Mathf.Lerp(-0.98f, 0.88f, star.Level);
        float averageValue = num6 >= 0.0 ? num6 + 0.65f : num6 - 0.65f;
        float standardDeviation = 0.33f;
        if (needtype == EStarType.GiantStar)
        {
            averageValue = y > -0.08 ? -1.5f : 1.6f;
            standardDeviation = 0.3f;
        }
        float num7 = RandNormal(averageValue, standardDeviation, r1, r2);
        if (needSpectr == ESpectrType.M) num7 = -3.0f;
        if (needSpectr == ESpectrType.O) num7 = 3.0f;
        float p1 = (float)(Mathf.Clamp(num7 <= 0.0 ? num7 * 1.0f : num7 * 2.0f, -2.4f, 4.65f) + num3 + 1.0);
        switch (needtype)
        {
            case EStarType.WhiteDwarf: star.Mass = (float)(1.0 + r2 * 5.0); break;
            case EStarType.NeutronStar: star.Mass = (float)(7.0 + r1 * 11.0); break;
            case EStarType.BlackHole: star.Mass = (float)(18.0 + r1 * r2 * 30.0); break;
            default: star.Mass = Mathf.Pow(2.0f, p1); break;
        }
        double d = 5.0;
        if (star.Mass < 2.0) d = 2.0 + 0.4 * (1.0 - star.Mass);
        star.Lifetime = (float)(10000.0 * System.Math.Pow(0.1, System.Math.Log10(star.Mass * 0.5) / System.Math.Log10(d) + 1.0) * num4);
        switch (needtype)
        {
            case EStarType.GiantStar:
                star.Lifetime = (float)(10000.0 * System.Math.Pow(0.1, System.Math.Log10(star.Mass * 0.58) / System.Math.Log10(d) + 1.0) * num4);
                star.Age = (float)(num2 * 0.0399999991059303 + 0.959999978542328);
                break;
            case EStarType.WhiteDwarf:
            case EStarType.NeutronStar:
            case EStarType.BlackHole:
                star.Age = (float)(num2 * 0.400000005960464 + 1.0);
                if (needtype == EStarType.WhiteDwarf) star.Lifetime += 10000.0f;
                if (needtype == EStarType.NeutronStar) star.Lifetime += 1000.0f;
                break;
            default:
                star.Age = star.Mass >= 0.5 ? (star.Mass >= 0.8 ? (float)(num2 * 0.699999988079071 + 0.200000002980232) : (float)(num2 * 0.400000005960464 + 0.100000001490116)) : (float)(num2 * 0.119999997317791 + 0.0199999995529652);
                break;
        }
        float num8 = star.Lifetime * star.Age;
        if (num8 > 5000.0) num8 = (float)(((double)Mathf.Log(num8 / 5000.0f) + 1.0) * 5000.0);
        if (num8 > 8000.0) num8 = (float)(((double)Mathf.Log(Mathf.Log(Mathf.Log(num8 / 8000.0f) + 1.0f) + 1.0f) + 1.0) * 8000.0);
        star.Lifetime = num8 / star.Age;
        float num9 = (float)(1.0 - Mathf.Pow(Mathf.Clamp01(star.Age), 20.0f) * 0.5) * star.Mass;
        star.Temperature = (float)(System.Math.Pow(num9, 0.56 + 0.14 / (System.Math.Log10(num9 + 4.0) / System.Math.Log10(5.0))) * 4450.0 + 1300.0);
        double num10 = System.Math.Log10((star.Temperature - 1300.0) / 4500.0) / System.Math.Log10(2.6) - 0.5;
        if (num10 < 0.0) num10 *= 4.0;
        if (num10 > 2.0) num10 = 2.0;
        else if (num10 < -4.0) num10 = -4.0;
        star.Spectr = (ESpectrType)Mathf.RoundToInt((float)num10 + 4.0f);
        star.Color = Mathf.Clamp01((float)((num10 + 3.5) * 0.200000002980232));
        star.ClassFactor = (float)num10;
        star.Luminosity = Mathf.Pow(num9, 0.7f);
        star.Radius = (float)(System.Math.Pow(star.Mass, 0.4) * num5);
        star.AcdiskRadius = 0.0f;
        float p2 = (float)num10 + 2.0f;
        star.HabitableRadius = Mathf.Pow(1.7f, p2) + 0.25f * Mathf.Min(1.0f, star.OrbitScaler);
        star.LightBalanceRadius = Mathf.Pow(1.7f, p2);
        star.OrbitScaler = Mathf.Pow(1.35f, p2);
        if (star.OrbitScaler < 1.0) star.OrbitScaler = Mathf.Lerp(star.OrbitScaler, 1.0f, 0.6f);
        SetStarAge(star, star.Age, rn, rt);
        star.DysonRadius = star.OrbitScaler * 0.28f;
        if (star.DysonRadius * 40000.0 < star.PhysicsRadius * 1.5)
            star.DysonRadius = (float)(star.PhysicsRadius * 1.5 / 40000.0);
        star.UPosition = star.Position * 2400000.0;

        // ===== 官方安全度/黑雾等级（Assembly-CSharp StarGen.CreateStar 尾部）=====
        // 注意：官方在消耗完 num3..y 共 8 个 double 后，再用 dotNet35Random2.Next() 派生
        // dotNet35Random3 并取 num10——该额外消耗不影响其它数据（此后不再使用该链）
        var rnd3 = new DotNet35Random(rnd2.Next());
        double safetyJitter = rnd3.NextDouble();
        float posMag = (float)pos.Magnitude;
        float num16 = Mathf.Pow(star.Color, 1.3f);
        float num17 = Mathf.Clamp((posMag - 2f) / 20f, 0f, 2.5f);
        if (num17 > 1f)
        {
            num17 = Mathf.Log(num17) + 1.0f;
            num17 = Mathf.Log(num17) + 1.0f;
        }
        num17 /= 1.4f;
        if (star.Type == EStarType.BlackHole) num16 = 5f;
        else if (star.Type == EStarType.NeutronStar) num16 = 1.7f;
        else if (star.Type == EStarType.WhiteDwarf) num16 = 1.2f;
        else if (star.Type == EStarType.GiantStar) num16 = Mathf.Max(0.6f, num16);
        else if (star.Spectr == ESpectrType.O) num16 += 0.05f;
        num16 *= 0.9f;
        num16 += 0.07f;
        float num18 = Mathf.Clamp01(1f - Mathf.Pow(num16, 0.73f) * Mathf.Pow(num17, 0.27f) + (float)safetyJitter * 0.08f - 0.04f);
        star.SafetyFactor = num18;
        star.HivePatternLevel = num18 >= 0.7f ? 0 : (num18 >= 0.3f ? 1 : 2);

        star.Name = NameGen.RandomStarName(seed1, star, Starnames);
        star.OverrideName = "";
    }

    StarClass CreateBirthStar(int index, int seed)
    {
        var birthStar = Stars[index];
        birthStar.Index = 0;
        birthStar.Level = 0.0f;
        birthStar.Id = 1;
        birthStar.Seed = seed;
        birthStar.ResourceCoef = 0.6f;
        var rnd1 = new DotNet35Random(seed);
        int seed1 = rnd1.Next();
        int Seed = rnd1.Next();
        birthStar.OverrideName = "";
        birthStar.Position = VecLF3.Zero;
        var rnd2 = new DotNet35Random(Seed);
        double r1 = rnd2.NextDouble();
        double r2 = rnd2.NextDouble();
        double num1 = rnd2.NextDouble();
        double rn = rnd2.NextDouble();
        double rt = rnd2.NextDouble();
        double num2 = rnd2.NextDouble() * 0.2 + 0.9;
        double num3 = System.Math.Pow(2.0, rnd2.NextDouble() * 0.4 - 0.2);
        float p1 = Mathf.Clamp(RandNormal(0.0f, 0.08f, r1, r2), -0.2f, 0.2f);
        birthStar.Mass = Mathf.Pow(2.0f, p1);
        double d = 2.0 + 0.4 * (1.0 - birthStar.Mass);
        birthStar.Lifetime = (float)(10000.0 * System.Math.Pow(0.1, System.Math.Log10(birthStar.Mass * 0.5) / System.Math.Log10(d) + 1.0) * num2);
        birthStar.Age = (float)(num1 * 0.4 + 0.3);
        float num4 = (float)(1.0 - Mathf.Pow(Mathf.Clamp01(birthStar.Age), 20.0f) * 0.5) * birthStar.Mass;
        birthStar.Temperature = (float)(System.Math.Pow(num4, 0.56 + 0.14 / (System.Math.Log10(num4 + 4.0) / System.Math.Log10(5.0))) * 4450.0 + 1300.0);
        double num5 = System.Math.Log10((birthStar.Temperature - 1300.0) / 4500.0) / System.Math.Log10(2.6) - 0.5;
        if (num5 < 0.0) num5 *= 4.0;
        if (num5 > 2.0) num5 = 2.0;
        else if (num5 < -4.0) num5 = -4.0;
        birthStar.Spectr = (ESpectrType)Mathf.RoundToInt((float)num5 + 4.0f);
        birthStar.Color = Mathf.Clamp01((float)((num5 + 3.5) * 0.200000002980232));
        birthStar.ClassFactor = (float)num5;
        birthStar.Luminosity = Mathf.Pow(num4, 0.7f);
        birthStar.Radius = (float)(System.Math.Pow(birthStar.Mass, 0.4) * num3);
        birthStar.AcdiskRadius = 0.0f;
        float p2 = (float)num5 + 2.0f;
        birthStar.HabitableRadius = Mathf.Pow(1.7f, p2) + 0.2f * Mathf.Min(1.0f, birthStar.OrbitScaler);
        birthStar.LightBalanceRadius = Mathf.Pow(1.7f, p2);
        birthStar.OrbitScaler = Mathf.Pow(1.35f, p2);
        if (birthStar.OrbitScaler < 1.0) birthStar.OrbitScaler = Mathf.Lerp(birthStar.OrbitScaler, 1.0f, 0.6f);
        SetStarAge(birthStar, birthStar.Age, rn, rt);
        birthStar.DysonRadius = birthStar.OrbitScaler * 0.28f;
        if (birthStar.DysonRadius * 40000.0 < birthStar.PhysicsRadius * 1.5)
            birthStar.DysonRadius = (float)(birthStar.PhysicsRadius * 1.5 / 40000.0);
        birthStar.UPosition = VecLF3.Zero;
        birthStar.Name = NameGen.RandomStarName(seed1, birthStar, Starnames);
        return birthStar;
    }

    int GenerateTempPoses(List<VecLF3> poses, int seed, int targetCount, int iterCount, double minDist, double minStepLen, double maxStepLen, double flatten)
    {
        var tmpDrunk = new List<VecLF3>();
        var tmpPoses = new List<VecLF3>();
        RandomPoses(tmpPoses, tmpDrunk, seed, targetCount * iterCount, minDist, minStepLen, maxStepLen, flatten);
        for (int i = 0; i < targetCount; i++) poses.Add(tmpPoses[i * 4]);
        return poses.Count;
    }

    static bool CheckCollision(List<VecLF3> pts, VecLF3 pt, double minDist)
    {
        double num1 = minDist * minDist;
        foreach (var pt1 in pts)
        {
            double num2 = pt.X - pt1.X;
            double num3 = pt.Y - pt1.Y;
            double num4 = pt.Z - pt1.Z;
            if (num2 * num2 + num3 * num3 + num4 * num4 < num1) return true;
        }
        return false;
    }

    static void RandomPoses(List<VecLF3> tmpPoses, List<VecLF3> tmpDrunk, int seed, int maxCount, double minDist, double minStepLen, double maxStepLen, double flatten)
    {
        var rnd = new DotNet35Random(seed);
        double num1 = rnd.NextDouble();
        tmpPoses.Add(VecLF3.Zero);
        int num2 = 6, num3 = 8;
        double num4 = num3 - num2;
        int num5 = (int)(num1 * num4 + num2);
        for (int index = 0; index < num5; ++index)
        {
            int num6 = 0;
            while (num6++ < 256)
            {
                double num7 = rnd.NextDouble() * 2.0 - 1.0;
                double num8 = (rnd.NextDouble() * 2.0 - 1.0) * flatten;
                double num9 = rnd.NextDouble() * 2.0 - 1.0;
                double num10 = rnd.NextDouble();
                double d = num7 * num7 + num8 * num8 + num9 * num9;
                if (d <= 1.0 && d >= 1E-08)
                {
                    double num11 = System.Math.Sqrt(d);
                    double num12 = (num10 * (maxStepLen - minStepLen) + minDist) / num11;
                    var pt = new VecLF3(num7 * num12, num8 * num12, num9 * num12);
                    if (!CheckCollision(tmpPoses, pt, minDist))
                    {
                        tmpDrunk.Add(pt);
                        tmpPoses.Add(pt);
                        if (tmpPoses.Count >= maxCount) return;
                        break;
                    }
                }
            }
        }
        int num13 = 0;
        while (num13++ < 256)
        {
            for (int index = 0; index < tmpDrunk.Count; ++index)
            {
                if (rnd.NextDouble() <= 0.7)
                {
                    int num14 = 0;
                    while (num14++ < 256)
                    {
                        double num15 = rnd.NextDouble() * 2.0 - 1.0;
                        double num16 = (rnd.NextDouble() * 2.0 - 1.0) * flatten;
                        double num17 = rnd.NextDouble() * 2.0 - 1.0;
                        double num18 = rnd.NextDouble();
                        double d = num15 * num15 + num16 * num16 + num17 * num17;
                        if (d <= 1.0 && d >= 1E-08)
                        {
                            double num19 = System.Math.Sqrt(d);
                            double num20 = (num18 * (maxStepLen - minStepLen) + minDist) / num19;
                            var pt = new VecLF3(tmpDrunk[index].X + num15 * num20, tmpDrunk[index].Y + num16 * num20, tmpDrunk[index].Z + num17 * num20);
                            if (!CheckCollision(tmpPoses, pt, minDist))
                            {
                                tmpDrunk[index] = pt;
                                tmpPoses.Add(pt);
                                if (tmpPoses.Count >= maxCount) return;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public void CreateStars(int galaxySeed, int starCount, float resourceRate)
    {
        ResourceMultiplier = resourceRate;
        IsInfiniteResource = ResourceMultiplier >= 99.5f;
        IsRareResource = ResourceMultiplier <= 0.1001f;
        var rnd = new DotNet35Random(galaxySeed);
        var tmpPoses = new List<VecLF3>();
        int tempPoses = GenerateTempPoses(tmpPoses, rnd.Next(), starCount, 4, 2.0, 2.3, 3.5, 0.18);
        Seed = galaxySeed;
        StarCount = tempPoses;
        Stars = new List<StarClass>(tempPoses);
        for (int i = 0; i < tempPoses; i++) Stars.Add(new StarClass());
        HabitableCount = 0;

        float num1 = (float)rnd.NextDouble();
        float num2 = (float)rnd.NextDouble();
        float num3 = (float)rnd.NextDouble();
        float num4 = (float)rnd.NextDouble();

        int num5 = Mathf.CeilToInt(0.00999999977648258f * tempPoses + num1 * 0.300000011920929f);
        int num6 = Mathf.CeilToInt(0.00999999977648258f * tempPoses + num2 * 0.300000011920929f);
        int num7 = Mathf.CeilToInt(0.0160000007599592f * tempPoses + num3 * 0.400000005960464f);
        int num8 = Mathf.CeilToInt(0.0130000002682209f * tempPoses + num4 * 1.39999997615814f);
        int num9 = tempPoses - num5;
        int num10 = num9 - num6;
        int num11 = num10 - num7;
        int num12 = (num11 - 1) / num8;
        int num13 = num12 / 2;
        for (int index = 0; index < tempPoses; ++index)
        {
            int seed = rnd.Next();
            if (index == 0)
            {
                CreateBirthStar(index, seed);
            }
            else
            {
                var needSpectr = ESpectrType.X;
                if (index == 3) needSpectr = ESpectrType.M;
                else if (index == num11 - 1) needSpectr = ESpectrType.O;
                var needtype = EStarType.MainSeqStar;
                if (index % num12 == num13) needtype = EStarType.GiantStar;
                if (index >= num9) needtype = EStarType.BlackHole;
                else if (index >= num10) needtype = EStarType.NeutronStar;
                else if (index >= num11) needtype = EStarType.WhiteDwarf;
                CreateStar(tmpPoses[index], index + 1, seed, needtype, needSpectr);
            }
        }
    }

    public void CreatePlanets()
    {
        foreach (var star in Stars) CreateStarPlanets(star);
    }
}