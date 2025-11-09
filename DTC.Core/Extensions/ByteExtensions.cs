// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System.Diagnostics;
using System.Text;
using K4os.Compression.LZ4;

namespace DTC.Core.Extensions;

public static class ByteExtensions
{
    public static byte[] Compress(this byte[] input) =>
        LZ4Pickler.Pickle(input);

    public static byte[] Decompress(this byte[] input) =>
        LZ4Pickler.Unpickle(input);

    public static string DecompressToString(this byte[] input) =>
        Encoding.UTF8.GetString(Decompress(input));

    public static bool IsBitSet(this byte b, byte i)
    {
        Debug.Assert(i <= 7, "Index out of range.");
        return (b & (1 << i)) != 0;
    }

    public static byte ResetBit(this byte b, byte i)
    {
        Debug.Assert(i <= 7, "Index out of range.");
        var mask = (byte)~(1 << i);
        return (byte)(b & mask);
    }

    public static byte SetBit(this byte b, byte i)
    {
        Debug.Assert(i <= 7, "Index out of range.");
        return (byte)(b | (1 << i));
    }
    
    /// <summary>
    /// Reverses the order of bits in a byte.
    /// </summary>
    /// <returns>A new byte with reversed bit order.</returns>
    /// <example>
    /// <code>
    /// byte value = 0b10110001;  // 177 decimal
    /// byte mirrored = value.Mirror();  // Returns 0b10001101 (141 decimal)
    /// </code>
    /// </example>
    public static byte Mirror(this byte bits)
    {
        bits = (byte)((bits & 0xF0) >> 4 | (bits & 0x0F) << 4);
        bits = (byte)((bits & 0xCC) >> 2 | (bits & 0x33) << 2);
        bits = (byte)((bits & 0xAA) >> 1 | (bits & 0x55) << 1);
        return bits;
    }
}