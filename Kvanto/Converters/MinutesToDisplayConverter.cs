using Microsoft.UI.Xaml.Data;
using System;

namespace Kvanto.Converters;

/// <summary>Converts total minutes to a human-readable string like "1h 25m".</summary>
public class MinutesToDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not int minutes) return "0m";
        return minutes >= 60
            ? $"{minutes / 60}h {minutes % 60}m"
            : $"{minutes}m";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
