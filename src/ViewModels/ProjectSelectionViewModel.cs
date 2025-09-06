using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.ViewModels.Base;

namespace TouchGanttChart.ViewModels;

/// <summary>
/// ViewModel for the project selection dialog.
/// Handles project browsing and selection for opening existing projects.
/// </summary>
public partial class ProjectSelectionViewModel : ViewModelBase, IDisposable
{
    private readonly IDataService _dataService;
    private readonly ILogger<ProjectSelectionViewModel> _logger;

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Event fired when a dialog result is requested
    /// </summary>
    public event EventHandler<bool?>? DialogResultRequested;

    /// <summary>
    /// Collection of all available projects
    /// </summary>
    public ObservableCollection<Project> AllProjects { get; } = new();

    /// <summary>
    /// Collection of filtered projects based on current criteria
    /// </summary>
    public ObservableCollection<Project> Projects { get; } = new();

    /// <summary>
    /// Gets a value indicating whether there are projects available
    /// </summary>
    public bool HasProjects => !Projects.Any();

    /// <summary>
    /// Gets a value indicating whether a project can be opened
    /// </summary>
    public bool CanOpenSelected => SelectedProject != null;

    public ProjectSelectionViewModel(
        IDataService dataService,
        ILogger<ProjectSelectionViewModel> logger) : base(logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger;

        Title = "Select Project";
    }

    /// <summary>
    /// Initializes the view model by loading projects
    /// </summary>
    protected override async Task OnInitializeAsync()
    {
        await LoadProjectsAsync();
    }

    /// <summary>
    /// Command to open the selected project
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenSelected))]
    private void OpenSelected()
    {
        if (SelectedProject == null) return;

        DialogResultRequested?.Invoke(this, true);
    }

    /// <summary>
    /// Command to cancel the dialog
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        DialogResultRequested?.Invoke(this, false);
    }

    /// <summary>
    /// Command to create a new project
    /// </summary>
    [RelayCommand]
    private void NewProject()
    {
        // This will trigger the main window's new project command
        DialogResultRequested?.Invoke(this, null);
    }

    /// <summary>
    /// Loads all projects from the data service
    /// </summary>
    private async Task LoadProjectsAsync()
    {
        try
        {
            IsLoading = true;
            SetStatus("Loading projects...");

            var projects = await _dataService.GetProjectsAsync();
            
            AllProjects.Clear();
            foreach (var project in projects.Where(p => !p.IsArchived).OrderBy(p => p.Name))
            {
                AllProjects.Add(project);
            }

            ApplyFilters();
            
            SetStatus($"Loaded {Projects.Count} projects");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading projects");
            SetStatus("Failed to load projects");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Applies current filters to the project list
    /// </summary>
    private void ApplyFilters()
    {
        Projects.Clear();
        
        var filteredProjects = AllProjects.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filteredProjects = filteredProjects.Where(p => 
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.ProjectManager.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var project in filteredProjects)
        {
            Projects.Add(project);
        }

        OnPropertyChanged(nameof(HasProjects));
    }

    /// <summary>
    /// Handles search text changes
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Handles selected project changes
    /// </summary>
    partial void OnSelectedProjectChanged(Project? value)
    {
        OpenSelectedCommand?.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public void Dispose()
    {
        // Nothing to dispose currently
        GC.SuppressFinalize(this);
    }
}