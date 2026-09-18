using SearchSeed.Core.Data;
using SearchSeed.Core.Maths;
using static SearchSeed.Core.Data.ConstValue;

namespace SearchSeed.Core.Gen;

public struct Pose
{
    public Vec3 Position;
    public Quat Rotation;
}

// Port of PlanetAlgorithm.hpp base class (scalar path only; GPU/AVX2 skipped)
public abstract class PlanetAlgorithm
{
    public static Vec3[] Vertices = new Vec3[VerticesDataLength];
    public static int[] IndexMap = new int[IndexMapDataLength];
    public static int[] LandIndex = new int[LandDataLength];
    public ushort[] HeightData = new ushort[VerticesDataLength];

    static bool _init;

    static int Trans(float x, int pr)
    {
        int num = (int)((Mathf.Sqrt(x + 0.23f) - 0.4795832f) / 0.6294705f * pr);
        if (num >= pr) num = pr - 1;
        return num;
    }

    public static int PositionHash(Vec3 v, int corner = 0)
    {
        if (corner == 0)
            corner = (v.X > 0.0f ? 1 : 0) + (v.Y > 0.0f ? 2 : 0) + (v.Z > 0.0f ? 4 : 0);
        if (v.X < 0.0f) v.X = -v.X;
        if (v.Y < 0.0f) v.Y = -v.Y;
        if (v.Z < 0.0f) v.Z = -v.Z;
        if (v.X < 1E-06f && v.Y < 1E-06f && v.Z < 1E-06f) return 0;
        int num, num2, num3;
        if (v.X >= v.Y && v.X >= v.Z)
        {
            num = 0; num2 = Trans(v.Z / v.X, IndexMapPrecision); num3 = Trans(v.Y / v.X, IndexMapPrecision);
        }
        else if (v.Y >= v.X && v.Y >= v.Z)
        {
            num = 1; num2 = Trans(v.X / v.Y, IndexMapPrecision); num3 = Trans(v.Z / v.Y, IndexMapPrecision);
        }
        else
        {
            num = 2; num2 = Trans(v.X / v.Z, IndexMapPrecision); num3 = Trans(v.Y / v.Z, IndexMapPrecision);
        }
        return num2 + num3 * IndexMapPrecision + num * IndexMapFaceStride + corner * IndexMapCornerStride;
    }

    static void CalcVerts()
    {
        int num = (Precision + 1) * 2;
        int num2 = Precision + 1;
        Vec3[] poles = [Vec3.Right, -Vec3.Right, Vec3.Up, -Vec3.Up, Vec3.Forward, -Vec3.Forward];
        for (int i = 0; i < IndexMapDataLength; i++) IndexMap[i] = -1;
        for (int j = 0; j < VerticesDataLength; j++)
        {
            int num3 = j % num;
            int num4 = j / num;
            int num5 = num3 % num2;
            int num6 = num4 % num2;
            int num7 = ((num3 >= num2 ? 1 : 0) + (num4 >= num2 ? 1 : 0) * 2) * 2 + (num5 < num6 ? 1 : 0);
            float num8 = num5 >= num6 ? Precision - num5 : num5;
            float num9 = num5 >= num6 ? num6 : Precision - num6;
            float num10 = Precision - num9;
            num9 /= Precision;
            num8 = num10 > 0.0f ? num8 / num10 : 0.0f;
            int num11;
            Vec3 a, a2, b;
            switch (num7)
            {
                case 0: a = poles[2]; a2 = poles[0]; b = poles[4]; num11 = 7; break;
                case 1: a = poles[3]; a2 = poles[4]; b = poles[0]; num11 = 5; break;
                case 2: a = poles[2]; a2 = poles[4]; b = poles[1]; num11 = 6; break;
                case 3: a = poles[3]; a2 = poles[1]; b = poles[4]; num11 = 4; break;
                case 4: a = poles[2]; a2 = poles[1]; b = poles[5]; num11 = 2; break;
                case 5: a = poles[3]; a2 = poles[5]; b = poles[1]; num11 = 0; break;
                case 6: a = poles[2]; a2 = poles[5]; b = poles[0]; num11 = 3; break;
                case 7: a = poles[3]; a2 = poles[0]; b = poles[5]; num11 = 1; break;
                default: a = poles[2]; a2 = poles[0]; b = poles[4]; num11 = 7; break;
            }
            Vertices[j] = NativeMath.Slerp(NativeMath.Slerp(a, b, num9), NativeMath.Slerp(a2, b, num9), num8);
            int num12 = PositionHash(Vertices[j], num11);
            if (IndexMap[num12] == -1) IndexMap[num12] = j;
        }
        for (int k = 1; k < IndexMapDataLength; k++)
        {
            if (IndexMap[k] == -1) IndexMap[k] = IndexMap[k - 1];
        }
    }

