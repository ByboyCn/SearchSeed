using System;
using SearchSeed.Core.Maths;

namespace SearchSeed.Core.Gen;

public static class RandomTable
{
    public static readonly VecLF3[] SphericNormal = new VecLF3[65536];
    static bool _init;

    public static double Normal(SystemRandom rand)
    {
        double num = rand.NextDouble();
        double a = rand.NextDouble() * Math.PI * 2.0;
        return Math.Sqrt(-2.0 * Math.Log(1.0 - num)) * Math.Sin(a);
    }

    public static VecLF3 SphericNormalAt(ref int seed, double scale)
    {
        seed = (seed + 1) & 65535;
        var v = SphericNormal[seed];
        return new VecLF3(v.X * scale, v.Y * scale, v.Z * scale);
    }

    public static void GenerateSphericNormal()
    {
        if (_init) return;
        _init = true;
        var random = new SystemRandom(1001);
        for (int i = 0; i < 65536; i++)
        {
            double num4, num5, num, num2, num3;
            while (true)
            {
                num = random.NextDouble() * 2.0 - 1.0;
                num2 = random.NextDouble() * 2.0 - 1.0;
                num3 = random.NextDouble() * 2.0 - 1.0;
                num4 = Normal(random);
                if (num4 > 5.0 || num4 < -5.0) continue;
                num5 = num * num + num2 * num2 + num3 * num3;
                if (num5 > 1.0 || num5 < 1E-06) continue;
                break;
            }
            double num6 = num4 / Math.Sqrt(num5);
            SphericNormal[i] = new VecLF3(num * num6, num2 * num6, num3 * num6);
        }
    }
}
