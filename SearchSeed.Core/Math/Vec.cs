using System;
using System.Numerics;

namespace SearchSeed.Core.Maths;

// Unity-style float vectors
public struct Vec2(float x, float y)
{
    public float X = x, Y = y;
    public static readonly Vec2 Zero = new(0, 0);
    public static readonly Vec2 One = new(1, 1);
    public readonly float Magnitude => MathF.Sqrt(X * X + Y * Y);
    public readonly float SqrMagnitude => X * X + Y * Y;
    public readonly Vec2 Normalized { get { float m = Magnitude; return m > 1e-6f ? new Vec2(X / m, Y / m) : Zero; } }
    public void Normalize() { float m = Magnitude; if (m > 1e-6f) { X /= m; Y /= m; } }
    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator -(Vec2 a) => new(-a.X, -a.Y);
    public static Vec2 operator *(Vec2 a, float d) => new(a.X * d, a.Y * d);
    public static Vec2 operator *(float d, Vec2 a) => a * d;
    public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;
}

public struct Vec3(float x, float y, float z)
{
    public float X = x, Y = y, Z = z;
    public static readonly Vec3 Zero = new(0, 0, 0);
    public static readonly Vec3 Up = new(0, 1, 0);
    public static readonly Vec3 Right = new(1, 0, 0);
    public static readonly Vec3 Forward = new(0, 0, 1);
    public readonly float Magnitude => MathF.Sqrt(X * X + Y * Y + Z * Z);
    public readonly float SqrMagnitude => X * X + Y * Y + Z * Z;
    public readonly Vec3 Normalized { get { float m = Magnitude; return m > 1e-6f ? new Vec3(X / m, Y / m, Z / m) : Zero; } }
    public void Normalize() { float m = Magnitude; if (m > 1e-6f) { X /= m; Y /= m; Z /= m; } }
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator -(Vec3 a) => new(-a.X, -a.Y, -a.Z);
    public static Vec3 operator *(Vec3 a, float d) => new(a.X * d, a.Y * d, a.Z * d);
    public static Vec3 operator *(float d, Vec3 a) => a * d;
    public static float Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public static Vec3 Cross(Vec3 a, Vec3 b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    public static Vec3 Normalize(Vec3 v) => v.Normalized;
    public static Vec3 Lerp(Vec3 a, Vec3 b, float t) => a + (b - a) * t;
    public readonly VecLF3 ToVecLF3() => new(X, Y, Z);
    public static float AngleDeg(Vec3 a, Vec3 b)
    {
        float d = Dot(a.Normalized, b.Normalized);
        d = Math.Clamp(d, -1f, 1f);
        return MathF.Acos(d) * 57.29578f;
    }
}

public struct Vec4(float x, float y, float z, float w)
{
    public float X = x, Y = y, Z = z, W = w;
}

// double precision vector (VectorLF3)
public struct VecLF3(double x, double y, double z)
{
    public double X = x, Y = y, Z = z;
    public static readonly VecLF3 Zero = new(0, 0, 0);
    public readonly double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
    public readonly double SqrMagnitude => X * X + Y * Y + Z * Z;
    public readonly VecLF3 Normalized { get { double m = Magnitude; return m > 1e-12 ? new VecLF3(X / m, Y / m, Z / m) : Zero; } }
    public static VecLF3 operator +(VecLF3 a, VecLF3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static VecLF3 operator -(VecLF3 a, VecLF3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static VecLF3 operator *(VecLF3 a, double d) => new(a.X * d, a.Y * d, a.Z * d);
    public static double Dot(VecLF3 a, VecLF3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public readonly Vec3 ToVec3() => new((float)X, (float)Y, (float)Z);
    public static VecLF3 Cross(VecLF3 a, VecLF3 b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
}

// Unity-style quaternion
public struct Quat(float x, float y, float z, float w)
{
    public float X = x, Y = y, Z = z, W = w;
    public static readonly Quat Identity = new(0, 0, 0, 1);
    public readonly Quat Normalized { get { float m = MathF.Sqrt(X*X+Y*Y+Z*Z+W*W); return m > 1e-6f ? new Quat(X/m,Y/m,Z/m,W/m) : Identity; } }
    public static Quat operator *(Quat a, Quat b) => new(
        a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
        a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
        a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
        a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
    public static Vec3 operator *(Quat q, Vec3 v)
    {
        float x2 = q.X * 2, y2 = q.Y * 2, z2 = q.Z * 2;
        float xx = q.X * x2, yy = q.Y * y2, zz = q.Z * z2;
        float xy = q.X * y2, xz = q.X * z2, yz = q.Y * z2;
        float wx = q.W * x2, wy = q.W * y2, wz = q.W * z2;
        return new Vec3(
            (1 - (yy + zz)) * v.X + (xy - wz) * v.Y + (xz + wy) * v.Z,
            (xy + wz) * v.X + (1 - (xx + zz)) * v.Y + (yz - wx) * v.Z,
            (xz - wy) * v.X + (yz + wx) * v.Y + (1 - (xx + yy)) * v.Z);
    }
    public static Quat AngleAxis(float deg, Vec3 axis)
    {
        axis.Normalize();
        double rad = deg * System.Math.PI / 180.0 / 2.0;
        float s = (float)System.Math.Sin(rad);
        return new Quat(axis.X * s, axis.Y * s, axis.Z * s, (float)System.Math.Cos(rad));
    }
    // equivalent of glm::rotation(from, to)
    public static Quat FromToRotation(Vec3 from, Vec3 to)
    {
        var f = from.Normalized; var t = to.Normalized;
        float d = Vec3.Dot(f, t);
        if (d >= 1f) return Identity;
        if (d <= -1f)
        {
            var ortho = MathF.Abs(f.X) < 0.6f ? new Vec3(1, 0, 0) : new Vec3(0, 1, 0);
            var axis = Vec3.Cross(f, ortho).Normalized;
            return new Quat(axis.X, axis.Y, axis.Z, 0);
        }
        var c = Vec3.Cross(f, t);
        float s = MathF.Sqrt((1 + d) * 2);
        return new Quat(c.X / s, c.Y / s, c.Z / s, s * 0.5f).Normalized;
    }
}

public static class Mathf
{
    public static float Pow(float f, float p) => MathF.Pow(f, p);
    public static int RoundToInt(float f) => (int)MathF.Round(f, MidpointRounding.AwayFromZero);
    public static int CeilToInt(float f) => (int)MathF.Ceiling(f);
    public static int FloorToInt(float f) => (int)MathF.Floor(f);
    public static float Round(float f) => MathF.Round(f, MidpointRounding.AwayFromZero);
    public static float Sqrt(float f) => MathF.Sqrt(f);
    public static float Clamp(float v, float min, float max) => v < min ? min : v > max ? max : v;
    public static float Lerp(float a, float b, float t) => a + (b - a) * t;
    public static float Log(float f) => MathF.Log(f);
    public static float Log10(float f) => MathF.Log10(f);
    public static float Max(float a, float b) => a > b ? a : b;
    public static float Min(float a, float b) => a < b ? a : b;
    public static float Abs(float f) => MathF.Abs(f);
    public static float Clamp01(float f) => f < 0 ? 0 : f > 1 ? 1 : f;
    public static float Ceil(float f) => MathF.Ceiling(f);
    public static float Sin(float f) => MathF.Sin(f);
    public static float Cos(float f) => MathF.Cos(f);
}

public static class MathUtil
{
    public static double Clamp(double val, double min, double max) => val < min ? min : val > max ? max : val;
    public static double Clamp01(double val) => val < 0 ? 0 : val > 1 ? 1 : val;

    public static double Levelize(double f, double level = 1.0, double offset = 0.0)
    {
        f = f / level - offset;
        double num = System.Math.Floor(f);
        double num2 = f - num;
        num2 = (3.0 - num2 - num2) * num2 * num2;
        f = num + num2;
        return (f + offset) * level;
    }
    public static double Levelize2(double f, double level = 1.0, double offset = 0.0) => LevelizeN(f, 2, level, offset);
    public static double Levelize3(double f, double level = 1.0, double offset = 0.0) => LevelizeN(f, 3, level, offset);
    public static double Levelize4(double f, double level = 1.0, double offset = 0.0) => LevelizeN(f, 4, level, offset);
    static double LevelizeN(double f, int n, double level, double offset)
    {
        f = f / level - offset;
        double num = System.Math.Floor(f);
        double num2 = f - num;
        for (int i = 0; i < n; i++) num2 = (3.0 - num2 - num2) * num2 * num2;
        f = num + num2;
        return (f + offset) * level;
    }

    public static Vec3 QRotate(Quat q, Vec3 v)
    {
        v.X *= 2; v.Y *= 2; v.Z *= 2;
        float num = q.W * q.W - 0.5f;
        float num2 = q.X * v.X + q.Y * v.Y + q.Z * v.Z;
        return new Vec3(
            v.X * num + (q.Y * v.Z - q.Z * v.Y) * q.W + q.X * num2,
            v.Y * num + (q.Z * v.X - q.X * v.Z) * q.W + q.Y * num2,
            v.Z * num + (q.X * v.Y - q.Y * v.X) * q.W + q.Z * num2);
    }

    public static VecLF3 QInvRotateLF(Quat q, VecLF3 v)
    {
        v.X *= 2; v.Y *= 2; v.Z *= 2;
        double num = q.W * (double)q.W - 0.5;
        double num2 = q.X * v.X + q.Y * v.Y + q.Z * v.Z;
        return new VecLF3(
            v.X * num - (q.Y * v.Z - q.Z * v.Y) * q.W + q.X * num2,
            v.Y * num - (q.Z * v.X - q.X * v.Z) * q.W + q.Y * num2,
            v.Z * num - (q.X * v.Y - q.Y * v.X) * q.W + q.Z * num2);
    }
}