    public static void DoInit()
    {
        if (_init) return;
        _init = true;
        CalcVerts();
        int index = 0;
        for (int i = 0; i < VerticesDataLength; i++)
        {
            int num5 = i % Stride;
            int num6 = i / Stride;
            if (num5 > LandPercentNum) num5--;
            if (num6 > LandPercentNum) num6--;
            if ((num5 & num6 & 1) != 0)
                LandIndex[index++] = i;
        }
    }

    protected PlanetAlgorithm() => DoInit();

    public abstract void GenerateHeight1(int index);

    public virtual void GenerateHeights(int[] indexArr, int count)
    {
        for (int i = 0; i < count; i++) GenerateHeight1(indexArr[i]);
    }

    public abstract void GenerateTerrain(PlanetClass planet, bool genTerr = false);

    public virtual bool CheckVeinPosition(Vec3 targetPos, EVeinType veinType)
    {
        float targetHeight = QueryHeight(targetPos);
        return targetHeight < NormalPlanetRadius || (veinType == EVeinType.Oil && targetHeight < NormalPlanetRadius + 0.5f);
    }

    public float CalcLandPercent(PlanetClass planet)
    {
        if (planet.Theme == 16) return 1.0f; // 水世界
        if (planet.Type == EPlanetType.Gas) return 0.0f;
        bool needGen = false;
        for (int i = 0; i < LandDataLength; i++)
        {
            if (HeightData[LandIndex[i]] == 0) { needGen = true; break; }
        }
        if (needGen) GenerateHeights(LandIndex, LandDataLength);

        float threshold = NormalPlanetRadius * 100.0f - 20.0f;
        int num3 = 0, num4 = 0;
        for (int i = 0; i < VerticesDataLength; i++)
        {
            int num5 = i % Stride;
            int num6 = i / Stride;
            if (num5 > LandPercentNum) num5--;
            if (num6 > LandPercentNum) num6--;
            if ((num5 & num6 & 1) != 0)
            {
                if (HeightData[i] >= threshold) num4++;
                num3++;
            }
        }
        return num3 > 0 ? (float)num4 / num3 : 0.0f;
    }

    public float QueryHeight(Vec3 vpos)
    {
        vpos.Normalize();
        int num = PositionHash(vpos);
        int num2 = IndexMap[num];
        float num3 = MathF.PI / (Precision * 2) * 1.2f;
        float num4 = num3 * num3;
        float num5 = 0.0f, num6 = 0.0f;
        int[] num8Cache = new int[25];
        float[] num9Cache = new float[25];
        int count = 0;
        for (int i = -1; i <= 3; i++)
        {
            for (int j = -1; j <= 3; j++)
            {
                int num8 = num2 + i + j * Stride;
                if ((uint)num8 < VerticesDataLength)
                {
                    float sqrMagnitude = (Vertices[num8] - vpos).SqrMagnitude;
                    if (sqrMagnitude <= num4)
                    {
                        float num9 = 1.0f - Mathf.Sqrt(sqrMagnitude) / num3;
                        num8Cache[count] = num8;
                        num9Cache[count] = num9;
                        count++;
                    }
                }
            }
        }
        int[] indexCache = new int[25];
        int curNum = 0;
        for (int i = 0; i < count; i++)
        {
            if (HeightData[num8Cache[i]] != 0) continue;
            indexCache[curNum++] = num8Cache[i];
        }
        GenerateHeights(indexCache, curNum);

        for (int i = 0; i < count; i++)
        {
            num5 += num9Cache[i];
            num6 += (int)HeightData[num8Cache[i]] * num9Cache[i];
        }
        if (num5 == 0.0f)
        {
            if (HeightData[0] == 0) GenerateHeight1(0);
            return (int)HeightData[0] * 0.01f;
        }
        return num6 / num5 * 0.01f;
    }

    public struct BirthMotion
    {
        public double OrbitalPeriod;
        public Quat OrbitRotation;
        public Pose Pose;
    }

