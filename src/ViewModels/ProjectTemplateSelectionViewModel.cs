using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.ViewModels.Base;

namespace TouchGanttChart.ViewModels;

/// <summary>
/// ViewModel for the project template selection dialog.
/// Handles template browsing, categorization, and selection.
/// </summary>
public partial class ProjectTemplateSelectionViewModel : ViewModelBase, IDisposable
{
    private readonly IProjectTemplateService _templateService;
    private readonly ILogger<ProjectTemplateSelectionViewModel> _logger;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _projectManager = string.Empty;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private ProjectTemplate? _selectedTemplate;

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showPopularOnly;

    /// <summary>
    /// Event fired when a dialog result is requested
    /// </summary>
    public event EventHandler<bool?>? DialogResultRequested;

    /// <summary>
    /// Collection of all available templates
    /// </summary>
    public ObservableCollection<ProjectTemplate> AllTemplates { get; } = new();

    /// <summary>
    /// Collection of filtered templates based on current criteria
    /// </summary>
    public ObservableCollection<ProjectTemplate> FilteredTemplates { get; } = new();

    /// <summary>
    /// Collection of available categories
    /// </summary>
    public ObservableCollection<string> Categories { get; } = new();

    /// <summary>
    /// Collection of popular templates
    /// </summary>
    public ObservableCollection<ProjectTemplate> PopularTemplates { get; } = new();

    public ProjectTemplateSelectionViewModel(
        IProjectTemplateService templateService,
        ILogger<ProjectTemplateSelectionViewModel> logger) : base(logger)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _logger = logger;

