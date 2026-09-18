using System.Runtime.InteropServices;
using SearchSeed.Core.Maths;

namespace SearchSeed.Core.Gen;

// Bit-exact port of static_value.cpp: ucrtSinf/ucrtCosf/ucrtAcosf and Vector3::Slerp
// (Unity native math library, MSVC UCRT polynomial approximations)
public static class NativeMath
{
    const double PiOver4 = 0.7853981633974483;
    const double SmallCut = 0.0078125;
    const double TinyCut = 0.0001220703125;
    const double TwoOverPi = 0.6366197723675814;
    const double PiOver2Hi = 1.5707963267341256;
    const double PiOver2Lo = 6.077100506506192e-11;
    const double SinC9 = 2.7557319223985893e-06;
    const double SinC7 = -0.0001984126984126984;
    const double SinC5 = 0.008333333333333333;
    const double SinC3 = -0.16666666666666666;
    const double CosC8 = -2.755731922398589e-07;
    const double CosC6 = 2.4801587301587298e-05;
    const double CosC4 = -0.0013888888888888887;
    const double CosC2 = 0.041666666666666664;
    const double FastReduceLimit = 16779436.0;

    static double AbsBits(double value)
    {
        long bits = BitConverter.DoubleToInt64Bits(value);
        bits &= 0x7FFFFFFFFFFFFFFFL;
        return BitConverter.Int64BitsToDouble(bits);
    }

    static double SinPoly(double x)
    {
        double xx = x * x;
        double p = Math.FusedMultiplyAdd(xx, SinC9, SinC7);
        p = Math.FusedMultiplyAdd(p, xx, SinC5);
        p = Math.FusedMultiplyAdd(p, xx, SinC3);
        return Math.FusedMultiplyAdd(p, x * xx, x);
    }

    static int Reduce(double magnitude, out double reduced)
    {
        double scaled = Math.FusedMultiplyAdd(TwoOverPi, magnitude, 0.5);
        double n = (double)(int)scaled;
        double high = Math.FusedMultiplyAdd(-n, PiOver2Hi, magnitude);
        reduced = high - n * PiOver2Lo;
        return (int)scaled & 3;
    }

    static double CosPolyReduced(double x)
    {
        double xx = x * x;
        double b = Math.FusedMultiplyAdd(xx, -0.5, 1.0);
        double p = Math.FusedMultiplyAdd(xx, CosC8, CosC6);
        p = Math.FusedMultiplyAdd(p, xx, CosC4);
        p = Math.FusedMultiplyAdd(p, xx, CosC2);
        return Math.FusedMultiplyAdd(p, xx * xx, b);
    }

    static double CosPoly(double x)
    {
        double xx = x * x;
        double b = 1.0 - xx * 0.5;
        double p = Math.FusedMultiplyAdd(xx, CosC8, CosC6);
        p = Math.FusedMultiplyAdd(p, xx, CosC4);
        p = Math.FusedMultiplyAdd(p, xx, CosC2);
        return Math.FusedMultiplyAdd(p, xx * xx, b);
    }

    static bool Negative(double value) => BitConverter.DoubleToInt64Bits(value) >> 63 != 0;

    public static float UcrtSinf(float value)
    {
        double x = value;
        double magnitude = AbsBits(x);
        if (magnitude <= PiOver4)
        {
            if (magnitude >= SmallCut) return (float)SinPoly(x);
            if (magnitude < TinyCut) return value;
            return (float)Math.FusedMultiplyAdd(-(x * 0.5), x * x, x);
        }
        if (magnitude >= FastReduceLimit) return (float)Math.Sin(x);
        int region = Reduce(magnitude, out double reduced);
        double result = (region & 1) != 0 ? CosPolyReduced(reduced) : SinPoly(reduced);
        if ((((region == 2 || region == 3) ? 1 : 0) ^ (Negative(x) ? 1 : 0)) != 0) result = -result;
        return (float)result;
    }