    public static BirthMotion PredictBirthMotion(PlanetClass planet, double time, bool includeRotation)
    {
        var star = planet.StarRef!;
        var random = new DotNet35Random(planet.InfoSeed);
        random.NextDouble(); // num3
        random.NextDouble(); // num4
        double num5 = random.NextDouble();
        double num6 = random.NextDouble();
        double num7 = random.NextDouble();

        BirthMotion parent = default;
        if (planet.OrbitAroundPlanet != null)
            parent = PredictBirthMotion(planet.OrbitAroundPlanet, time, false);

        BirthMotion motion = default;
        float inclination = (float)(num5 * 16.0 - 8.0);
        if (planet.OrbitAround > 0) inclination *= 2.2f;
        if (star.Type >= EStarType.NeutronStar)
        {
            if (inclination > 0.0) inclination += 3.0f;
            else inclination -= 3.0f;
        }
        float f1 = planet.OrbitRadius;
        motion.OrbitalPeriod = planet.OrbitAroundPlanet != null
            ? System.Math.Sqrt(39.4784176043574 * f1 * f1 * f1 / 1.08308421068537E-08)
            : System.Math.Sqrt(39.4784176043574 * f1 * f1 * f1 / (1.35385519905204E-06 * star.Mass));
        float orbitPhase = (float)(num7 * 360.0);
        motion.OrbitRotation = Quat.AngleAxis((float)(num6 * 360.0), Vec3.Up) * Quat.AngleAxis(inclination, Vec3.Forward);
        if (planet.OrbitAroundPlanet != null)
            motion.OrbitRotation = parent.OrbitRotation * motion.OrbitRotation;

        double num = time / motion.OrbitalPeriod + orbitPhase / 360.0;
        int num2 = (int)(num + 0.1);
        num -= num2;
        num *= System.Math.PI * 2.0;
        motion.Pose.Position = MathUtil.QRotate(motion.OrbitRotation, new Vec3((float)System.Math.Cos(num) * planet.OrbitRadius, 0.0f, (float)System.Math.Sin(num) * planet.OrbitRadius));
        if (planet.OrbitAroundPlanet != null)
        {
            motion.Pose.Position += parent.Pose.Position;
        }
        if (!includeRotation) return motion;

        double num8 = random.NextDouble();
        double num9 = random.NextDouble();
        double num10 = random.NextDouble();
        double num11 = random.NextDouble();
        double num12 = random.NextDouble();
        random.NextDouble(); // num13
        random.NextDouble(); // num14
        random.NextDouble(); // rand1
        double num15 = random.NextDouble();
        float obliquity;
        if (num15 < 0.0399999991059303)
        {
            obliquity = (float)(num8 * (num9 - 0.5) * 39.9);
            if (obliquity < 0.0) obliquity -= 70.0f;
            else obliquity += 70.0f;
        }
        else if (num15 < 0.100000001490116)
        {
            obliquity = (float)(num8 * (num9 - 0.5) * 80.0);
            if (obliquity < 0.0) obliquity -= 30.0f;
            else obliquity += 30.0f;
        }
        else
            obliquity = (float)(num8 * (num9 - 0.5) * 60.0);
        bool gasGiant = planet.Type == EPlanetType.Gas;
        double rotationPeriod = (num10 * num11 * 1000.0 + 400.0) * (planet.OrbitAround == 0 ? (double)Mathf.Pow(f1, 0.25f) : 1.0) * (gasGiant ? 0.200000002980232 : 1.0);
        if (!gasGiant)
        {
            if (star.Type == EStarType.WhiteDwarf) rotationPeriod *= 0.5;
            else if (star.Type == EStarType.NeutronStar) rotationPeriod *= 0.200000002980232;
            else if (star.Type == EStarType.BlackHole) rotationPeriod *= 0.150000005960464;
        }
        float rotationPhase = (float)(num12 * 360.0);
        double num17 = planet.OrbitAround == 0 ? motion.OrbitalPeriod : parent.OrbitalPeriod;
        rotationPeriod = 1.0 / (1.0 / num17 + 1.0 / rotationPeriod);
        if (planet.OrbitAround == 0 && planet.OrbitIndex <= 4 && !gasGiant)
        {
            if (num15 > 0.959999978542328)
            {
                obliquity *= 0.01f;
                rotationPeriod = motion.OrbitalPeriod;
            }
            else if (num15 > 0.930000007152557)
            {
                obliquity *= 0.1f;
                rotationPeriod = motion.OrbitalPeriod * 0.5;
            }
            else if (num15 > 0.899999976158142)
            {
                obliquity *= 0.2f;
                rotationPeriod = motion.OrbitalPeriod * 0.25;
            }
        }
        if (num15 > 0.85 && num15 <= 0.9) rotationPeriod = -rotationPeriod;
        Quat systemRotation = motion.OrbitRotation * Quat.AngleAxis(obliquity, Vec3.Forward);
        double num3b = time / rotationPeriod + rotationPhase / 360.0;
        int num4b = (int)(num3b + 0.1);
        num3b = (num3b - num4b) * 360.0;
        motion.Pose.Rotation = systemRotation * Quat.AngleAxis((float)num3b, -Vec3.Up);
        return motion;
    }

