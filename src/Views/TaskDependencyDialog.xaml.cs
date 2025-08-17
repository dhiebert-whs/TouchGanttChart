using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using TouchGanttChart.Models;
using TouchGanttChart.ViewModels;

namespace TouchGanttChart.Views;

/// <summary>
/// Interaction logic for TaskDependencyDialog.xaml
/// Touch-optimized dialog for managing task dependencies.
/// </summary>
public partial class TaskDependencyDialog : Window
{
    public TaskDependencyDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor that accepts a view model for dependency injection
    /// </summary>
    /// <param name="viewModel">The TaskDependencyDialogViewModel instance</param>
    public TaskDependencyDialog(TaskDependencyDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
        
        // Subscribe to dialog result events
        if (viewModel != null)
        {
            viewModel.DialogResultRequested += OnDialogResultRequested;
        }
        
        // Center on parent if available
        if (Owner != null)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    /// <summary>
    /// Handles dialog result requests from the view model
    /// </summary>
    /// <param name="sender">The view model</param>
    /// <param name="result">The dialog result</param>
    private void OnDialogResultRequested(object? sender, bool? result)
    {
        DialogResult = result;
        Close();
    }

    /// <summary>
    /// Handles clicks on available tasks
    /// </summary>
    private void OnAvailableTaskClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is GanttTask task && DataContext is TaskDependencyDialogViewModel viewModel)
        {
            viewModel.SelectedAvailableTask = task;
        }
    }

    /// <summary>
    /// Handles clicks on current dependencies
    /// </summary>
    private void OnCurrentDependencyClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is GanttTask task && DataContext is TaskDependencyDialogViewModel viewModel)
        {
            viewModel.SelectedCurrentDependency = task;
        }
    }

    /// <summary>
    /// Override to handle closing events
    /// </summary>
    /// <param name="e">Cancel event args</param>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        if (DataContext is TaskDependencyDialogViewModel viewModel)
        {
            viewModel.DialogResultRequested -= OnDialogResultRequested;
        }
        
        base.OnClosing(e);
    }

    /// <summary>
    /// Factory method to create dialog with proper dependency injection
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="task">The task to manage dependencies for</param>
    /// <param name="allTasks">All available tasks in the project</param>
    /// <param name="owner">The owner window</param>
    /// <returns>Configured dialog instance</returns>
    public static TaskDependencyDialog Create(
        IServiceProvider serviceProvider, 
        GanttTask task,
        IEnumerable<GanttTask> allTasks,
        Window? owner = null)
    {
        var viewModel = serviceProvider.GetRequiredService<TaskDependencyDialogViewModel>();
        viewModel.SetTask(task, allTasks);
        
        var dialog = new TaskDependencyDialog(viewModel)
        {
            Owner = owner
        };
        
        return dialog;
    }
}