    public static float UcrtCosf(float value)
    {
        double x = value;
        double magnitude = AbsBits(x);
        if (magnitude <= PiOver4)
        {
            if (magnitude >= SmallCut) return (float)CosPoly(x);
            if (magnitude < TinyCut) return 1.0f;
            return (float)Math.FusedMultiplyAdd(-(x * 0.5), x, 1.0);
        }
        if (magnitude >= FastReduceLimit) return (float)Math.Cos(x);
        int region = Reduce(magnitude, out double reduced);
        double result = (region & 1) != 0 ? SinPoly(reduced) : CosPolyReduced(reduced);
        if (region == 1 || region == 2) result = -result;
        return (float)result;
    }

    static float BitsToF(uint bits) => BitConverter.UInt32BitsToSingle(bits);
    static uint FToBits(float f) => BitConverter.SingleToUInt32Bits(f);

    public static float UcrtAcosf(float value)
    {
        uint raw = FToBits(value);
        uint exponent = (raw >> 23) & 0xffu;
        float magnitude = BitsToF(raw & 0x7fffffffu);
        if (exponent < 0x65u) return BitsToF(0x3fc90fdbu);
        if (exponent >= 0x7fu)
        {
            if (value == 1.0f) return 0.0f;
            if (value == -1.0f) return BitsToF(0x40490fdbu);
            return float.NaN;
        }
        float z, root = 0.0f;
        if (exponent < 0x7eu) z = magnitude * magnitude;
        else
        {
            float oneMinus = 1.0f - magnitude;
            z = oneMinus * 0.5f;
            root = MathF.Sqrt(z);
        }
        float p0 = BitsToF(0xbc5b3fe1u), p1 = BitsToF(0x3b81ce6bu), p2 = BitsToF(0x3d678bddu), p3 = BitsToF(0x3e3c94dcu);
        float d0 = BitsToF(0x3f8d6fa5u), d1 = BitsToF(0x3f561f0du);
        float numerator = p0 - z * p1;
        numerator = numerator * z;
        numerator = numerator - p2;
        numerator = numerator * z;
        numerator = numerator + p3;
        numerator = numerator * z;
        float denominator = d0 - z * d1;
        float ratio = numerator / denominator;

        const double piOver2Low = 6.123233995736766e-17;
        const double piOver2 = 1.5707963267948966;
        const double pi = 3.141592653589793;
        if (exponent < 0x7eu)
        {
            float product = ratio * value;
            return (float)(piOver2 - ((double)value - (piOver2Low - (double)product)));
        }
        if (value >= 0.0f)
        {
            float high = BitsToF(FToBits(root) & 0xffff0000u);
            float twiceRoot = root + root;
            float correction = (z - high * high) / (high + root);
            twiceRoot = twiceRoot * ratio;
            float twiceCorrection = correction + correction;
            float result = twiceRoot + twiceCorrection;
            result = result + high * 2.0f;
            return result;
        }
        float product2 = root * ratio;
        double inner = (double)root + ((double)product2 - piOver2Low);
        return (float)(pi - (inner + inner));
    }

    static Vec3 NativeSlerpOrthonormal(Vec3 value)
    {
        float threshold = BitsToF(0x3f3504f3u);
        float absZ = MathF.Abs(value.Z);
        if (absZ <= threshold)
        {
            float xSquared = value.X * value.X;
            float ySquared = value.Y * value.Y;
            float lengthSquared = xSquared + ySquared;
            float length = MathF.Sqrt(lengthSquared);
            float inverse = 1.0f / length;
            return new Vec3(-value.Y * inverse, value.X * inverse, 0.0f);
        }
        float zSquared = value.Z * value.Z;
        float ySquared2 = value.Y * value.Y;
        float lengthSquared2 = zSquared + ySquared2;
        float length2 = MathF.Sqrt(lengthSquared2);
        float inverse2 = 1.0f / length2;
        return new Vec3(0.0f, -value.Z * inverse2, value.Y * inverse2);
    }

