// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System.Numerics;
using DotnetNoise;

namespace CSharp.Core.Extensions;

public static class NumberExtensions
{
    private static readonly FastNoise Noise = new FastNoise();
    
    public static int Clamp(this int f, int min, int max) =>
        Math.Max(min, Math.Min(max, f));
    
    public static double Clamp(this double f, double min, double max) =>
        Math.Max(min, Math.Min(max, f));

    public static float Clamp(this float f, float min, float max) =>
        MathF.Max(min, MathF.Min(max, f));

    public static double Lerp(this double f, double from, double to) =>
        from * (1.0 - f) + to * f;
    
    public static float Lerp(this float f, float from, float to) =>
        from * (1.0f - f) + to * f;

    public static Rgb Lerp(this double f, Rgb from, Rgb to)
    {
        var r = f.Lerp(from.R, to.R);
        var g = f.Lerp(from.G, to.G);
        var b = f.Lerp(from.B, to.B);
        return new Rgb((byte)r.Clamp(0, 255), (byte)g.Clamp(0, 255), (byte)b.Clamp(0, 255));
    }

    public static double InverseLerp(this double f, double from, double to)
    {
        if (Math.Abs(from - to) < 0.001)
            return 0.0; // Avoid division by zero
        return (f - from) / (to - from);
    }

    public static float InverseLerp(this float f, float from, float to)
    {
        if (Math.Abs(from - to) < 0.001f)
            return 0.0f; // Avoid division by zero
        return (f - from) / (to - from);
    }

    /// <summary>
    /// Smooth noise function that returns value between 0.0 and 1.0.
    /// </summary>
    public static double SmoothNoise(this (double X, double Y) f) =>
        Noise.GetSimplex((float)f.X, (float)f.Y) * 0.5 + 0.5;

    /// <summary>
    /// Smooth noise function that returns value between 0.0 and 1.0.
    /// </summary>
    public static float SmoothNoise(this (float X, float Y) f) =>
        Noise.GetSimplex(f.X, f.Y) * 0.5f + 0.5f;

    /// <summary>
    /// Smooth noise function that returns value between 0.0 and 1.0.
    /// </summary>
    public static double SmoothNoise(this double f) =>
        SmoothNoise((f, 1.0));

    /// <summary>
    /// Smooth noise function that returns value between 0.0 and 1.0.
    /// </summary>
    public static float SmoothNoise(this float f) =>
        SmoothNoise((f, 1.0f));

    /// <summary>
    /// Computes the cross product of two 2D vectors.
    /// </summary>
    public static float Cross(this Vector2 a, Vector2 b) =>
        a.X * b.Y - a.Y * b.X;

    public static Vector3 Rotate(this Vector3 v, Vector3 r)
    {
        var cosX = MathF.Cos(r.X);
        var sinX = MathF.Sin(r.X);
        var cosY = MathF.Cos(r.Y);
        var sinY = MathF.Sin(r.Y);
        var cosZ = MathF.Cos(r.Z);
        var sinZ = MathF.Sin(r.Z);

        var y1 = v.Y * cosX - v.Z * sinX;
        var z1 = v.Y * sinX + v.Z * cosX;
        var x2 = v.X * cosY + z1 * sinY;
        var z2 = -v.X * sinY + z1 * cosY;
        var x3 = x2 * cosZ - y1 * sinZ;
        var y3 = x2 * sinZ + y1 * cosZ;

        return new Vector3(x3, y3, z2);
    }
    
    public static char ToAscii(this double f)
    {
        const string gradient = " .,;ilS8$@";
        var index = (int)Math.Round(f.Clamp(0.0, 1.0) * (gradient.Length - 1));
        return gradient[index];
    }

    /// <summary>
    /// Convert a byte count to human-readable string.
    /// </summary>
    public static string ToSize(this long bytes)
    {
        if (bytes < 4096)
            return $"{bytes:N0} bytes";
        string[] sizes =
        [
            "bytes", "KB", "MB", "GB", "TB"
        ];

        var order = (int)Math.Log(bytes, 1024);
        var formattedValue = bytes / Math.Pow(1024, order);
        return $"{formattedValue:N2} {sizes[order]}";
    }
}