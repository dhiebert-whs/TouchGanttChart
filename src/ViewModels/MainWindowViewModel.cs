using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.ViewModels.Base;
using TouchGanttChart.Views;

namespace TouchGanttChart.ViewModels;

/// <summary>
/// Main window view model handling the primary application interface.
/// Manages project navigation, task display, and touch interactions.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDataService _dataService;
    private readonly IPdfExportService _pdfExportService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindowViewModel> _logger;

    /// <summary>
    /// Initializes a new instance of the MainWindowViewModel class.
    /// </summary>
    /// <param name="dataService">The data service.</param>
    /// <param name="pdfExportService">The PDF export service.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="logger">The logger instance.</param>
    public MainWindowViewModel(
        IDataService dataService,
        IPdfExportService pdfExportService,
        IServiceProvider serviceProvider,
        ILogger<MainWindowViewModel> logger) : base(logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _pdfExportService = pdfExportService ?? throw new ArgumentNullException(nameof(pdfExportService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger;
        
        Title = "Touch Gantt Chart";
        Projects = new ObservableCollection<Project>();
        Tasks = new ObservableCollection<GanttTask>();
        SelectedTasks = new ObservableCollection<GanttTask>();
        
        InitializeCommands();
    }

    /// <summary>
    /// Gets the collection of projects.
    /// </summary>
    public ObservableCollection<Project> Projects { get; }

    /// <summary>
    /// Gets the collection of tasks for the current project.
    /// </summary>
    public ObservableCollection<GanttTask> Tasks { get; }

    /// <summary>
    /// Gets the collection of currently selected tasks.
    /// </summary>
    public ObservableCollection<GanttTask> SelectedTasks { get; }

    /// <summary>
    /// Gets or sets the currently selected project.
    /// </summary>
    [ObservableProperty]
    private Project? _selectedProject;

    /// <summary>
    /// Gets or sets the currently selected task.
    /// </summary>
    [ObservableProperty]
    private GanttTask? _selectedTask;

    /// <summary>
    /// Gets or sets the current timeline view mode.
    /// </summary>
    [ObservableProperty]
    private TimelineViewMode _viewMode = TimelineViewMode.Weekly;

    /// <summary>
    /// Gets or sets the zoom level for the timeline.
    /// </summary>
    [ObservableProperty]
    private double _zoomLevel = 1.0;

    /// <summary>
    /// Gets or sets the timeline start date.
    /// </summary>
    [ObservableProperty]
    private DateTime _timelineStart = DateTime.Today.AddMonths(-1);

    /// <summary>
    /// Gets or sets the timeline end date.
    /// </summary>
    [ObservableProperty]
    private DateTime _timelineEnd = DateTime.Today.AddMonths(2);

    /// <summary>
    /// Gets or sets a value indicating whether the task details panel is visible.
    /// </summary>
    [ObservableProperty]
    private bool _isTaskDetailsPanelVisible = true;

    /// <summary>
    /// Gets or sets a value indicating whether the project tree is expanded.
    /// </summary>
    [ObservableProperty]
    private bool _isProjectTreeExpanded = true;

    /// <summary>
    /// Gets or sets the search filter text.
    /// </summary>
    [ObservableProperty]
    private string _searchFilter = string.Empty;

    /// <summary>
    /// Gets or sets the status filter.
    /// </summary>
    [ObservableProperty]
    private Models.TaskStatus? _statusFilter;

    /// <summary>
    /// Gets or sets the priority filter.
    /// </summary>
    [ObservableProperty]
    private TaskPriority? _priorityFilter;

    /// <summary>
    /// Gets or sets the assignee filter.
    /// </summary>
    [ObservableProperty]
    private string _assigneeFilter = string.Empty;

    // Commands
    /// <summary>
    /// Gets the command to create a new project.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _newProjectCommand;

    /// <summary>
    /// Gets the command to open an existing project.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _openProjectCommand;

    /// <summary>
    /// Gets the command to save the current project.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _saveCommand;

    /// <summary>
    /// Gets the command to add a new task.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _addTaskCommand;

    /// <summary>
    /// Gets the command to edit the selected task.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _editTaskCommand;

    /// <summary>
    /// Gets the command to delete the selected task.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _deleteTaskCommand;

    /// <summary>
    /// Gets the command to export to PDF.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _exportPdfCommand;

    /// <summary>
    /// Gets the command to zoom in on the timeline.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _zoomInCommand;

    /// <summary>
    /// Gets the command to zoom out on the timeline.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _zoomOutCommand;

    /// <summary>
    /// Gets the command to reset the timeline zoom.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _zoomResetCommand;

    /// <summary>
    /// Gets the command to toggle the task details panel.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _toggleTaskDetailsPanelCommand;

    /// <summary>
    /// Gets the command to clear all filters.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _clearFiltersCommand;

    /// <summary>
    /// Gets the command to manage task dependencies.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand? _manageDependenciesCommand;

    /// <summary>
    /// Gets the command to change timeline view mode.
    /// </summary>
    [ObservableProperty]
    private IRelayCommand<TimelineViewMode>? _changeViewModeCommand;

    /// <summary>
    /// Initializes all relay commands.
    /// </summary>
    private void InitializeCommands()
    {
        NewProjectCommand = new AsyncRelayCommand(CreateNewProjectAsync);
        OpenProjectCommand = new AsyncRelayCommand(OpenProjectAsync);
        SaveCommand = new AsyncRelayCommand(SaveProjectAsync, () => CanSave);
        AddTaskCommand = new AsyncRelayCommand(AddTaskAsync, () => SelectedProject != null);
        EditTaskCommand = new AsyncRelayCommand(EditTaskAsync, () => SelectedTask != null);
        DeleteTaskCommand = new AsyncRelayCommand(DeleteTaskAsync, () => SelectedTask != null);
        ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync, () => SelectedProject != null);
        ZoomInCommand = new RelayCommand(ZoomIn);
        ZoomOutCommand = new RelayCommand(ZoomOut);
        ZoomResetCommand = new RelayCommand(ResetZoom);
        ToggleTaskDetailsPanelCommand = new RelayCommand(ToggleTaskDetailsPanel);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ManageDependenciesCommand = new AsyncRelayCommand(ManageDependenciesAsync, () => SelectedTask != null);
        ChangeViewModeCommand = new RelayCommand<TimelineViewMode>(ChangeViewMode);
    }

    /// <summary>
    /// Handles property changes and updates command states.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(SelectedProject):
                OnSelectedProjectChanged();
                break;
            case nameof(SelectedTask):
                OnSelectedTaskChanged();
                break;
            case nameof(SearchFilter):
            case nameof(StatusFilter):
            case nameof(PriorityFilter):
            case nameof(AssigneeFilter):
                ApplyFilters();
                break;
        }
        
        UpdateCommandStates();
    }

    /// <summary>
    /// Updates the state of all commands.
    /// </summary>
    private void UpdateCommandStates()
    {
        SaveCommand?.NotifyCanExecuteChanged();
        AddTaskCommand?.NotifyCanExecuteChanged();
        EditTaskCommand?.NotifyCanExecuteChanged();
        DeleteTaskCommand?.NotifyCanExecuteChanged();
        ExportPdfCommand?.NotifyCanExecuteChanged();
        ManageDependenciesCommand?.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Handles selected project changes.
    /// </summary>
    private async void OnSelectedProjectChanged()
    {
        if (SelectedProject != null)
        {
            await LoadTasksForProjectAsync(SelectedProject.Id);
            UpdateTimelineRange();
        }
        else
        {
            Tasks.Clear();
            OnPropertyChanged(nameof(HasNoTasks));
        }
    }

    /// <summary>
    /// Handles selected task changes.
    /// </summary>
    private void OnSelectedTaskChanged()
    {
        // Update task selection in the collection
        foreach (var task in Tasks)
        {
            task.IsSelected = task == SelectedTask;
        }
    }

    /// <summary>
    /// Initializes the view model asynchronously.
    /// </summary>
    protected override async Task OnInitializeAsync()
    {
        await LoadProjectsAsync();
        
        // Select the first project if available
        if (Projects.Count > 0)
        {
            SelectedProject = Projects.First();
        }
    }

    /// <summary>
    /// Loads all projects from the data service.
    /// </summary>
    private async Task LoadProjectsAsync()
    {
        var projects = await _dataService.GetProjectsAsync();
        
        Projects.Clear();
        foreach (var project in projects)
        {
            Projects.Add(project);
        }
        
        SetStatus($"Loaded {Projects.Count} projects");
    }

    /// <summary>
    /// Loads tasks for the specified project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    private async Task LoadTasksForProjectAsync(int projectId)
    {
        var tasks = await _dataService.GetTasksByProjectIdAsync(projectId);
        
        Tasks.Clear();
        foreach (var task in tasks)
        {
            Tasks.Add(task);
        }
        
        ApplyFilters();
        OnPropertyChanged(nameof(HasNoTasks));
        SetStatus($"Loaded {Tasks.Count} tasks");
    }

    /// <summary>
    /// Creates a new project using the template selection dialog.
    /// </summary>
    private async Task CreateNewProjectAsync()
    {
        try
        {
            var mainWindow = Application.Current.MainWindow;
            var dialog = ProjectTemplateSelectionDialog.Create(_serviceProvider, mainWindow);
            
            var result = dialog.ShowDialog();
            
            if (result == true && dialog.CreatedProject != null)
            {
                // Reload projects to include the new one
                await LoadProjectsAsync();
                
                // Select the newly created project
                SelectedProject = Projects.FirstOrDefault(p => p.Id == dialog.CreatedProject.Id);
                
                SetStatus($"Project '{dialog.CreatedProject.Name}' created successfully from template");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening project template selection dialog");
            SetStatus("Failed to open project creation dialog");
        }
    }

    /// <summary>
    /// Opens an existing project.
    /// </summary>
    private async Task OpenProjectAsync()
    {
        // TODO: Show project selection dialog
        await Task.CompletedTask;
        SetStatus("Open project functionality will be implemented");
    }

    /// <summary>
    /// Saves the current project.
    /// </summary>
    private async Task SaveProjectAsync()
    {
        if (SelectedProject == null) return;
        
        await ExecuteAsync(
            async () => await _dataService.UpdateProjectAsync(SelectedProject),
            "Saving project...",
            "Project saved successfully");
    }

    /// <summary>
    /// Adds a new task to the current project.
    /// </summary>
    private async Task AddTaskAsync()
    {
        if (SelectedProject == null) return;
        
        try
        {
            var mainWindow = Application.Current.MainWindow;
            var dialog = TaskEditDialog.Create(_serviceProvider, null, SelectedProject.Id, mainWindow);
            
            var result = dialog.ShowDialog();
            
            if (result == true)
            {
                await LoadTasksForProjectAsync(SelectedProject.Id);
                SetStatus("New task created successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening task creation dialog");
            SetStatus("Failed to open task creation dialog");
        }
    }

    /// <summary>
    /// Edits the selected task.
    /// </summary>
    private async Task EditTaskAsync()
    {
        if (SelectedTask == null) return;
        
        try
        {
            var mainWindow = Application.Current.MainWindow;
            var dialog = TaskEditDialog.Create(_serviceProvider, SelectedTask, null, mainWindow);
            
            var result = dialog.ShowDialog();
            
            if (result == true)
            {
                await LoadTasksForProjectAsync(SelectedProject?.Id ?? SelectedTask.ProjectId);
                SetStatus("Task updated successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening task edit dialog");
            SetStatus("Failed to open task edit dialog");
        }
    }

    /// <summary>
    /// Deletes the selected task.
    /// </summary>
    private async Task DeleteTaskAsync()
    {
        if (SelectedTask == null) return;
        
        await ExecuteAsync(
            async () =>
            {
                await _dataService.DeleteTaskAsync(SelectedTask.Id);
                Tasks.Remove(SelectedTask);
                SelectedTask = null;
            },
            "Deleting task...",
            "Task deleted successfully");
    }

    /// <summary>
    /// Exports the current project to PDF.
    /// </summary>
    private async Task ExportPdfAsync()
    {
        if (SelectedProject == null) return;
        
        await ExecuteAsync(
            async () => await _pdfExportService.ExportProjectToPdfAsync(SelectedProject, Tasks.ToList()),
            "Exporting to PDF...",
            "PDF export completed successfully");
    }

    /// <summary>
    /// Zooms in on the timeline.
    /// </summary>
    private void ZoomIn()
    {
        ZoomLevel = Math.Min(ZoomLevel * 1.2, 5.0);
        SetStatus($"Zoom: {ZoomLevel:P0}");
    }

    /// <summary>
    /// Zooms out on the timeline.
    /// </summary>
    private void ZoomOut()
    {
        ZoomLevel = Math.Max(ZoomLevel / 1.2, 0.1);
        SetStatus($"Zoom: {ZoomLevel:P0}");
    }

    /// <summary>
    /// Resets the timeline zoom to 100%.
    /// </summary>
    private void ResetZoom()
    {
        ZoomLevel = 1.0;
        SetStatus("Zoom reset to 100%");
    }

    /// <summary>
    /// Toggles the visibility of the task details panel.
    /// </summary>
    private void ToggleTaskDetailsPanel()
    {
        IsTaskDetailsPanelVisible = !IsTaskDetailsPanelVisible;
        SetStatus($"Task details panel {(IsTaskDetailsPanelVisible ? "shown" : "hidden")}");
    }

    /// <summary>
    /// Clears all applied filters.
    /// </summary>
    private void ClearFilters()
    {
        SearchFilter = string.Empty;
        StatusFilter = null;
        PriorityFilter = null;
        AssigneeFilter = string.Empty;
        
        SetStatus("Filters cleared");
    }

    /// <summary>
    /// Opens the task dependency management dialog.
    /// </summary>
    private async Task ManageDependenciesAsync()
    {
        if (SelectedTask == null) return;
        
        try
        {
            var mainWindow = Application.Current.MainWindow;
            var dialog = TaskDependencyDialog.Create(_serviceProvider, SelectedTask, Tasks, mainWindow);
            
            var result = dialog.ShowDialog();
            
            if (result == true)
            {
                // Reload tasks to refresh dependency data
                await LoadTasksForProjectAsync(SelectedProject?.Id ?? SelectedTask.ProjectId);
                SetStatus("Task dependencies updated successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening task dependency dialog");
            SetStatus("Failed to open task dependency dialog");
        }
    }

    /// <summary>
    /// Applies current filters to the task list.
    /// </summary>
    private void ApplyFilters()
    {
        // TODO: Implement filtering logic
        // For now, just update the status
        var filterCount = 0;
        if (!string.IsNullOrEmpty(SearchFilter)) filterCount++;
        if (StatusFilter.HasValue) filterCount++;
        if (PriorityFilter.HasValue) filterCount++;
        if (!string.IsNullOrEmpty(AssigneeFilter)) filterCount++;
        
        if (filterCount > 0)
        {
            SetStatus($"{filterCount} filter(s) applied");
        }
    }

    /// <summary>
    /// Updates the timeline range based on the current project.
    /// </summary>
    private void UpdateTimelineRange()
    {
        if (SelectedProject == null || Tasks.Count == 0) return;
        
        var minDate = Tasks.Min(t => t.StartDate);
        var maxDate = Tasks.Max(t => t.EndDate);
        
        TimelineStart = minDate.AddDays(-7);
        TimelineEnd = maxDate.AddDays(7);
    }

    /// <summary>
    /// Changes the timeline view mode.
    /// </summary>
    /// <param name="newMode">The new view mode.</param>
    private void ChangeViewMode(TimelineViewMode newMode)
    {
        ViewMode = newMode;
        
        // Adjust timeline range based on view mode
        if (SelectedProject != null && Tasks.Count > 0)
        {
            var projectDuration = (int)(Tasks.Max(t => t.EndDate) - Tasks.Min(t => t.StartDate)).TotalDays;
            var padding = TimelineViewConfig.GetTimeUnitDays(newMode);
            
            TimelineStart = Tasks.Min(t => t.StartDate).AddDays(-padding);
            TimelineEnd = Tasks.Max(t => t.EndDate).AddDays(padding);
        }
        
        SetStatus($"Timeline view changed to {TimelineViewConfig.GetDisplayName(newMode)}");
        _logger.LogInformation("Timeline view mode changed to {ViewMode}", newMode);
    }

    /// <summary>
    /// Gets the available timeline view options.
    /// </summary>
    public static IReadOnlyList<TimelineViewMode> TimelineViewOptions { get; } = TimelineViewConfig.GetAllModes();

    /// <summary>
    /// Gets the available task status options for filtering.
    /// </summary>
    public static IReadOnlyList<Models.TaskStatus> StatusFilterOptions { get; } = Enum.GetValues<Models.TaskStatus>();

    /// <summary>
    /// Gets the available task priority options for filtering.
    /// </summary>
    public static IReadOnlyList<TaskPriority> PriorityFilterOptions { get; } = Enum.GetValues<TaskPriority>();

    /// <summary>
    /// Gets a value indicating whether there are no tasks to display.
    /// </summary>
    public bool HasNoTasks => Tasks.Count == 0;
}