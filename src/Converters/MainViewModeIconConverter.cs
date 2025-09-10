using System.Globalization;
using System.Windows.Data;
using TouchGanttChart.Models;

namespace TouchGanttChart.Converters;

/// <summary>
/// Converter that returns the icon character for a MainViewMode enum value.
/// </summary>
public class MainViewModeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MainViewMode mode)
        {
            return MainViewConfig.GetIcon(mode);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}