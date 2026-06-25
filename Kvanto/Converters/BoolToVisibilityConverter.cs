using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Kvanto.Converters;

/// <summary>Converts a bool (or any truthy value) to Visibility.</summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isVisible = value switch
        {
            bool b => b,
            int i => i > 0,
            string s => !string.IsNullOrEmpty(s),
            _ => value != null
        };
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is Visibility v && v == Visibility.Visible;
}

/// <summary>Inverse of BoolToVisibilityConverter.</summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isVisible = value switch
        {
            bool b => b,
            int i => i > 0,
            _ => value != null
        };
        return isVisible ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is Visibility v && v == Visibility.Collapsed;
}
