using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TouchGanttChart.ViewModels;

namespace TouchGanttChart.Views;

/// <summary>
/// Interaction logic for TaskEditDialog.xaml
/// Touch-optimized dialog for creating and editing Gantt chart tasks.
/// </summary>
public partial class TaskEditDialog : Window
{
    public TaskEditDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor that accepts a view model for dependency injection
    /// </summary>
    /// <param name="viewModel">The TaskEditDialogViewModel instance</param>
    public TaskEditDialog(TaskEditDialogViewModel viewModel) : this()
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
    /// Override to handle closing events
    /// </summary>
    /// <param name="e">Cancel event args</param>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        if (DataContext is TaskEditDialogViewModel viewModel)
        {
            viewModel.DialogResultRequested -= OnDialogResultRequested;
        }
        
        base.OnClosing(e);
    }

    /// <summary>
    /// Factory method to create dialog with proper dependency injection
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="task">The task to edit (null for new task)</param>
    /// <param name="projectId">The project ID for new tasks</param>
    /// <param name="owner">The owner window</param>
    /// <returns>Configured dialog instance</returns>
    public static TaskEditDialog Create(
        IServiceProvider serviceProvider, 
        Models.GanttTask? task = null, 
        int? projectId = null,
        Window? owner = null)
    {
        var viewModel = serviceProvider.GetRequiredService<TaskEditDialogViewModel>();
        
        if (task != null)
        {
            viewModel.SetTask(task);
        }
        else if (projectId.HasValue)
        {
            viewModel.SetNewTask(projectId.Value);
        }
        
        var dialog = new TaskEditDialog(viewModel)
        {
            Owner = owner
        };
        
        return dialog;
    }
}