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
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using DTC.Core.Extensions;

namespace DTC.Core.Converters;

public class FileInfoToLeafNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (value as FileInfo)?.LeafName();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}