        Title = "Create Project from Template";
    }

    /// <summary>
    /// Initializes the view model by loading templates and categories
    /// </summary>
    protected override async Task OnInitializeAsync()
    {
        await LoadTemplatesAsync();
        await LoadCategoriesAsync();
        await LoadPopularTemplatesAsync();
    }

    /// <summary>
    /// Command to create a project from the selected template
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCreateProject))]
    private async Task CreateProjectAsync()
    {
        if (SelectedTemplate == null || string.IsNullOrWhiteSpace(ProjectName) || string.IsNullOrWhiteSpace(ProjectManager))
            return;

        try
        {
            IsLoading = true;
            SetStatus("Creating project from template...");

            // Validate inputs
            if (StartDate < DateTime.Today.AddDays(-30))
            {
                SetStatus("Start date cannot be more than 30 days in the past");
                return;
            }

            _logger.LogInformation("Creating project '{ProjectName}' from template '{TemplateName}'", 
                ProjectName, SelectedTemplate.Name);

            // Create the project (this will be handled by the dialog's parent)
            DialogResultRequested?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project from template");
            SetStatus($"Error creating project: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to cancel template selection
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInformation("Template selection cancelled");
        DialogResultRequested?.Invoke(this, false);
    }

    /// <summary>
    /// Command to refresh templates
    /// </summary>
    [RelayCommand]
    private async Task RefreshTemplatesAsync()
    {
        await LoadTemplatesAsync();
        ApplyFilters();
        SetStatus("Templates refreshed");
    }

    /// <summary>
    /// Command to clear search and filters
    /// </summary>
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedCategory = "All";
        ShowPopularOnly = false;
        ApplyFilters();
        SetStatus("Filters cleared");
    }

    /// <summary>
    /// Loads all available templates
    /// </summary>
    private async Task LoadTemplatesAsync()
    {
        try
        {
            var templates = await _templateService.GetTemplatesAsync();
            
            AllTemplates.Clear();
            foreach (var template in templates)
            {
                AllTemplates.Add(template);
            }

            ApplyFilters();
            _logger.LogInformation("Loaded {TemplateCount} project templates", AllTemplates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading templates");
            SetStatus("Failed to load templates");
        }
    }

    /// <summary>
    /// Loads all available categories
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _templateService.GetCategoriesAsync();
            
            Categories.Clear();
            Categories.Add("All");
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            _logger.LogInformation("Loaded {CategoryCount} template categories", Categories.Count - 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories");
        }
    }

    /// <summary>
    /// Loads popular templates
    /// </summary>
    private async Task LoadPopularTemplatesAsync()
    {
        try
        {
            var popular = await _templateService.GetPopularTemplatesAsync(5);
            
            PopularTemplates.Clear();
            foreach (var template in popular)
            {
                PopularTemplates.Add(template);
            }

            _logger.LogInformation("Loaded {PopularCount} popular templates", PopularTemplates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading popular templates");
        }
    }

    /// <summary>
    /// Applies current filters to the template list
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = AllTemplates.AsEnumerable();

        // Category filter
        if (SelectedCategory != "All")
        {
            filtered = filtered.Where(t => t.Category == SelectedCategory);
        }

        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(t => 
                t.Name.ToLowerInvariant().Contains(searchLower) ||
                t.Description.ToLowerInvariant().Contains(searchLower) ||
                t.Category.ToLowerInvariant().Contains(searchLower));
        }

        // Popular filter
        if (ShowPopularOnly)
        {
            var popularIds = PopularTemplates.Select(p => p.Id).ToHashSet();
            filtered = filtered.Where(t => popularIds.Contains(t.Id));
        }

        FilteredTemplates.Clear();
        foreach (var template in filtered.OrderBy(t => t.Category).ThenBy(t => t.Name))
        {
            FilteredTemplates.Add(template);
        }

        OnPropertyChanged(nameof(HasNoTemplates));
        OnPropertyChanged(nameof(FilteredTemplateCount));
    }

    /// <summary>
    /// Updates filters when search text changes
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Updates filters when category selection changes
    /// </summary>
    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Updates filters when popular filter changes
    /// </summary>
    partial void OnShowPopularOnlyChanged(bool value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Updates command states when template selection changes
    /// </summary>
    partial void OnSelectedTemplateChanged(ProjectTemplate? value)
    {
        CreateProjectCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(HasSelectedTemplate));
        OnPropertyChanged(nameof(SelectedTemplateInfo));
    }

    /// <summary>
    /// Updates command states when project name changes
    /// </summary>
    partial void OnProjectNameChanged(string value)
    {
        CreateProjectCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Updates command states when project manager changes
    /// </summary>
    partial void OnProjectManagerChanged(string value)
    {
        CreateProjectCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Gets whether the create project command can execute
    /// </summary>
    private bool CanCreateProject()
    {
        return SelectedTemplate != null && 
               !string.IsNullOrWhiteSpace(ProjectName) && 
               !string.IsNullOrWhiteSpace(ProjectManager);
    }

    /// <summary>
    /// Gets whether there are no templates to display
    /// </summary>
    public bool HasNoTemplates => FilteredTemplates.Count == 0;

    /// <summary>
    /// Gets whether a template is selected
    /// </summary>
    public bool HasSelectedTemplate => SelectedTemplate != null;

    /// <summary>
    /// Gets the number of filtered templates
    /// </summary>
    public int FilteredTemplateCount => FilteredTemplates.Count;

    /// <summary>
    /// Gets information about the selected template
    /// </summary>
    public string SelectedTemplateInfo
    {
        get
        {
            if (SelectedTemplate == null) return string.Empty;
            
            return $"{SelectedTemplate.TaskCount} tasks • {SelectedTemplate.DurationDisplay} • {SelectedTemplate.BudgetDisplay}";
        }
    }

    /// <summary>
    /// Gets the estimated end date based on start date and selected template
    /// </summary>
    public DateTime EstimatedEndDate
    {
        get
        {
            if (SelectedTemplate == null) return StartDate;
            return StartDate.AddDays(SelectedTemplate.EstimatedDurationDays);
        }
    }

    /// <summary>
    /// Updates estimated end date when start date changes
    /// </summary>
    partial void OnStartDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(EstimatedEndDate));
    }

    /// <summary>
    /// Cleanup when the view model is disposed
    /// </summary>
    public void Dispose()
    {
        // No cleanup needed for this implementation
    }
}