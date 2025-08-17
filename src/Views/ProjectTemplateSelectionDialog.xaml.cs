using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.ViewModels;

namespace TouchGanttChart.Views;

/// <summary>
/// Interaction logic for ProjectTemplateSelectionDialog.xaml
/// Touch-optimized dialog for selecting and creating projects from templates.
/// </summary>
public partial class ProjectTemplateSelectionDialog : Window
{
    /// <summary>
    /// Gets the created project if the dialog result is true.
    /// </summary>
    public Project? CreatedProject { get; private set; }

    public ProjectTemplateSelectionDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor that accepts a view model for dependency injection
    /// </summary>
    /// <param name="viewModel">The ProjectTemplateSelectionViewModel instance</param>
    public ProjectTemplateSelectionDialog(ProjectTemplateSelectionViewModel viewModel) : this()
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
    private async void OnDialogResultRequested(object? sender, bool? result)
    {
        if (result == true && DataContext is ProjectTemplateSelectionViewModel viewModel)
        {
            try
            {
                // Create the project from the selected template
                var templateService = App.Current.GetService<IProjectTemplateService>();
                
                CreatedProject = await templateService.CreateProjectFromTemplateAsync(
                    viewModel.SelectedTemplate!.Id,
                    viewModel.ProjectName,
                    viewModel.ProjectManager,
                    viewModel.StartDate);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating project: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        else
        {
            DialogResult = result;
        }
        
        Close();
    }

    /// <summary>
    /// Handles clicks on template cards
    /// </summary>
    private void OnTemplateCardClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && 
            element.Tag is ProjectTemplate template && 
            DataContext is ProjectTemplateSelectionViewModel viewModel)
        {
            viewModel.SelectedTemplate = template;
        }
    }

    /// <summary>
    /// Override to handle closing events
    /// </summary>
    /// <param name="e">Cancel event args</param>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        if (DataContext is ProjectTemplateSelectionViewModel viewModel)
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
    public static ProjectTemplateSelectionDialog Create(
        IServiceProvider serviceProvider, 
        Window? owner = null)
    {
        var viewModel = serviceProvider.GetRequiredService<ProjectTemplateSelectionViewModel>();
        
        var dialog = new ProjectTemplateSelectionDialog(viewModel)
        {
            Owner = owner
        };
        
        return dialog;
    }
}