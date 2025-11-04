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
using System;
using System.Collections.Generic;

namespace DTC.Core.Extensions;

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