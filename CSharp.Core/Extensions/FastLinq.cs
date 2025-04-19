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

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable MemberCanBePrivate.Global
namespace CSharp.Core.Extensions;

public static class FastLinq
{
    public static int FastSum(this IList<int> items)
    {
        var sum = 0;
        for (var i = 0; i < items.Count; i++)
            sum += items[i];
        return sum;
    }

    public static float FastSum(this IList<float> items)
    {
        var sum = 0.0f;
        for (var i = 0; i < items.Count; i++)
            sum += items[i];
        return sum;
    }

    public static double FastSum(this IList<double> items)
    {
        var sum = 0.0;
        for (var i = 0; i < items.Count; i++)
            sum += items[i];
        return sum;
    }

    public static int FastSum<T>(this IList<T> items, Func<T, int> selector)
    {
        var sum = 0;
        for (var i = 0; i < items.Count; i++)
            sum += selector(items[i]);
        return sum;
    }

    public static float FastSum<T>(this IList<T> items, Func<T, float> selector)
    {
        var sum = 0f;
        for (var i = 0; i < items.Count; i++)
            sum += selector(items[i]);
        return sum;
    }

    public static double FastSum<T>(this IList<T> items, Func<T, double> selector)
    {
        var sum = 0.0;
        for (var i = 0; i < items.Count; i++)
            sum += selector(items[i]);
        return sum;
    }

    public static int FastMin(this IList<int> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");

        var min = items[0];
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v < min)
                min = v;
        }

        return min;
    }
    
    public static float FastMin(this IList<float> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");

        var min = items[0];
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v < min)
                min = v;
        }

        return min;
    }
    
    public static double FastMin(this IList<double> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");

        var min = items[0];
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v < min)
                min = v;
        }

        return min;
    }

    public static int FastMin<T>(this IList<T> items, Func<T, int> selector)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");

        var min = selector(items[0]);
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v < min)
                min = v;
        }

        return min;
    }

    public static float FastMin<T>(this IList<T> items, Func<T, float> selector)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");

        var min = selector(items[0]);
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v < min)
                min = v;
        }

        return min;
    }

    public static double FastMin<T>(this IList<T> items, Func<T, double> selector)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");

        var min = selector(items[0]);
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v < min)
                min = v;
        }

        return min;
    }

    public static int FastMax(this IList<int> items)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");

        var max = items[0];
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v > max)
                max = v;
        }

        return max;
    }
    
    public static float FastMax(this IList<float> items)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");

        var max = items[0];
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v > max)
                max = v;
        }

        return max;
    }
    
    public static double FastMax(this IList<double> items)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");

        var max = items[0];
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v > max)
                max = v;
        }

        return max;
    }
    
    public static int FastMax<T>(this IList<T> items, Func<T, int> selector)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");

        var max = selector(items[0]);
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v > max)
                max = v;
        }

        return max;
    }

    public static float FastMax<T>(this IList<T> items, Func<T, float> selector)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");

        var max = selector(items[0]);
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v > max)
                max = v;
        }

        return max;
    }

    public static double FastMax<T>(this IList<T> items, Func<T, double> selector)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");

        var max = selector(items[0]);
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v > max)
                max = v;
        }

        return max;
    }

    public static double FastAvg(this IList<int> items)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");
        return (double)items.FastSum() / items.Count;
    }

    public static float FastAvg(this IList<float> items)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");
        return items.FastSum() / items.Count;
    }

    public static double FastAvg(this IList<double> items)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");
        return items.FastSum() / items.Count;
    }

    public static double FastAvg<T>(this IList<T> items, Func<T, int> selector)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");
        return (double)items.FastSum(selector) / items.Count;
    }

    public static float FastAvg<T>(this IList<T> items, Func<T, float> selector)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Sequence contains no elements.");
        return items.FastSum(selector) / items.Count;
    }

    public static double FastAvg<T>(this IList<T> items, Func<T, double> selector)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        return items.FastSum(selector) / items.Count;
    }
    
    public static T FastFindMin<T>(this IList<T> items, Func<T, int> selector)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var min = selector(items[0]);
        var minIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v < min)
            {
                min = v;
                minIndex = i;
            }
        }
        
        return items[minIndex];
    }
    
    public static T FastFindMin<T>(this IList<T> items, Func<T, float> selector)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var min = selector(items[0]);
        var minIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v < min)
            {
                min = v;
                minIndex = i;
            }
        }
        
        return items[minIndex];
    }
    
    public static T FastFindMin<T>(this IList<T> items, Func<T, double> selector)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var min = selector(items[0]);
        var minIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v < min)
            {
                min = v;
                minIndex = i;
            }
        }
        
        return items[minIndex];
    }
    
    public static int FastFindMin(this IList<int> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var min = items[0];
        var minIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v < min)
            {
                min = v;
                minIndex = i;
            }
        }
        
        return items[minIndex];
    }
    
    public static float FastFindMin(this IList<float> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var min = items[0];
        var minIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v < min)
            {
                min = v;
                minIndex = i;
            }
        }
        
        return items[minIndex];
    }
    
    public static double FastFindMin(this IList<double> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var min = items[0];
        var minIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v < min)
            {
                min = v;
                minIndex = i;
            }
        }
        
        return items[minIndex];
    }
    
    public static T FastFindMax<T>(this IList<T> items, Func<T, int> selector)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var max = selector(items[0]);
        var maxIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v > max)
            {
                max = v;
                maxIndex = i;
            }
        }
        
        return items[maxIndex];
    }
    
    public static T FastFindMax<T>(this IList<T> items, Func<T, float> selector)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var max = selector(items[0]);
        var maxIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v > max)
            {
                max = v;
                maxIndex = i;
            }
        }
        
        return items[maxIndex];
    }
    
    public static T FastFindMax<T>(this IList<T> items, Func<T, double> selector)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var max = selector(items[0]);
        var maxIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = selector(items[i]);
            if (v > max)
            {
                max = v;
                maxIndex = i;
            }
        }
        
        return items[maxIndex];
    }
    
    public static int FastFindMax(this IList<int> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var max = items[0];
        var maxIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v > max)
            {
                max = v;
                maxIndex = i;
            }
        }
        
        return items[maxIndex];
    }
    
    public static float FastFindMax(this IList<float> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var max = items[0];
        var maxIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v > max)
            {
                max = v;
                maxIndex = i;
            }
        }
        
        return items[maxIndex];
    }
    
    public static double FastFindMax(this IList<double> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements.");
        
        var max = items[0];
        var maxIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var v = items[i];
            if (v > max)
            {
                max = v;
                maxIndex = i;
            }
        }
        
        return items[maxIndex];
    }
}
