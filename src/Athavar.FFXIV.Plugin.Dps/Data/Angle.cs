// <copyright file="Angle.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

// wrapper around float, stores angle in radians, provides type-safety and convenience
// when describing rotation in world, common convention is 0 for 'south'/'down'/(0, -1) and increasing counterclockwise - so +90 is 'east'/'right'/(1, 0)
internal struct Angle
{
    public const float RadToDeg = 180 / MathF.PI;
    public const float DegToRad = MathF.PI / 180;

    public float Rad;

    public Angle(float radians = 0) => this.Rad = radians;

    public float Deg => this.Rad * RadToDeg;

    public static Angle operator+(Angle a, Angle b) => new(a.Rad + b.Rad);

    public static Angle operator-(Angle a, Angle b) => new(a.Rad - b.Rad);

    public static Angle operator-(Angle a) => new(-a.Rad);

    public static Angle operator*(Angle a, float b) => new(a.Rad * b);

    public static Angle operator*(float a, Angle b) => new(a * b.Rad);

    public static Angle operator/(Angle a, float b) => new(a.Rad / b);

    public static Angle Asin(float x) => new(MathF.Asin(x));

    public static Angle Acos(float x) => new(MathF.Acos(x));

    public static bool operator==(Angle l, Angle r) => l.Rad == r.Rad;

    public static bool operator!=(Angle l, Angle r) => l.Rad != r.Rad;

    public Angle Abs() => new(Math.Abs(this.Rad));

    public float Sin() => MathF.Sin(this.Rad);

    public float Cos() => MathF.Cos(this.Rad);

    public float Tan() => MathF.Tan(this.Rad);

    public Angle Normalized()
    {
        var r = this.Rad;
        while (r < -MathF.PI)
        {
            r += 2 * MathF.PI;
        }

        while (r > MathF.PI)
        {
            r -= 2 * MathF.PI;
        }

        return new Angle(r);
    }

    public bool AlmostEqual(Angle other, float epsRad)
    {
        var delta = Math.Abs(this.Rad - other.Rad);
        return delta <= epsRad || delta >= (2 * MathF.PI) - epsRad;
    }

    public override bool Equals(object? obj) => obj is Angle && this == (Angle)obj;

    public override int GetHashCode() => this.Rad.GetHashCode();

    public override string ToString() => this.Deg.ToString("f0");
}

internal static class AngleExtensions
{
    public static Angle Radians(this float radians) => new(radians);

    public static Angle Degrees(this float degrees) => new(degrees * Angle.DegToRad);

    public static Angle Degrees(this int degrees) => new(degrees * Angle.DegToRad);
}