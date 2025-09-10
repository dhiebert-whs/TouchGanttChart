using System.Globalization;
using System.Windows.Data;
using TouchGanttChart.Models;

namespace TouchGanttChart.Converters;

/// <summary>
/// Converter that returns the display name for a MainViewMode enum value.
/// </summary>
public class MainViewModeDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MainViewMode mode)
        {
            return MainViewConfig.GetDisplayName(mode);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}