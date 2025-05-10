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

using System;
using System.Collections.Generic;
using System.Numerics;
using DotnetNoise;

namespace DTC.Core.Extensions;

public static class NumberExtensions
{
    private static readonly FastNoise Noise = new FastNoise();
    
    public static int Clamp(this int f, int min, int max) =>
        Math.Clamp(f, min, max);
    
    public static double Clamp(this double f, double min, double max) =>
        Math.Clamp(f, min, max);

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
    public static double SmoothNoise(this (double X, double Y, double Z) f) =>
        Noise.GetSimplex((float)f.X, (float)f.Y, (float)f.Z) * 0.5 + 0.5;

    /// <summary>
    /// Smooth noise function that returns value between 0.0 and 1.0.
    /// </summary>
    public static float SmoothNoise(this (float X, float Y) f) =>
        Noise.GetSimplex(f.X, f.Y) * 0.5f + 0.5f;
    
    /// <summary>
    /// Smooth noise function that returns value between 0.0 and 1.0.
    /// </summary>
    public static float SmoothNoise(this (int X, int Y) f) =>
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

    // Rotate around Z-axis (XY plane)
    public static Matrix4x4 RotateXy(this Matrix4x4 matrix, float angle) =>
        Matrix4x4.CreateRotationZ(angle) * matrix;

    // Rotate around X-axis (YZ plane)
    public static Matrix4x4 RotateYz(this Matrix4x4 matrix, float angle) =>
        Matrix4x4.CreateRotationX(angle) * matrix;

    // Rotate around Y-axis (XZ plane)
    public static Matrix4x4 RotateXz(this Matrix4x4 matrix, float angle) =>
        Matrix4x4.CreateRotationY(angle) * matrix;

    // Translate (move in X, Y, Z)
    public static Matrix4x4 Translate(this Matrix4x4 matrix, float x, float y, float z) =>
        Matrix4x4.CreateTranslation(x, y, z) * matrix;

    public static Matrix4x4 Scale(this Matrix4x4 matrix, float f) =>
        Matrix4x4.CreateScale(f) * matrix;

    public static char ToAscii(this double f)
    {
        const string gradient = " .,;•il!S8@";
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
    
    /// <summary>
    /// Zero radians => Right. Positive is clockwise.
    /// </summary>
    public static Vector2 ToDirection(this float theta) => new Vector2(MathF.Cos(theta), MathF.Sin(theta));
    
    /// <summary>
    /// Zero radians => Right. Positive is clockwise.
    /// </summary>
    public static Vector2 ToDirection(this double theta) => new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
    
    public static string ToProgressBar(this double f, int width, char fill = '█', char empty = '░')
    {
        var filledWidth = (int)(width * f.Clamp(0.0, 1.0));
        return new string(fill, filledWidth) + new string(empty, width - filledWidth);
    }
}

public static class RandomExtensions
{
    public static bool NextBool(this Random rand) =>
        rand.NextDouble() > 0.5;

    public static float NextFloat(this Random rand) =>
        (float)rand.NextDouble();

    /// <summary>
    /// Returns a single sample (+/-) from a Gaussian distribution with given mean and standard deviation.
    /// </summary>
    public static double GaussianSample(this Random rand, double stdDev = 1.0)
    {
        var u1 = 1.0 - rand.NextDouble();
        var u2 = 1.0 - rand.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(Math.Tau * u2); // Box-Muller.
        return stdDev * randStdNormal;
    }

    public static T RouletteSelection<T>(this Random rand, IList<T> items, Func<T, double> weightSelector)
    {
        var sum = items.FastSum(weightSelector);
        if (sum == 0.0)
            throw new InvalidOperationException("Sequence sum is zero.");
        
        var r = rand.NextDouble(); // between 0.0 and 1.0
        var index = 0;

        while (r > 0 && index < items.Count)
        {
            r -= weightSelector(items[index]) / sum;
            index++;
        }

        index = Math.Max(0, index - 1); // Avoid out of bounds.
        return items[index];
    }
}