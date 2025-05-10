using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DTC.Core.Converters;

/// <summary>
/// Converts a 'risk' factor [0.0, 1.0) to a green/orange/red warning brush.
/// </summary>
public class RiskToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (value as double? ?? 0.0) switch
        {
            < 0.6 => Brushes.LimeGreen,
            < 0.8 => Brushes.Orange,
            _ => Brushes.Red
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}