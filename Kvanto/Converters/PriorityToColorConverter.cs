using Kvanto.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace Kvanto.Converters;

/// <summary>Maps TaskPriority to a SolidColorBrush for the priority indicator bar.</summary>
public class PriorityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var color = value is TaskPriority priority
            ? priority switch
            {
                TaskPriority.Low => Color.FromArgb(255, 148, 163, 184),   // slate-400
                TaskPriority.Medium => Color.FromArgb(255, 59, 130, 246), // blue-500
                TaskPriority.High => Color.FromArgb(255, 249, 115, 22),   // orange-500
                TaskPriority.Critical => Color.FromArgb(255, 239, 68, 68), // red-500
                _ => Color.FromArgb(255, 148, 163, 184)
            }
            : Color.FromArgb(255, 148, 163, 184);

        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
