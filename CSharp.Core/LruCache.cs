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

namespace CSharp.Core;

/// <summary>
/// A caching class that has an upper limit on the number of entries it can contain.
/// </summary>
/// <remarks>
/// An LRU cache is a fixed-size cache that removes the least recently used items first when the cache reaches its capacity. 
/// It is useful for managing a limited amount of resources by ensuring that the most frequently accessed items remain in the cache.
/// </remarks>
/// <typeparam name="TKey">The type of the keys in the cache.</typeparam>
/// <typeparam name="TValue">The type of the values in the cache.</typeparam>
public class LruCache<TKey, TValue>
{
    private readonly int m_capacity;
    private readonly Dictionary<TKey, LinkedListNode<CacheItem>> m_cacheMap;
    private readonly LinkedList<CacheItem> m_lruList;

    /// <summary>
    /// Initializes a new instance of the <see cref="LruCache{TKey, TValue}"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of items that the cache can hold.</param>
    /// <exception cref="ArgumentException">Thrown when the capacity is less than or equal to zero.</exception>
    public LruCache(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero", nameof(capacity));
        m_capacity = capacity;
        m_cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
        m_lruList = new LinkedList<CacheItem>();
    }

    /// <summary>
    /// Gets the number of items currently stored in the cache.
    /// </summary>
    public int Count => m_cacheMap.Count;

    /// <summary>
    /// Gets the value associated with the specified key, or adds a new value if the key does not exist.
    /// </summary>
    /// <param name="key">The key of the item to get or add.</param>
    /// <param name="createItem">A function to create a new value if the key does not exist.</param>
    /// <returns>The value associated with the specified key.</returns>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> createItem)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (createItem == null)
            throw new ArgumentNullException(nameof(createItem));
        
        if (m_cacheMap.TryGetValue(key, out var node))
        {
            // Cache hit - Return the cached item.
            m_lruList.Remove(node);
            m_lruList.AddFirst(node);
            return node.Value.Value;
        }

        // Trim max cache size.
        if (m_cacheMap.Count >= m_capacity)
        {
#if DEBUG
            Console.WriteLine($"Cache full: {typeof(TKey)}, {typeof(TValue)}");
#endif
            while (m_cacheMap.Count >= m_capacity * 3 / 4)
                RemoveLast();
        }
        
        // Add the new cache item.
        var value = createItem(key);
        var cacheItem = new CacheItem(key, value);
        var newNode = new LinkedListNode<CacheItem>(cacheItem);
        m_lruList.AddFirst(newNode);
        m_cacheMap[key] = newNode;

        return value;
    }

    /// <summary>
    /// Removes the last item from the LRU (Least Recently Used) list and the cache map.
    /// </summary>
    /// <remarks>
    /// This method is called when the cache reaches its capacity and a new item needs to be added.
    /// It removes the least recently used item to make space for the new item.
    /// </remarks>
    private void RemoveLast()
    {
        var lastNode = m_lruList.Last;
        if (lastNode == null)
            return;
        m_lruList.RemoveLast();
        m_cacheMap.Remove(lastNode.Value.Key);
    }

    /// <summary>
    /// Represents an item in the cache.
    /// </summary>
    /// <param name="Key">The key of the cache item.</param>
    /// <param name="Value">The value of the cache item.</param>
    private record CacheItem(TKey Key, TValue Value);
}