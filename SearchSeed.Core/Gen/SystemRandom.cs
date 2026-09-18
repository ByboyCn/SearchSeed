using System;

namespace SearchSeed.Core.Gen;

// Port of SystemRandom.hpp - used only for spheric normal table generation
public sealed class SystemRandom
{
    readonly int[] _seedArray = new int[56];
    int _inext, _inextp;

    public SystemRandom(int seed)
    {
        int num = 0;
        int num2 = 161803398 - (seed == int.MinValue ? int.MaxValue : Math.Abs(seed));
        _seedArray[55] = num2;
        int num3 = 1;
        for (int i = 1; i < 55; i++)
        {
            if ((num += 21) >= 55) num -= 55;
            _seedArray[num] = num3;
            num3 = num2 - num3;
            if (num3 < 0) num3 += int.MaxValue;
            num2 = _seedArray[num];
        }
        for (int j = 1; j < 5; j++)
        {
            for (int k = 1; k < 56; k++)
            {
                int num4 = k + 30;
                if (num4 >= 55) num4 -= 55;
                _seedArray[k] -= _seedArray[1 + num4];
                if (_seedArray[k] < 0) _seedArray[k] += int.MaxValue;
            }
        }
        _inext = 0;
        _inextp = 21;
    }

    int InternalSample()
    {
        int inext = _inext, inextp = _inextp;
        if (++inext >= 56) inext = 1;
        if (++inextp >= 56) inextp = 1;
        int num = _seedArray[inext] - _seedArray[inextp];
        if (num == int.MaxValue) num--;
        if (num < 0) num += int.MaxValue;
        _seedArray[inext] = num;
        _inext = inext;
        _inextp = inextp;
        return num;
    }

    public double NextDouble() => InternalSample() * 4.656612875245797E-10;
}
