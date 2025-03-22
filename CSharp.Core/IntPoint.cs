// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
//  purpose.
// 
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
// 
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.
namespace CSharp.Core;

public readonly struct IntPoint : IEquatable<IntPoint>
{
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
}