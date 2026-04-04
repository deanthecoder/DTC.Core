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
/// A least-recently-used cache with a fixed total cost budget.
/// </summary>
/// <remarks>
/// Keys are string-based for simplicity, while values remain generic.
/// This is useful when the number of entries is less important than the total size they consume.
/// </remarks>
/// <typeparam name="TValue">The type of cache values.</typeparam>
public sealed class SizedCache<TValue>
{
    private readonly long m_capacityCost;
    private readonly Dictionary<string, LinkedListNode<CacheItem>> m_cacheMap;
    private readonly LinkedList<CacheItem> m_lruList;
    private long m_usedCost;

    /// <summary>
    /// Creates a new cache with the specified total cost budget.
    /// </summary>
    public SizedCache(long capacityCost)
    {
        if (capacityCost <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacityCost));

        m_capacityCost = capacityCost;
        m_cacheMap = new Dictionary<string, LinkedListNode<CacheItem>>(StringComparer.Ordinal);
        m_lruList = new LinkedList<CacheItem>();
    }

    /// <summary>
    /// Gets the maximum total cost the cache will retain.
    /// </summary>
    public long CapacityCost => m_capacityCost;

    /// <summary>
    /// Gets the current total cost of all retained entries.
    /// </summary>
    public long UsedCost => m_usedCost;

    /// <summary>
    /// Gets the number of entries in the cache.
    /// </summary>
    public int Count => m_cacheMap.Count;

    /// <summary>
    /// Gets a cached value if it exists and marks it as recently used.
    /// </summary>
    public bool TryGetValue(string key, out TValue value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (!m_cacheMap.TryGetValue(key, out var node))
        {
            value = default;
            return false;
        }

        m_lruList.Remove(node);
        m_lruList.AddFirst(node);
        value = node.Value.Value;
        return true;
    }

    /// <summary>
    /// Adds or updates an entry and returns anything that had to be evicted.
    /// </summary>
    public IReadOnlyList<CacheItem> Set(string key, TValue value, long cost)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (cost < 0)
            throw new ArgumentOutOfRangeException(nameof(cost), "Cost must be zero or greater.");

        if (m_cacheMap.TryGetValue(key, out var existingNode))
        {
            m_lruList.Remove(existingNode);
            m_cacheMap.Remove(key);
            m_usedCost -= existingNode.Value.Cost;
        }

        var cacheItem = new CacheItem(key, value, cost);
        if (cost > m_capacityCost)
            return [cacheItem];

        var newNode = new LinkedListNode<CacheItem>(cacheItem);
        m_lruList.AddFirst(newNode);
        m_cacheMap[key] = newNode;
        m_usedCost += cost;

        var evictedItems = new List<CacheItem>();
        while (m_usedCost > m_capacityCost)
            evictedItems.Add(RemoveLast());

        return evictedItems;
    }

    /// <summary>
    /// Removes an entry if it exists.
    /// </summary>
    public bool Remove(string key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (!m_cacheMap.TryGetValue(key, out var node))
            return false;

        m_lruList.Remove(node);
        m_cacheMap.Remove(key);
        m_usedCost -= node.Value.Cost;
        return true;
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void Clear()
    {
        m_cacheMap.Clear();
        m_lruList.Clear();
        m_usedCost = 0;
    }

    private CacheItem RemoveLast()
    {
        var lastNode = m_lruList.Last;
        if (lastNode == null)
            throw new InvalidOperationException("The cache is empty.");

        m_lruList.RemoveLast();
        m_cacheMap.Remove(lastNode.Value.Key);
        m_usedCost -= lastNode.Value.Cost;
        return lastNode.Value;
    }

    /// <summary>
    /// Represents an entry in the cache.
    /// </summary>
    public sealed record CacheItem(string Key, TValue Value, long Cost);
}
