using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TouchGanttChart.Models;
using TouchGanttChart.ViewModels;

namespace TouchGanttChart.Views;

/// <summary>
/// Interaction logic for DayView.xaml
/// Day view showing tasks scheduled for a specific date with navigation controls
/// </summary>
public partial class DayView : UserControl
{
    public DayView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles task click events to show task details or allow editing
    /// </summary>
    private void OnTaskClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && 
            element.Tag is GanttTask task && 
            DataContext is DayViewModel viewModel)
        {
            // Notify the view model of task selection
            if (viewModel.TaskSelectedCommand?.CanExecute(task) == true)
            {
                viewModel.TaskSelectedCommand.Execute(task);
            }
        }
    }
}

/// <summary>
/// Converter to show/hide elements based on string content
/// </summary>
public class StringToVisibilityConverter : System.Windows.Data.IValueConverter
{
    public static readonly StringToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Boolean to Visibility converter with inverse option
/// </summary>
public class BoolToVisibilityConverter : System.Windows.Data.IValueConverter
{
    public static readonly BoolToVisibilityConverter Instance = new();
    public static readonly BoolToVisibilityConverter InverseInstance = new() { Inverse = true };

    public bool Inverse { get; set; }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        if (Inverse) boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}