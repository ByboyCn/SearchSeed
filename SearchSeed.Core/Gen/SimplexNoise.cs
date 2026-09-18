namespace SearchSeed.Core.Gen;

// Port of SimplexNoise.hpp scalar implementation
public sealed class SimplexNoise
{
    static readonly double[,] Grad3 =
    {
        {1,1,0},{-1,1,0},{1,-1,0},{-1,-1,0},
        {1,0,1},{-1,0,1},{1,0,-1},{-1,0,-1},
        {0,1,1},{0,-1,1},{0,1,-1},{0,-1,-1}
    };
    const double F3 = 1.0 / 3.0;
    const double G3 = 1.0 / 6.0;

    public readonly int[] Perm = new int[512];
    public readonly int[] PermMod12 = new int[512];

    public SimplexNoise() { }
    public SimplexNoise(int seed) => Init(seed);

    void Init(int seed)
    {
        var p = new short[256];
        for (int i = 0; i < 256; i++) p[i] = (short)i;
        var rnd = new DotNet35Random(seed);
        for (int j = 0; j < 256; j++)
        {
            int num = rnd.Next(0, 256);
            short num2 = p[j];
            p[j] = p[num];
            p[num] = num2;
        }
        for (int k = 0; k < 512; k++)
        {
            Perm[k] = p[k & 0xFF];
            PermMod12[k] = (short)(Perm[k] % 12);
        }
    }

    static int FastFloor(double x) => (int)System.Math.Floor(x);

    static double Dot(int g, double x, double y, double z) => Grad3[g, 0] * x + Grad3[g, 1] * y + Grad3[g, 2] * z;

    public double Noise(double x, double y, double z)
    {
        double num = (x + y + z) * F3;
        int num2 = FastFloor(x + num);
        int num3 = FastFloor(y + num);
        int num4 = FastFloor(z + num);
        double num5 = (num2 + num3 + num4) * G3;
        double num6 = num2 - num5;
        double num7 = num3 - num5;
        double num8 = num4 - num5;
        double num9 = x - num6;
        double num10 = y - num7;
        double num11 = z - num8;

        int num12, num13, num14, num15, num16, num17;
        if (num9 >= num10)
        {
            if (num10 >= num11) { num12 = 1; num13 = 0; num14 = 0; num15 = 1; num16 = 1; num17 = 0; }
            else if (num9 >= num11) { num12 = 1; num13 = 0; num14 = 0; num15 = 1; num16 = 0; num17 = 1; }
            else { num12 = 0; num13 = 0; num14 = 1; num15 = 1; num16 = 0; num17 = 1; }
        }
        else if (num10 < num11) { num12 = 0; num13 = 0; num14 = 1; num15 = 0; num16 = 1; num17 = 1; }
        else if (num9 < num11) { num12 = 0; num13 = 1; num14 = 0; num15 = 0; num16 = 1; num17 = 1; }
        else { num12 = 0; num13 = 1; num14 = 0; num15 = 1; num16 = 1; num17 = 0; }

        double num18 = num9 - num12 + G3;
        double num19 = num10 - num13 + G3;
        double num20 = num11 - num14 + G3;
        double num21 = num9 - num15 + 2.0 * G3;
        double num22 = num10 - num16 + 2.0 * G3;
        double num23 = num11 - num17 + 2.0 * G3;
        double num24 = num9 - 1.0 + 3.0 * G3;
        double num25 = num10 - 1.0 + 3.0 * G3;
        double num26 = num11 - 1.0 + 3.0 * G3;
        int num27 = num2 & 0xFF;
        int num28 = num3 & 0xFF;
        int num29 = num4 & 0xFF;
        int num30 = PermMod12[num27 + Perm[num28 + Perm[num29]]];
        int num31 = PermMod12[num27 + num12 + Perm[num28 + num13 + Perm[num29 + num14]]];
        int num32 = PermMod12[num27 + num15 + Perm[num28 + num16 + Perm[num29 + num17]]];
        int num33 = PermMod12[num27 + 1 + Perm[num28 + 1 + Perm[num29 + 1]]];

        double num34 = 0.6 - num9 * num9 - num10 * num10 - num11 * num11;
        double num35;
        if (num34 < 0.0) num35 = 0.0;
        else { num34 *= num34; num35 = num34 * num34 * Dot(num30, num9, num10, num11); }
        double num36 = 0.6 - num18 * num18 - num19 * num19 - num20 * num20;
        double num37;
        if (num36 < 0.0) num37 = 0.0;
        else { num36 *= num36; num37 = num36 * num36 * Dot(num31, num18, num19, num20); }
        double num38 = 0.6 - num21 * num21 - num22 * num22 - num23 * num23;
        double num39;
        if (num38 < 0.0) num39 = 0.0;
        else { num38 *= num38; num39 = num38 * num38 * Dot(num32, num21, num22, num23); }
        double num40 = 0.6 - num24 * num24 - num25 * num25 - num26 * num26;
        double num41;
        if (num40 < 0.0) num41 = 0.0;
        else { num40 *= num40; num41 = num40 * num40 * Dot(num33, num24, num25, num26); }
        double total = num35 + num37 + num39 + num41;

        return 32.696434 * total;
    }

    public double Noise3DFBM(double x, double y, double z, int nOctaves, double deltaAmp = 0.5, double deltaWLen = 2.0, double initialAmp = 0.5)
    {
        double num = 0.0, num2 = initialAmp;
        for (int i = 0; i < nOctaves; i++)
        {
            num += Noise(x, y, z) * num2;
            num2 *= deltaAmp;
            x *= deltaWLen; y *= deltaWLen; z *= deltaWLen;
        }
        return num;
    }

    public double RidgedNoise(double x, double y, double z, int nOctaves, double deltaAmp = 0.5, double deltaWLen = 2.0, double initialAmp = 0.5)
    {
        double num = 0.0, num2 = initialAmp;
        for (int i = 0; i < nOctaves; i++)
        {
            num += System.Math.Abs(Noise(x, y, z) * num2);
            num2 *= deltaAmp;
            x *= deltaWLen; y *= deltaWLen; z *= deltaWLen;
        }
        return num;
    }
}
