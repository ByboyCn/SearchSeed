using SearchSeed.Core.Data;
using SearchSeed.Core.Maths;
using static SearchSeed.Core.Data.ConstValue;

namespace SearchSeed.Core.Gen;

// Port of PlanetAlgorithm.hpp PlanetAlgorithm0..13 (scalar path only; AVX2/OpenCL blocks skipped).
// C++ floats stay float, doubles stay double; Math.* = System.Math (double), Mathf.* = float helpers.

public class PlanetAlgorithm0 : PlanetAlgorithm
{
    public override void GenerateHeight1(int index)
    {
        HeightData[index] = (ushort)((double)NormalPlanetRadius * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        Array.Fill(HeightData, (ushort)((double)NormalPlanetRadius * 100.0), 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm1 : PlanetAlgorithm
{
    const double num = 0.01, num2 = 0.012, num3 = 0.01;
    const double num4 = 3.0, num5 = -0.2, num6 = 0.9, num7 = 0.5, num8 = 2.5, num9 = 0.3;

    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();

    public override void GenerateHeight1(int index)
    {
        double num12 = Vertices[index].X * NormalPlanetRadius;
        double num13 = Vertices[index].Y * NormalPlanetRadius;
        double num14 = Vertices[index].Z * NormalPlanetRadius;
        double num17 = simplexNoise.Noise3DFBM(num12 * num, num13 * num2, num14 * num3, 6) * num4 + num5;
        double num18 = simplexNoise2.Noise3DFBM(num12 * 0.0025, num13 * 0.0025, num14 * 0.0025, 3) * num4 * num6 + num7;
        double num19 = num18 > 0.0 ? num18 * 0.5 : num18;
        double num20 = num17 + num19;
        double num21 = num20 > 0.0 ? num20 * 0.5 : num20 * 1.6;
        double num22 = num21 > 0.0 ? MathUtil.Levelize3(num21, 0.7) : MathUtil.Levelize2(num21, 0.5);
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num22 + 0.2) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num10 = dotNet35Random.Next();
        int num11 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num10);
        simplexNoise2 = new SimplexNoise(num11);
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm2 : PlanetAlgorithm
{
    double num, num2, num3, num4;
    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();

    public override void GenerateHeight1(int index)
    {
        double num8 = Vertices[index].X * NormalPlanetRadius;
        double num9 = Vertices[index].Y * NormalPlanetRadius;
        double num10 = Vertices[index].Z * NormalPlanetRadius;
        double num14 = simplexNoise.Noise3DFBM(num8 * num, num9 * num2, num10 * num3, 6, 0.45, 1.8);
        double value = num14 * num4 + num4 * 0.4;
        double num16 = 0.6 / (Math.Abs(value) + 0.6) - 0.25;
        double num17 = num16 < 0.0 ? num16 * 0.3 : num16;
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num17 + 0.1) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        double modX = planet.ModX;
        double modY = planet.ModY;
        modX = (3.0 - modX - modX) * modX * modX;
        num = 0.0035;
        num2 = 0.025 * modX + 0.0035 * (1.0 - modX);
        num3 = 0.0035;
        num4 = 3.0;
        double num5 = 1.0 + 1.3 * modY;
        num *= num5;
        num2 *= num5;
        num3 *= num5;
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num6 = dotNet35Random.Next();
        int num7 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num6);
        simplexNoise2 = new SimplexNoise(num7);
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm3 : PlanetAlgorithm
{
    const double num = 0.007, num2 = 0.007, num3 = 0.007;

    double modX;
    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();

    static double Lerp(double a, double b, double t) => a + (b - a) * t;

    public override void GenerateHeight1(int index)
    {
        double num6 = Vertices[index].X * NormalPlanetRadius;
        double num7 = Vertices[index].Y * NormalPlanetRadius;
        double num8 = Vertices[index].Z * NormalPlanetRadius;
        num6 += Math.Sin(num7 * 0.15) * 3.0;
        num7 += Math.Sin(num8 * 0.15) * 3.0;
        num8 += Math.Sin(num6 * 0.15) * 3.0;
        double num11 = simplexNoise.Noise3DFBM(num6 * num * 1.0, num7 * num2 * 1.1, num8 * num3 * 1.0, 6, 0.5, 1.8);
        double num12 = simplexNoise2.Noise3DFBM(num6 * num * 1.3 + 0.5, num7 * num2 * 2.8 + 0.2, num8 * num3 * 1.3 + 0.7, 3) * 2.0;
        double num13 = simplexNoise2.Noise3DFBM(num6 * num * 6.0, num7 * num2 * 12.0, num8 * num3 * 6.0, 2) * 2.0;
        num13 = Lerp(num13, num13 * 0.1, modX);
        double num14 = simplexNoise2.Noise3DFBM(num6 * num * 0.8, num7 * num2 * 0.8, num8 * num3 * 0.8, 2) * 2.0;
        double num15 = num11 * 2.0 + 0.92;
        double num16 = num12 * (double)Mathf.Abs((float)num14 + 0.5f);
        num15 += (double)Mathf.Clamp01((float)(num16 - 0.35) * 1.0f);
        if (num15 < 0.0)
            num15 *= 2.0;
        double num17 = MathUtil.Levelize2(num15);
        if (num17 > 0.0)
        {
            num17 = MathUtil.Levelize2(num15);
            num17 = Lerp(MathUtil.Levelize4(num17), num17, modX);
        }
        double b = !(num17 > 0.0)
            ? (double)Mathf.Lerp(-1.0f, 0.0f, (float)num17 + 1.0f)
            : (!(num17 > 1.0)
                ? (double)Mathf.Lerp(0.0f, 0.3f, (float)num17) + num13 * 0.1
                : (num17 > 2.0
                    ? (double)Mathf.Lerp(1.2f, 2.0f, (float)num17 - 2.0f) + num13 * 0.12
                    : (double)Mathf.Lerp(0.3f, 1.2f, (float)num17 - 1.0f) + num13 * 0.12));
        double a = !(num17 > 0.0)
            ? (double)Mathf.Lerp(-4.0f, 0.0f, (float)num17 + 1.0f)
            : (!(num17 > 1.0)
                ? (double)Mathf.Lerp(0.0f, 0.3f, (float)num17) + num13 * 0.1
                : (num17 > 2.0
                    ? (double)Mathf.Lerp(1.4f, 2.7f, (float)num17 - 2.0f) + num13 * 0.12
                    : (double)Mathf.Lerp(0.3f, 1.4f, (float)num17 - 1.0f) + num13 * 0.12));
        double num18 = Lerp(a, b, modX);
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num18 + 0.2) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        modX = planet.ModX;
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num4 = dotNet35Random.Next();
        int num5 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num4);
        simplexNoise2 = new SimplexNoise(num5);
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm4 : PlanetAlgorithm
{
    const int kCircleCount = 80;
    const double num = 0.007, num2 = 0.007, num3 = 0.007;

    readonly Vec4[] circles = new Vec4[kCircleCount];
    readonly double[] heights = new double[kCircleCount];
    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();

    public override void GenerateHeight1(int index)
    {
        double num7 = Vertices[index].X * NormalPlanetRadius;
        double num8 = Vertices[index].Y * NormalPlanetRadius;
        double num9 = Vertices[index].Z * NormalPlanetRadius;
        double num12 = simplexNoise.Noise3DFBM(num7 * num, num8 * num2, num9 * num3, 4, 0.45, 1.8);
        double num13 = simplexNoise2.Noise3DFBM(num7 * num * 5.0, num8 * num2 * 5.0, num9 * num3 * 5.0, 4);
        double num14 = num12 * 1.5;
        double num15 = num13 * 0.2;
        double num16 = num14 * 0.08 + num15 * 2.0;
        double num17 = 0.0;
        for (int k = 0; k < kCircleCount; k++)
        {
            double num18 = circles[k].X - num7;
            double num19 = circles[k].Y - num8;
            double num20 = circles[k].Z - num9;
            double num21 = num18 * num18 + num19 * num19 + num20 * num20;
            if (num21 <= circles[k].W)
            {
                double num22 = num21 / circles[k].W + num15 * 1.2;
                if (num22 < 0.0)
                    num22 = 0.0;
                double num23 = num22 * num22;
                double num24 = num23 * num22;
                double num25 = -15.0 * num24 + 21.833333333334 * num23 - 7.533333333333 * num22 + 0.7 + num15;
                if (num25 < 0.0)
                    num25 = 0.0;
                num25 *= num25;
                num25 *= heights[k];
                num17 = num17 > num25 ? num17 : num25;
            }
        }
        double num10 = num17 + num16 + 0.2;
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num10 + 0.1) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num4 = dotNet35Random.Next();
        int num5 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num4);
        simplexNoise2 = new SimplexNoise(num5);
        int num6 = dotNet35Random.Next();
        for (int i = 0; i < kCircleCount; i++)
        {
            VecLF3 vectorLF = RandomTable.SphericNormalAt(ref num6, 1.0);
            var vector = new Vec4((float)vectorLF.X, (float)vectorLF.Y, (float)vectorLF.Z, 0.0f);
            // vector.Normalize(); vector *= NORMAL_PLANET_RADIUS;
            float mag = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            vector.X /= mag; vector.Y /= mag; vector.Z /= mag;
            vector.X *= NormalPlanetRadius; vector.Y *= NormalPlanetRadius; vector.Z *= NormalPlanetRadius;
            vector.W = (float)vectorLF.Magnitude * 8.0f + 8.0f;
            vector.W *= vector.W;
            circles[i] = vector;
            heights[i] = dotNet35Random.NextDouble() * 0.4 + 0.20000000298023224;
        }
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm5 : PlanetAlgorithm
{
    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();

    public override void GenerateHeight1(int index)
    {
        double num3 = Vertices[index].X * NormalPlanetRadius;
        double num4 = Vertices[index].Y * NormalPlanetRadius;
        double num5 = Vertices[index].Z * NormalPlanetRadius;
        double num8 = MathUtil.Levelize(num3 * 0.007);
        double num9 = MathUtil.Levelize(num4 * 0.007);
        double num10 = MathUtil.Levelize(num5 * 0.007);
        num8 += simplexNoise.Noise(num3 * 0.05, num4 * 0.05, num5 * 0.05) * 0.04;
        num9 += simplexNoise.Noise(num4 * 0.05, num5 * 0.05, num3 * 0.05) * 0.04;
        num10 += simplexNoise.Noise(num5 * 0.05, num3 * 0.05, num4 * 0.05) * 0.04;
        double num11 = Math.Abs(simplexNoise2.Noise(num8, num9, num10));
        double num12 = (0.16 - num11) * 10.0;
        num12 = !(num12 > 0.0) ? 0.0 : (num12 > 1.0 ? 1.0 : num12);
        num12 *= num12;
        double num13 = (simplexNoise.Noise3DFBM(num4 * 0.005, num5 * 0.005, num3 * 0.005, 4) + 0.22) * 5.0;
        num13 = !(num13 > 0.0) ? 0.0 : (num13 > 1.0 ? 1.0 : num13);
        double num14 = Math.Abs(simplexNoise2.Noise3DFBM(num8 * 1.5, num9 * 1.5, num10 * 1.5, 2));
        double num6 = num12 * -1.2 * num13;
        if (num6 >= 0.0)
            num6 += num11 * 0.25 + num14 * 0.6;
        num6 -= 0.1;
        double num16 = -0.3 - num6;
        if (num16 > 0.0)
        {
            double num17 = simplexNoise2.Noise(num3 * 0.16, num4 * 0.16, num5 * 0.16) - 1.0;
            num16 = num16 > 1.0 ? 1.0 : num16;
            num16 = (3.0 - num16 - num16) * num16 * num16;
            num6 = -0.3 - num16 * 3.700000047683716 + num16 * num16 * num16 * num16 * num17 * 0.5;
        }
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num6 + 0.2) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num = dotNet35Random.Next();
        int num2 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num);
        simplexNoise2 = new SimplexNoise(num2);
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm6 : PlanetAlgorithm
{
    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();

    public override void GenerateHeight1(int index)
    {
        double num3 = Vertices[index].X * NormalPlanetRadius;
        double num4 = Vertices[index].Y * NormalPlanetRadius;
        double num5 = Vertices[index].Z * NormalPlanetRadius;
        double num8 = MathUtil.Levelize(num3 * 0.007);
        double num9 = MathUtil.Levelize(num4 * 0.007);
        double num10 = MathUtil.Levelize(num5 * 0.007);
        num8 += simplexNoise.Noise(num3 * 0.05, num4 * 0.05, num5 * 0.05) * 0.04;
        num9 += simplexNoise.Noise(num4 * 0.05, num5 * 0.05, num3 * 0.05) * 0.04;
        num10 += simplexNoise.Noise(num5 * 0.05, num3 * 0.05, num4 * 0.05) * 0.04;
        double num11 = Math.Abs(simplexNoise2.Noise(num8, num9, num10));
        double num12 = (0.16 - num11) * 10.0;
        num12 = !(num12 > 0.0) ? 0.0 : (num12 > 1.0 ? 1.0 : num12);
        num12 *= num12;
        double num13 = (simplexNoise.Noise3DFBM(num4 * 0.005, num5 * 0.005, num3 * 0.005, 4) + 0.22) * 5.0;
        num13 = !(num13 > 0.0) ? 0.0 : (num13 > 1.0 ? 1.0 : num13);
        double num14 = Math.Abs(simplexNoise2.Noise3DFBM(num8 * 1.5, num9 * 1.5, num10 * 1.5, 2));
        double num6 = num12 * -1.2 * num13;
        if (num6 >= 0.0)
            num6 += num11 * 0.25 + num14 * 0.6;
        num6 -= 0.1;
        double num15 = -0.3 - num6;
        if (num15 > 0.0)
        {
            num15 = num15 > 1.0 ? 1.0 : num15;
            num15 = (3.0 - num15 - num15) * num15 * num15;
            num6 = -0.3 - num15 * 3.700000047683716;
        }
        double f = num12 > 0.30000001192092896 ? num12 : 0.30000001192092896;
        f = MathUtil.Levelize(f, 0.7);
        num6 = num6 > -0.800000011920929 ? num6 : (0.0 - f - num11) * 0.8999999761581421;
        num6 = num6 > -1.2000000476837158 ? num6 : -1.2000000476837158;
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num6 + 0.2) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num = dotNet35Random.Next();
        int num2 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num);
        simplexNoise2 = new SimplexNoise(num2);
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm7 : PlanetAlgorithm
{
    const double num = 0.008, num2 = 0.01, num3 = 0.01;
    const double num4 = 3.0, num5 = -2.4, num6 = 0.9, num7 = 0.5, num8 = 2.5, num9 = 0.3;

    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();

            public override bool CheckVeinPosition(Vec3 targetPos, EVeinType veinType)
        {
            return veinType == EVeinType.Bamboo && QueryHeight(targetPos) > NormalPlanetRadius - 4.0f;
        }

public override void GenerateHeight1(int index)
    {
        double num12 = Vertices[index].X * NormalPlanetRadius;
        double num13 = Vertices[index].Y * NormalPlanetRadius;
        double num14 = Vertices[index].Z * NormalPlanetRadius;
        double num17 = simplexNoise.Noise3DFBM(num12 * num, num13 * num2, num14 * num3, 6) * num4 + num5;
        double num18 = simplexNoise2.Noise3DFBM(num12 * 0.0025, num13 * 0.0025, num14 * 0.0025, 3) * num4 * num6 + num7;
        double num19 = num18 > 0.0 ? num18 * 0.5 : num18;
        double num20 = num17 + num19;
        double num21 = num20 > 0.0 ? num20 * 0.5 : num20 * 1.6;
        double num22 = num21 > 0.0 ? MathUtil.Levelize3(num21, 0.7) : MathUtil.Levelize2(num21, 0.5);
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num22) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num10 = dotNet35Random.Next();
        int num11 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num10);
        simplexNoise2 = new SimplexNoise(num11);
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm8 : PlanetAlgorithm
{
    double num, num2, num3, modY;
    SimplexNoise simplexNoise = new();

    static float Sign(float f) => f > 0f ? 1f : f < 0f ? -1f : 0f;

    public override void GenerateHeight1(int index)
    {
        double num4 = Vertices[index].X * NormalPlanetRadius;
        double num5 = Vertices[index].Y * NormalPlanetRadius;
        double num6 = Vertices[index].Z * NormalPlanetRadius;
        float num9 = Mathf.Clamp((float)simplexNoise.Noise3DFBM(num4 * num, num5 * num2, num6 * num3, 6, 0.45, 1.8) + 1.0f + (float)modY * 0.01f, 0.0f, 2.0f);
        float num10;
        if ((double)num9 < 1.0)
        {
            float f = Mathf.Cos(num9 * MathF.PI) * 1.1f;
            f = Sign(f) * Mathf.Pow(f, 4.0f);
            f = Mathf.Clamp(f, -1.0f, 1.0f);
            num10 = 1.0f - (f + 1.0f) * 0.5f;
        }
        else
        {
            float f2 = Mathf.Cos((num9 - 1.0f) * MathF.PI) * 1.1f;
            f2 = Sign(f2) * Mathf.Pow(f2, 4.0f);
            f2 = Mathf.Clamp(f2, -1.0f, 1.0f);
            num10 = 2.0f - (f2 + 1.0f) * 0.5f;
        }
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num10 + 0.1) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        double modX = planet.ModX;
        modY = planet.ModY;
        num = 0.002 * modX;
        num2 = 0.002 * modX * modX * 6.66667;
        num3 = 0.002 * modX;
        simplexNoise = new SimplexNoise(new DotNet35Random(planet.Seed).Next());
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm9 : PlanetAlgorithm
{
    const double num = 0.01, num2 = 0.012, num3 = 0.01;
    const double num4 = 3.0, num5 = -0.2, num6 = 0.9, num7 = 0.5, num8 = 2.5, num9 = 0.3;

    double modX, modY;
    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();

    public override void GenerateHeight1(int index)
    {
        double num12 = Vertices[index].X * NormalPlanetRadius;
        double num13 = Vertices[index].Y * NormalPlanetRadius;
        double num14 = Vertices[index].Z * NormalPlanetRadius;
        double num17 = simplexNoise.Noise3DFBM(num12 * num * 0.75, num13 * num2 * 0.5, num14 * num3 * 0.75, 6) * num4 + num5;
        double num18 = simplexNoise2.Noise3DFBM(num12 * 0.0025, num13 * 0.0025, num14 * 0.0025, 3) * num4 * num6 + num7;
        double num19 = num18 > 0.0 ? num18 * 0.5 : num18;
        double num20 = num17 + num19;
        double num21 = num20 > 0.0 ? num20 * 0.5 : num20 * 1.6;
        double num22 = num21 > 0.0 ? MathUtil.Levelize3(num21, 0.7) : MathUtil.Levelize2(num21, 0.5);
        num22 += 0.618;
        num22 = num22 > -1.0 ? num22 * 1.5 : num22 * 4.0;
        double num23 = simplexNoise2.Noise3DFBM(num12 * num * 2.5, num13 * num2 * 8.0, num14 * num3 * 2.5, 2) * 0.6 - 0.3;
        double num24 = num21 * num8 + num23 + num9;
        double val = MathUtil.Levelize(num21 + 0.7);
        double num25 = simplexNoise.Noise3DFBM(num12 * num * modX, num13 * num2 * modX, num14 * num3 * modX, 6) * num4 + num5;
        double num26 = simplexNoise2.Noise3DFBM(num12 * 0.0025, num13 * 0.0025, num14 * 0.0025, 3) * num4 * num6 + num7;
        double num27 = num26 > 0.0 ? num26 * 0.5 : num26;
        double x = (num25 + num27 + 5.0) * 0.13;
        x = Math.Pow(x, 6.0) * 24.0 - 24.0;
        double num28 = num22 >= 0.0 - modY ? 0.0 : Math.Pow(Math.Min(Math.Abs(num22 + modY) / 5.0, 1.0), 1.0);
        double num15 = num22 * (1.0 - num28) + x * num28;
        num15 = num15 > 0.0 ? num15 * 0.5 : num15;
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num15 + 0.2) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        modX = planet.ModX;
        modY = planet.ModY;
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num10 = dotNet35Random.Next();
        int num11 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num10);
        simplexNoise2 = new SimplexNoise(num11);
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm10 : PlanetAlgorithm
{
    const int kCircleCount = 10;
    const double num = 0.007, num2 = 0.007, num3 = 0.007;

    readonly Vec4[] ellipses = new Vec4[kCircleCount];
    readonly double[] eccentricities = new double[kCircleCount];
    readonly double[] heights = new double[kCircleCount];
    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();
    SimplexNoise simplexNoise3 = new();
    SimplexNoise simplexNoise4 = new();

    static double Max(double a, double b) => a > b ? a : b;

    static double Remap(double sourceMin, double sourceMax, double targetMin, double targetMax, double x)
        => (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;

    public override void GenerateHeight1(int index)
    {
        double num9 = Vertices[index].X * NormalPlanetRadius;
        double num10 = Vertices[index].Y * NormalPlanetRadius;
        double num11 = Vertices[index].Z * NormalPlanetRadius;
        double num12 = MathUtil.Levelize(num9 * 0.007);
        double num13 = MathUtil.Levelize(num10 * 0.007);
        double num14 = MathUtil.Levelize(num11 * 0.007);
        num12 += simplexNoise3.Noise(num9 * 0.05, num10 * 0.05, num11 * 0.05) * 0.04;
        num13 += simplexNoise3.Noise(num10 * 0.05, num11 * 0.05, num9 * 0.05) * 0.04;
        num14 += simplexNoise3.Noise(num11 * 0.05, num9 * 0.05, num10 * 0.05) * 0.04;
        double num15 = Math.Abs(simplexNoise4.Noise(num12, num13, num14));
        double num16 = (0.16 - num15) * 10.0;
        num16 = !(num16 > 0.0) ? 0.0 : (num16 > 1.0 ? 1.0 : num16);
        num16 *= num16;
        double num17 = (simplexNoise3.Noise3DFBM(num10 * 0.005, num11 * 0.005, num9 * 0.005, 4) + 0.22) * 5.0;
        num17 = !(num17 > 0.0) ? 0.0 : (num17 > 1.0 ? 1.0 : num17);
        double num18 = Math.Abs(simplexNoise4.Noise3DFBM(num12 * 1.5, num13 * 1.5, num14 * 1.5, 2));
        double num19 = 0.0;
        double num21 = simplexNoise2.Noise3DFBM(num9 * num * 5.0, num10 * num2 * 5.0, num11 * num3 * 5.0, 4);
        double num22 = num21 * 0.2;
        double num23 = 0.0;
        for (int k = 0; k < kCircleCount; k++)
        {
            double num24 = ellipses[k].X - num9;
            double num25 = ellipses[k].Y - num10;
            double num26 = ellipses[k].Z - num11;
            double num27 = eccentricities[k] * num24 * num24 + num25 * num25 + num26 * num26;
            num27 = Remap(-1.0, 1.0, 0.2, 5.0, num21) * num27;
            if (!(num27 >= ellipses[k].W * ellipses[k].W))
            {
                double num28 = 1.0f - Mathf.Sqrt((float)(num27 / (ellipses[k].W * ellipses[k].W)));
                double num29 = 1.0 - num28;
                double num30 = 1.0 - num29 * num29 * num29 * num29 + num22 * 2.0;
                if (num30 < 0.0)
                    num30 = 0.0;
                num23 = Max(num23, heights[k] * num30);
            }
        }
        num9 += Math.Sin(num10 * 0.15) * 2.0;
        num10 += Math.Sin(num11 * 0.15) * 2.0;
        num11 += Math.Sin(num9 * 0.15) * 2.0;
        num9 *= num;
        num10 *= num2;
        num11 *= num3;
        double f = Mathf.Pow((float)((simplexNoise.Noise3DFBM(num9 * 0.6, num10 * 0.6, num11 * 0.6, 4, 0.5, 1.8) + 1.0) * 0.5), 1.3f);
        double x = simplexNoise2.Noise3DFBM(num9 * 6.0, num10 * 6.0, num11 * 6.0, 5);
        x = Remap(-1.0, 1.0, -0.1, 0.15, x);
        double num31 = simplexNoise2.Noise3DFBM(num9 * 5.0 * 3.0, num10 * 5.0, num11 * 5.0, 1);
        double num32 = simplexNoise2.Noise3DFBM(num9 * 5.0 * 3.0 + num31 * 0.3, num10 * 5.0 + num31 * 0.3, num11 * 5.0 + num31 * 0.3, 5) * 0.1;
        f = (float)MathUtil.Levelize(MathUtil.Levelize4(f));
        f = Math.Min(1.0, f);
        if (!(f > 0.8))
            f = !(f > 0.4) ? f + x : f + num32;
        double a = f * 2.5 - f * num23;
        num19 = Max(a, x * 2.0);
        double num33 = (2.0 - num19) / 2.0;
        num19 -= num16 * 1.2 * num17 * num33;
        if (num19 >= 0.0)
            num19 += (num15 * 0.25 + num18 * 0.6) * num33;
        num19 -= 0.1;
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num19 + 0.1) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num4 = dotNet35Random.Next();
        int num5 = dotNet35Random.Next();
        int num6 = dotNet35Random.Next();
        int num7 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num4);
        simplexNoise2 = new SimplexNoise(num5);
        simplexNoise3 = new SimplexNoise(num6);
        simplexNoise4 = new SimplexNoise(num7);
        int num8 = dotNet35Random.Next();

        for (int i = 0; i < kCircleCount; i++)
        {
            VecLF3 vectorLF = RandomTable.SphericNormalAt(ref num8, 1.0);
            var vector = new Vec4((float)vectorLF.X, (float)vectorLF.Y, (float)vectorLF.Z, 0.0f);
            float mag = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            vector.X /= mag; vector.Y /= mag; vector.Z /= mag;
            vector.X *= NormalPlanetRadius; vector.Y *= NormalPlanetRadius; vector.Z *= NormalPlanetRadius;
            vector.W = (float)(dotNet35Random.NextDouble() * 10.0 + 40.0);
            ellipses[i] = vector;
            if (dotNet35Random.NextDouble() > 0.5)
                eccentricities[i] = Remap(0.0, 1.0, 3.0, 5.0, dotNet35Random.NextDouble());
            else
                eccentricities[i] = Remap(0.0, 1.0, 0.2, 1.0 / 3.0, dotNet35Random.NextDouble());
            heights[i] = Remap(0.0, 1.0, 1.0, 2.0, dotNet35Random.NextDouble());
        }
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm11 : PlanetAlgorithm
{
    const double num = 0.007, num2 = 0.007, num3 = 0.007;

    double modY, num4, num5, num6;
    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();
    SimplexNoise simplexNoise3 = new();

    static double Remap(double sourceMin, double sourceMax, double targetMin, double targetMax, double x)
        => (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;

            public override bool CheckVeinPosition(Vec3 targetPos, EVeinType veinType)
        {
            float targetHeight = QueryHeight(targetPos);
            return targetHeight < NormalPlanetRadius
                || (veinType == EVeinType.Oil && targetHeight < NormalPlanetRadius + 0.5f)
                || ((int)veinType <= 2 && targetHeight > NormalPlanetRadius + 0.7f)
                || ((veinType == EVeinType.Silicium || veinType == EVeinType.Titanium) && targetHeight <= NormalPlanetRadius + 0.7f);
        }

public override void GenerateHeight1(int index)
    {
        double num10 = Vertices[index].X * NormalPlanetRadius;
        double num11 = Vertices[index].Y * NormalPlanetRadius;
        double num12 = Vertices[index].Z * NormalPlanetRadius;
        double num15 = simplexNoise2.Noise3DFBM(num10 * num * 4.0, num11 * num2 * 8.0, num12 * num3 * 4.0, 3);
        double x = simplexNoise.Noise3DFBM(num10 * num * 0.6, num11 * num * 1.5 * 2.5, num12 * num * 0.6, 6, 0.45, 1.8) * 0.95 + num15 * 0.05;
        x = Remap(-1.0, 1.0, 0.0, 1.0, x);
        x = Math.Pow(x, modY);
        x += 1.0;
        x = MathUtil.Levelize2(x);
        double x2 = simplexNoise3.Noise3DFBM(num10 * num4, num11 * num5, num12 * num6, 5, 0.55);
        x2 = Remap(-1.0, 1.0, 0.0, 1.0, x2);
        x2 = Math.Pow(x2, 0.65);
        double num14 = MathUtil.Levelize3(x2) * x;
        double num13 = (num14 - 0.4) * 0.9;
        num13 = Math.Max(-0.3, num13);
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num13) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        double modX = planet.ModX;
        modY = planet.ModY;
        num4 = 0.002 * modX;
        num5 = 0.002 * modX * 4.0;
        num6 = 0.002 * modX;
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num7 = dotNet35Random.Next();
        int num8 = dotNet35Random.Next();
        int num9 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num7);
        simplexNoise2 = new SimplexNoise(num8);
        simplexNoise3 = new SimplexNoise(num9);
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm12 : PlanetAlgorithm
{
    const double num2 = 0.2;
    const double num3 = 8.0;

    double num, modY;
    SimplexNoise simplexNoise = new();
    SimplexNoise simplexNoise2 = new();

    static double Remap(double sourceMin, double sourceMax, double targetMin, double targetMax, double x)
        => (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;

    static double CurveEvaluate(double t)
    {
        t /= 0.6;
        if (t >= 1.0)
            return 0.0;
        return Math.Pow(1.0 - t, 3.0) + Math.Pow(1.0 - t, 2.0) * 3.0 * t;
    }

            public override bool CheckVeinPosition(Vec3 targetPos, EVeinType veinType)
        {
            float targetHeight = QueryHeight(targetPos);
            return targetHeight < NormalPlanetRadius
                || (veinType == EVeinType.Oil && targetHeight < NormalPlanetRadius + 0.5f)
                || (veinType == EVeinType.Fireice && targetHeight < NormalPlanetRadius + 1.2f);
        }

public override void GenerateHeight1(int index)
    {
        double num6 = Math.Abs(Math.Asin(Vertices[index].Y)) * 2.0 / Math.PI;
        double num11 = Vertices[index].X;
        double num12 = (double)Vertices[index].Y * 2.5 * modY;
        double num13 = Vertices[index].Z;
        double num14 = simplexNoise2.Noise3DFBM(num11 * num, num12 * num, num13 * num, 3, 0.4) * 0.2;
        double num9 = simplexNoise.RidgedNoise(num11 * num, num12 * num - num14, num13 * num, 6, 0.7, 2.0, 0.8);
        double num10 = simplexNoise.Noise3DFBM(num11 * num, num12 * num - num14, num13 * num, 6, 0.6, 2.0, 0.7);
        num10 *= num9 + num10;
        num10 = num2 + num3 * num10 * num9;
        double x = num10 + 0.5;
        x = Remap(-8.0, 8.0, 0.0, 1.0, x);
        x = MathUtil.Clamp01(x);
        x += 0.5;
        x = Math.Pow(x, 1.5);
        x -= CurveEvaluate((float)(num6 * 0.9));
        double num7 = MathUtil.Clamp(x * 2.0, 0.0, 2.0);
        num7 = num7 * 1.1 - 0.2;
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num7) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        double modX = planet.ModX;
        modY = planet.ModY;
        num = 1.1 * modX;
        var dotNet35Random = new DotNet35Random(planet.Seed);
        int num4 = dotNet35Random.Next();
        int num5 = dotNet35Random.Next();
        simplexNoise = new SimplexNoise(num4);
        simplexNoise2 = new SimplexNoise(num5);
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public class PlanetAlgorithm13 : PlanetAlgorithm
{
    double num, num2, num3, modY;
    SimplexNoise simplexNoise = new();

    static double Remap(double sourceMin, double sourceMax, double targetMin, double targetMax, double x)
        => (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;

            public override bool CheckVeinPosition(Vec3 targetPos, EVeinType veinType)
        {
            float targetHeight = QueryHeight(targetPos);
            return targetHeight < NormalPlanetRadius
                || (veinType == EVeinType.Oil && targetHeight < NormalPlanetRadius + 0.5f)
                || ((int)veinType <= 4 && targetHeight > NormalPlanetRadius + 0.7f);
        }

public override void GenerateHeight1(int index)
    {
        double num4 = Vertices[index].X * NormalPlanetRadius;
        double num5 = Vertices[index].Y * NormalPlanetRadius;
        double num6 = Vertices[index].Z * NormalPlanetRadius;
        double x = Remap(-1.0, 1.0, 0.0, 1.0, simplexNoise.Noise3DFBM(num4 * num, num5 * num2, num6 * num3, 6));
        x = Math.Pow(x, modY) * 3.0625;
        x = Remap(0.0, 2.0, 0.0, 4.0, x);
        if (x < 1.0)
            x = Math.Pow(x, 2.0);
        x -= 0.2;
        double num8 = Math.Min(x, 4.0);
        if (num8 > 2.0)
            num8 = !(num8 > 3.0) ? 2.0 - 1.0 * (num8 - 2.0) : (!(num8 > 3.5) ? 1.0 : 1.0 + 2.0 * (num8 - 3.5));
        HeightData[index] = (ushort)(((double)NormalPlanetRadius + num8 + 0.1) * 100.0);
    }

    public override void GenerateTerrain(PlanetClass planet, bool genTerr = false)
    {
        double modX = planet.ModX;
        modY = planet.ModY;
        num = 0.007 * modX;
        num2 = 0.007 * modX;
        num3 = 0.007 * modX;
        simplexNoise = new SimplexNoise(new DotNet35Random(planet.Seed).Next());
        Array.Clear(HeightData, 0, VerticesDataLength);
    }
}

public static partial class PlanetAlgorithmFactory
{
    public static PlanetAlgorithm Create(int algoId) => algoId switch
    {
        1 => new PlanetAlgorithm1(),
        2 => new PlanetAlgorithm2(),
        3 => new PlanetAlgorithm3(),
        4 => new PlanetAlgorithm4(),
        5 => new PlanetAlgorithm5(),
        6 => new PlanetAlgorithm6(),
        7 => new PlanetAlgorithm7(),
        8 => new PlanetAlgorithm8(),
        9 => new PlanetAlgorithm9(),
        10 => new PlanetAlgorithm10(),
        11 => new PlanetAlgorithm11(),
        12 => new PlanetAlgorithm12(),
        13 => new PlanetAlgorithm13(),
        _ => new PlanetAlgorithm0(),
    };
}
