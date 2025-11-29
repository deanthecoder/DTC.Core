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
using System.Collections.Generic;

namespace DTC.Core;

/// <summary>
/// Implements a circular buffer (ring buffer) data structure.
/// A circular buffer is a fixed-size buffer that wraps around when full,
/// overwriting the oldest data when new items are added.
/// </summary>
public class CircularBuffer<T> : IEnumerable<T>
{
    private readonly T[] m_buffer;
    private int m_head;
    private int m_tail;
    private int m_count;

    public CircularBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

        m_buffer = new T[capacity];
        m_head = 0;
        m_tail = 0;
        m_count = 0;
    }

    public int Count => m_count;
    public int Capacity => m_buffer.Length;

    public void Write(T item)
    {
        m_buffer[m_head] = item;
        m_head = (m_head + 1) % Capacity;

        if (m_count == Capacity)
            m_tail = (m_tail + 1) % Capacity;
        else
            m_count++;
    }

    public void Write(ReadOnlySpan<T> items)
    {
        foreach (var item in items)
            Write(item);
    }

    public T Read()
    {
        if (m_count == 0)
            throw new InvalidOperationException("Buffer is empty.");

        var item = m_buffer[m_tail];
        m_tail = (m_tail + 1) % Capacity;
        m_count--;

        return item;
    }

    public int Read(Span<T> destination)
    {
        var toRead = Math.Min(destination.Length, m_count);
        for (var i = 0; i < toRead; i++)
        {
            destination[i] = m_buffer[(m_tail + i) % Capacity];
        }

        m_tail = (m_tail + toRead) % Capacity;
        m_count -= toRead;

        return toRead;
    }

    public void Clear()
    {
        m_head = 0;
        m_tail = 0;
        m_count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < m_count; i++)
        {
            var index = (m_tail + i) % Capacity;
            yield return m_buffer[index];
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
