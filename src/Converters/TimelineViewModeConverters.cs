using System.Globalization;
using System.Windows.Data;
using TouchGanttChart.Models;

namespace TouchGanttChart.Converters;

/// <summary>
/// Converts TimelineViewMode enum to display name.
/// </summary>
public class TimelineViewModeDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimelineViewMode mode)
        {
            return TimelineViewConfig.GetDisplayName(mode);
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts TimelineViewMode enum to icon.
/// </summary>
public class TimelineViewModeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimelineViewMode mode)
        {
            return TimelineViewConfig.GetIcon(mode);
        }
        return "📋";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}