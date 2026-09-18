namespace SearchSeed.Core.Gen;

// Exact port of DotNet35Random.hpp scalar implementation (matches the game's RNG consumption)
public sealed class DotNet35Random
{
    readonly int[] _seedArray = new int[56];
    int _inext = 0, _inextp = 31;

    public DotNet35Random(int seed)
    {
        int num = 161803398 - System.Math.Abs(seed);
        _seedArray[55] = num;
        int num2 = 1;
        for (int i = 1; i < 55; i++)
        {
            int num3 = 21 * i % 55;
            _seedArray[num3] = num2;
            num2 = num - num2;
            if (num2 < 0) num2 += 2147483647;
            num = _seedArray[num3];
        }
        for (int j = 1; j < 5; j++)
        {
            for (int k = 1; k < 56; k++)
            {
                _seedArray[k] -= _seedArray[1 + (k + 30) % 55];
                if (_seedArray[k] < 0) _seedArray[k] += 2147483647;
            }
        }
    }

    public double NextDouble() => Sample();

    double Sample()
    {
        if (++_inext >= 56) _inext = 1;
        if (++_inextp >= 56) _inextp = 1;
        int num = _seedArray[_inext] - _seedArray[_inextp];
        if (num < 0) num += 2147483647;
        _seedArray[_inext] = num;
        return (double)num * 4.6566128752457969E-10;
    }

    public int Next() => (int)(Sample() * 2147483647.0);

    public int Next(int maxValue) => (int)(Sample() * (double)maxValue);

    public int Next(int minValue, int maxValue)
    {
        uint num = (uint)(maxValue - minValue);
        if (num <= 1) return minValue;
        return (int)((uint)(Sample() * num) + minValue);
    }
}