    public (Vec3 birthPoint, Vec3 res0, Vec3 res1) GenBirthPoints(PlanetClass planet, int birthSeed, VecLF3 starUPosition)
    {
        var dotNet35Random = new DotNet35Random(birthSeed);
        Pose pose = PredictBirthMotion(planet, 85.0, true).Pose;
        Vec3 vector = MathUtil.QInvRotateLF(pose.Rotation, starUPosition - (pose.Position * 40000.0f).ToVecLF3()).ToVec3();
        vector.Normalize();
        Vec3 normalized = Vec3.Cross(vector, Vec3.Up).Normalized;
        Vec3 normalized2 = Vec3.Cross(normalized, vector).Normalized;
        int i = 0;
        Vec3 birthPoint = default, birthResourcePoint0 = default, birthResourcePoint1 = default;
        for (; i < 256; i++)
        {
            float num2 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.5f;
            float num3 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.5f;
            Vec3 vector2 = vector + normalized * num2 + normalized2 * num3;
            vector2.Normalize();
            birthPoint = vector2 * (NormalPlanetRadius + 0.2f + 1.45f);
            normalized = Vec3.Cross(vector2, Vec3.Up).Normalized;
            normalized2 = Vec3.Cross(normalized, vector2).Normalized;
            bool flag = false;
            for (int j = 0; j < 10; j++)
            {
                float x = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0);
                float y = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0);
                Vec2 vector3 = new Vec2(x, y).Normalized * 0.1f;
                Vec2 vector4 = -vector3;
                float num4 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.06f;
                float num5 = (float)(dotNet35Random.NextDouble() * 2.0 - 1.0) * 0.06f;
                vector4.X += num4;
                vector4.Y += num5;
                Vec3 normalized3 = (vector2 + normalized * vector3.X + normalized2 * vector3.Y).Normalized;
                Vec3 normalized4 = (vector2 + normalized * vector4.X + normalized2 * vector4.Y).Normalized;
                birthResourcePoint0 = normalized3;
                birthResourcePoint1 = normalized4;
                float num6 = NormalPlanetRadius + 0.2f;
                if (QueryHeight(vector2) > num6 && QueryHeight(normalized3) > num6 && QueryHeight(normalized4) > num6)
                {
                    Vec3 vpos = normalized3 + normalized * 0.03f;
                    Vec3 vpos2 = normalized3 - normalized * 0.03f;
                    Vec3 vpos3 = normalized3 + normalized2 * 0.03f;
                    Vec3 vpos4 = normalized3 - normalized2 * 0.03f;
                    Vec3 vpos5 = normalized4 + normalized * 0.03f;
                    Vec3 vpos6 = normalized4 - normalized * 0.03f;
                    Vec3 vpos7 = normalized4 + normalized2 * 0.03f;
                    Vec3 vpos8 = normalized4 - normalized2 * 0.03f;
                    if (QueryHeight(vpos) > num6 && QueryHeight(vpos2) > num6 && QueryHeight(vpos3) > num6 && QueryHeight(vpos4) > num6 &&
                        QueryHeight(vpos5) > num6 && QueryHeight(vpos6) > num6 && QueryHeight(vpos7) > num6 && QueryHeight(vpos8) > num6)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag) break;
        }
        if (i >= 256) birthPoint = new Vec3(0.0f, NormalPlanetRadius + 5.0f, 0.0f);
        return (birthPoint, birthResourcePoint0, birthResourcePoint1);
    }
}
