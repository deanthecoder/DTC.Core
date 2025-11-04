// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any
//  purpose.
// 
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
// 
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.
using System;

namespace DTC.Core;

public readonly struct IntPoint : IEquatable<IntPoint>
{
    public static IntPoint Zero { get; } = new IntPoint(0, 0);
    
    public int X { get; }
    public int Y { get; }

    public IntPoint(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj) =>
        obj is IntPoint other && Equals(other);

    public bool Equals(IntPoint other) =>
        X == other.X && Y == other.Y;

    public override int GetHashCode() =>
        HashCode.Combine(X, Y);

    public static bool operator ==(IntPoint left, IntPoint right) => left.Equals(right);
    public static bool operator !=(IntPoint left, IntPoint right) => !left.Equals(right);

    public override string ToString() => $"({X}, {Y})";
    
    public IntPoint WithDelta(int dX, int dY) => new IntPoint(X + dX, Y + dY);

    public double DistanceSquared(IntPoint pt)
    {
        var dx = pt.X - X;
        var dy = pt.Y - Y;
        return dx * dx + dy * dy;
    }

    public int ManhattanDistance(IntPoint pt) =>
        Math.Abs(X - pt.X) + Math.Abs(Y - pt.Y);
}