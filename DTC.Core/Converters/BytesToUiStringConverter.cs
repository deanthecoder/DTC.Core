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
using System.Globalization;
using Avalonia.Data.Converters;
using DTC.Core.Extensions;

namespace DTC.Core.Converters;

/// <summary>
/// Converts a byte count to a human-readable bytes/Kb/Mb/etc string.
/// </summary>
public class BytesToUiStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var byteCount = value as long? ?? value as int? ?? 0L;
        return byteCount.ToSize();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}