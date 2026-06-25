using Kvanto.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace Kvanto.Converters;

/// <summary>
/// Converts a CalendarDayViewModel to a background brush.
/// Today → subtle accent tint; other-month days → dimmer background.
/// </summary>
public class CalendarDayBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is CalendarDayViewModel day)
        {
            if (day.IsToday)
                return new SolidColorBrush(Color.FromArgb(40, 99, 102, 241)); // indigo tint

            if (!day.IsCurrentMonth)
                return new SolidColorBrush(Color.FromArgb(20, 128, 128, 128)); // very dim

            if (day.TotalMinutes > 0)
                return new SolidColorBrush(Color.FromArgb(15, 16, 185, 129)); // slight green tint

            return new SolidColorBrush(Colors.Transparent);
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

/// <summary>
/// Converts a hex color string (e.g. "#6366F1") to a <see cref="Windows.UI.Color"/>.
/// Returns <see cref="Colors.Transparent"/> for invalid input.
/// </summary>
public class HexToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                try
                {
                    byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                    byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                    byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                    return Color.FromArgb(255, r, g, b);
                }
                catch { }
            }
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

/// <summary>
/// Converts a fraction (0.0–1.0) to a pixel width, assuming the containing
/// element reports its actual width.  Used for the per-task analytics bar.
/// Falls back to a static width of 600px × fraction.
/// </summary>
public class FractionToWidthConverter : IValueConverter
{
    private const double MaxWidth = 600.0;

    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is double fraction ? fraction * MaxWidth : 0.0;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
