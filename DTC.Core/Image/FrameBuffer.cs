// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System;

namespace DTC.Core.Image;

/// <summary>
/// Represents a tightly packed linear framebuffer (width × height × bytes-per-pixel).
/// </summary>
public sealed class FrameBuffer
{
    /// <summary>
    /// Creates a framebuffer for the provided geometry.
    /// </summary>
    public FrameBuffer(int width, int height, int bytesPerPixel)
    {
        Resize(width, height, bytesPerPixel);
    }

    /// <summary>
    /// Gets the width in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the number of bytes per pixel.
    /// </summary>
    public int BytesPerPixel { get; private set; }

    /// <summary>
    /// Gets the backing linear byte buffer.
    /// </summary>
    public byte[] Data { get; private set; } = [];

    /// <summary>
    /// Gets the required byte length for the current geometry.
    /// </summary>
    public int ByteLength => Width * Height * BytesPerPixel;

    /// <summary>
    /// Resizes the framebuffer geometry and reallocates storage when needed.
    /// Existing pixel contents are discarded.
    /// </summary>
    public void Resize(int width, int height, int bytesPerPixel)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        if (bytesPerPixel <= 0)
            throw new ArgumentOutOfRangeException(nameof(bytesPerPixel));

        if (Width == width && Height == height && BytesPerPixel == bytesPerPixel && Data.Length == ByteLength)
            return;

        Width = width;
        Height = height;
        BytesPerPixel = bytesPerPixel;
        Data = new byte[ByteLength];
    }

    /// <summary>
    /// Copies source pixel bytes into this framebuffer.
    /// </summary>
    /// <remarks>
    /// When <paramref name="clearRemainderWhenShort"/> is true and source is short,
    /// the copied prefix is preserved and the remainder is cleared.
    /// </remarks>
    public void CopyFrom(ReadOnlySpan<byte> source, bool clearRemainderWhenShort = false)
    {
        if (source.Length < ByteLength && !clearRemainderWhenShort)
            throw new ArgumentException($"Source length {source.Length} is smaller than required {ByteLength}.", nameof(source));

        var copyLength = Math.Min(source.Length, ByteLength);
        source[..copyLength].CopyTo(Data.AsSpan(0, copyLength));
        if (copyLength < ByteLength)
            Data.AsSpan(copyLength).Clear();
    }

    /// <summary>
    /// Copies this framebuffer into <paramref name="destination"/>, resizing it first when required.
    /// </summary>
    public void CopyTo(FrameBuffer destination)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        destination.Resize(Width, Height, BytesPerPixel);
        Data.AsSpan().CopyTo(destination.Data);
    }

    /// <summary>
    /// Clears all color channels to black. For RGBA buffers, alpha is set to 255.
    /// </summary>
    public void FillBlack()
    {
        Data.AsSpan().Clear();
        if (BytesPerPixel != 4)
            return;

        for (var i = 3; i < Data.Length; i += BytesPerPixel)
            Data[i] = 255;
    }

    /// <summary>
    /// Blends this framebuffer (previous frame) with current-frame bytes in place.
    /// </summary>
    public void BlendWithPrevious(ReadOnlySpan<byte> currentFrame, int previousWeight, int currentWeight)
    {
        if (previousWeight < 0)
            throw new ArgumentOutOfRangeException(nameof(previousWeight));
        if (currentWeight < 0)
            throw new ArgumentOutOfRangeException(nameof(currentWeight));
        if (previousWeight == 0 && currentWeight == 0)
            throw new ArgumentOutOfRangeException(nameof(currentWeight), "At least one blend weight must be non-zero.");
        if (currentFrame.Length < ByteLength)
            throw new ArgumentException($"Source length {currentFrame.Length} is smaller than required {ByteLength}.", nameof(currentFrame));

        var totalWeight = previousWeight + currentWeight;
        var span = Data.AsSpan();
        for (var i = 0; i < span.Length; i++)
            span[i] = (byte)((span[i] * previousWeight + currentFrame[i] * currentWeight) / totalWeight);
    }
}