    static Vec3 NativeRotateAroundAxis(Vec3 value, Vec3 axis, float angle, float magnitude)
    {
        float sine = UcrtSinf(angle);
        float cosine = UcrtCosf(angle);
        float oneMinusCosine = 1.0f - cosine;

        float x = axis.X, y = axis.Y, z = axis.Z;
        float xy = y * x, xz = z * x, yz = z * y;
        float xs = x * sine, ys = y * sine, zs = z * sine;

        float m0 = x * x * oneMinusCosine + cosine;
        float qxy = oneMinusCosine * xy;
        float m3 = qxy - zs;
        float m1 = qxy + zs;
        float m6 = oneMinusCosine * xz + ys;
        float m4 = y * y * oneMinusCosine + cosine;
        float m8 = z * z * oneMinusCosine + cosine;
        float m2 = oneMinusCosine * xz - ys;
        float m7 = oneMinusCosine * yz - xs;
        float m5 = oneMinusCosine * yz + xs;

        float rx = m3 * value.Y + m0 * value.X + m6 * value.Z;
        float ry = m4 * value.Y + m1 * value.X + m7 * value.Z;
        float rz = m5 * value.Y + m2 * value.X + m8 * value.Z;
        return new Vec3(rx * magnitude, ry * magnitude, rz * magnitude);
    }

    static Vec3 NativeLerp(Vec3 a, Vec3 b, float t) => b * t + a * (1 - t);

    public static Vec3 Slerp(Vec3 a, Vec3 b, float t)
    {
        if (0.0f > t) t = 0.0f;
        else t = MathF.Min(1.0f, t);

        float axSquared = a.X * a.X;
        float aySquared = a.Y * a.Y;
        float azSquared = a.Z * a.Z;
        float lengthSquaredA = axSquared + aySquared + azSquared;
        float lengthA = MathF.Sqrt(lengthSquaredA);

        float bxSquared = b.X * b.X;
        float bySquared = b.Y * b.Y;
        float bzSquared = b.Z * b.Z;
        float lengthSquaredB = bxSquared + bySquared + bzSquared;
        float lengthB = MathF.Sqrt(lengthSquaredB);

        float inverse = 1.0f - t;
        if (lengthA < 1e-5f || lengthB < 1e-5f) return NativeLerp(a, b, t);

        float magnitudeA = inverse * lengthA;
        float magnitudeB = lengthB * t;
        float magnitude = magnitudeA + magnitudeB;

        float dotX = b.X * a.X;
        float dotY = b.Y * a.Y;
        float dotXY = dotX + dotY;
        float dotZ = b.Z * a.Z;
        float numerator = dotXY + dotZ;
        float denominator = lengthB * lengthA;
        float cosine = numerator / denominator;
        float threshold = BitsToF(0x3f7fff58u);
        if (cosine > threshold) return NativeLerp(a, b, t);

        Vec3 normalA = new(a.X / lengthA, a.Y / lengthA, a.Z / lengthA);
        if (cosine < -threshold)
        {
            Vec3 axis = NativeSlerpOrthonormal(normalA);
            float angle = t * MathF.PI;
            return NativeRotateAroundAxis(normalA, axis, angle, magnitude);
        }

        float crossX = b.Z * a.Y - b.Y * a.Z;
        float crossY = b.X * a.Z - b.Z * a.X;
        float crossZ = b.Y * a.X - b.X * a.Y;
        float crossYSquared = crossY * crossY;
        float crossXSquared = crossX * crossX;
        float crossLengthSquared = crossYSquared + crossXSquared + crossZ * crossZ;
        float crossLength = MathF.Sqrt(crossLengthSquared);
        Vec3 axis2 = new(crossX / crossLength, crossY / crossLength, crossZ / crossLength);
        float angle2 = UcrtAcosf(cosine) * t;
        return NativeRotateAroundAxis(normalA, axis2, angle2, magnitude);
    }
}
