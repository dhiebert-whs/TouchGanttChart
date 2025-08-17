using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Windows;
using TouchGanttChart.Controls;
using TouchGanttChart.ViewModels;

namespace TouchGanttChart;

/// <summary>
/// Interaction logic for MainWindow.xaml with dependency injection support.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ILogger<MainWindow> _logger;

    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    /// <param name="viewModel">The main window view model.</param>
    /// <param name="logger">The logger instance.</param>
    public MainWindow(MainWindowViewModel viewModel, ILogger<MainWindow> logger)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeComponent();
        
        DataContext = _viewModel;
        
        _logger.LogInformation("MainWindow initialized");
    }

    /// <summary>
    /// Handles the window loaded event.
    /// </summary>
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _logger.LogInformation("MainWindow loading - initializing view model");
            await _viewModel.InitializeAsync();
            _logger.LogInformation("MainWindow loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading MainWindow");
            MessageBox.Show($"Error loading application: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the window closing event.
    /// </summary>
    private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _logger.LogInformation("MainWindow closing - cleaning up view model");
            await _viewModel.CleanupAsync();
            _logger.LogInformation("MainWindow cleanup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MainWindow cleanup");
        }
    }

    /// <summary>
    /// Handles task selection events from the task list panel
    /// </summary>
    private void OnTaskSelected(object sender, TaskSelectedEventArgs e)
    {
        _viewModel.SelectedTask = e.Task;
        _logger.LogDebug("Task selected: {TaskName}", e.Task.Name);
    }

    /// <summary>
    /// Handles task edit requests from the task list panel
    /// </summary>
    private async void OnTaskEditRequested(object sender, TaskSelectedEventArgs e)
    {
        _logger.LogDebug("Task edit requested: {TaskName}", e.Task.Name);
        _viewModel.SelectedTask = e.Task;
        
        // Execute the edit task command
        if (_viewModel.EditTaskCommand?.CanExecute(null) == true)
        {
            if (_viewModel.EditTaskCommand is IAsyncRelayCommand asyncCommand)
            {
                await asyncCommand.ExecuteAsync(null);
            }
            else
            {
                _viewModel.EditTaskCommand.Execute(null);
            }
        }
    }
}