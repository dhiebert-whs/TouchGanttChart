using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using TouchGanttChart.Models;
using TouchGanttChart.ViewModels;

namespace TouchGanttChart.Views;

/// <summary>
/// Interaction logic for ProjectSelectionDialog.xaml
/// Touch-optimized dialog for selecting projects.
/// </summary>
public partial class ProjectSelectionDialog : Window
{
    /// <summary>
    /// Gets the selected project if the dialog result is true.
    /// </summary>
    public Project? SelectedProject { get; private set; }

    public ProjectSelectionDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor that accepts a view model for dependency injection
    /// </summary>
    /// <param name="viewModel">The ProjectSelectionViewModel instance</param>
    public ProjectSelectionDialog(ProjectSelectionViewModel viewModel) : this()
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

        // Initialize the view model
        _ = viewModel?.InitializeAsync();
    }

    /// <summary>
    /// Handles dialog result requests from the view model
    /// </summary>
    /// <param name="sender">The view model</param>
    /// <param name="result">The dialog result</param>
    private void OnDialogResultRequested(object? sender, bool? result)
    {
        if (result == true && DataContext is ProjectSelectionViewModel viewModel)
        {
            SelectedProject = viewModel.SelectedProject;
        }
        
        DialogResult = result;
        Close();
    }

    /// <summary>
    /// Handles clicks on project cards
    /// </summary>
    private void OnProjectCardClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && 
            element.Tag is Project project && 
            DataContext is ProjectSelectionViewModel viewModel)
        {
            viewModel.SelectedProject = project;
        }
    }

    /// <summary>
    /// Override to handle closing events
    /// </summary>
    /// <param name="e">Cancel event args</param>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        if (DataContext is ProjectSelectionViewModel viewModel)
        {
            viewModel.DialogResultRequested -= OnDialogResultRequested;
        }
        
        base.OnClosing(e);
    }

    /// <summary>
    /// Factory method to create dialog with proper dependency injection
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="owner">The owner window</param>
    /// <returns>Configured dialog instance</returns>
    public static ProjectSelectionDialog Create(
        IServiceProvider serviceProvider, 
        Window? owner = null)
    {
        var viewModel = serviceProvider.GetRequiredService<ProjectSelectionViewModel>();
        
        var dialog = new ProjectSelectionDialog(viewModel)
        {
            Owner = owner
        };
        
        return dialog;
    }
}