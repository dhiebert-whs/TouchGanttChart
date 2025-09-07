using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.ViewModels.Base;

namespace TouchGanttChart.ViewModels;

/// <summary>
/// ViewModel for the day view showing tasks for a specific date
/// Provides date navigation and task management for daily planning
/// </summary>
public partial class DayViewModel : ViewModelBase
{
    private readonly IDataService _dataService;
    private readonly ILogger<DayViewModel> _logger;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private Project? _currentProject;

    /// <summary>
    /// Collection of tasks scheduled for the selected date
    /// </summary>
    public ObservableCollection<GanttTask> DayTasks { get; } = new();

    /// <summary>
    /// All tasks in the current project (for filtering by date)
    /// </summary>
    public ObservableCollection<GanttTask> AllTasks { get; } = new();

    /// <summary>
    /// Event fired when a task is selected for editing
    /// </summary>
    public event EventHandler<GanttTask>? TaskEditRequested;

    /// <summary>
    /// Gets summary text for the day's tasks
    /// </summary>
    public string DayTasksSummary
    {
        get
        {
            var total = DayTasks.Count;
            if (total == 0) return "No tasks scheduled";
            
            var completed = DayTasks.Count(t => t.Status == Models.TaskStatus.Completed);
            var inProgress = DayTasks.Count(t => t.Status == Models.TaskStatus.InProgress);
            
            if (completed == total) return $"All {total} tasks completed! 🎉";
            if (inProgress > 0) return $"{total} tasks: {completed} done, {inProgress} in progress";
            return $"{total} tasks scheduled";
        }
    }

    /// <summary>
    /// Gets completion summary text
    /// </summary>
    public string CompletionSummary
    {
        get
        {
            if (DayTasks.Count == 0) return "Ready for new tasks";
            
            var totalHours = DayTasks.Sum(t => t.EstimatedHours);
            var completedHours = DayTasks.Where(t => t.Status == Models.TaskStatus.Completed).Sum(t => t.EstimatedHours);
            
            return $"{completedHours:F1} / {totalHours:F1} hours completed";
        }
    }

    /// <summary>
    /// Gets the completion percentage for the day
    /// </summary>
    public double DayCompletionPercentage
    {
        get
        {
            if (DayTasks.Count == 0) return 0;
            
            var totalTasks = DayTasks.Count;
            var completedTasks = DayTasks.Count(t => t.Status == Models.TaskStatus.Completed);
            
            return Math.Round((double)completedTasks / totalTasks * 100, 1);
        }
    }

    /// <summary>
    /// Gets whether there are tasks for the selected day
    /// </summary>
    public bool HasDayTasks => DayTasks.Count > 0;

    public DayViewModel(IDataService dataService, ILogger<DayViewModel> logger) : base(logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger;
        
        Title = "Day View";
    }

    /// <summary>
    /// Initializes the day view with the current project
    /// </summary>
    /// <param name="project">The current project</param>
    /// <param name="tasks">All tasks in the project</param>
    public async Task InitializeAsync(Project project, IEnumerable<GanttTask> tasks)
    {
        CurrentProject = project;
        
        AllTasks.Clear();
        foreach (var task in tasks)
        {
            AllTasks.Add(task);
        }

        await RefreshDayTasksAsync();
        
        _logger.LogInformation("Day view initialized for project {ProjectName} with {TaskCount} total tasks", 
            project.Name, AllTasks.Count);
    }

    /// <summary>
    /// Command to navigate to the previous day
    /// </summary>
    [RelayCommand]
    private async Task PreviousDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        await RefreshDayTasksAsync();
        _logger.LogDebug("Navigated to previous day: {Date}", SelectedDate.ToShortDateString());
    }

    /// <summary>
    /// Command to navigate to the next day
    /// </summary>
    [RelayCommand]
    private async Task NextDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(1);
        await RefreshDayTasksAsync();
        _logger.LogDebug("Navigated to next day: {Date}", SelectedDate.ToShortDateString());
    }

    /// <summary>
    /// Command to navigate to today
    /// </summary>
    [RelayCommand]
    private async Task TodayAsync()
    {
        SelectedDate = DateTime.Today;
        await RefreshDayTasksAsync();
        _logger.LogDebug("Navigated to today: {Date}", SelectedDate.ToShortDateString());
    }

    /// <summary>
    /// Command to handle task selection
    /// </summary>
    [RelayCommand]
    private void TaskSelected(GanttTask task)
    {
        if (task != null)
        {
            TaskEditRequested?.Invoke(this, task);
            _logger.LogDebug("Task selected for editing: {TaskName}", task.Name);
        }
    }

    /// <summary>
    /// Command to toggle task status (start, in progress, complete)
    /// </summary>
    [RelayCommand]
    private async Task ToggleTaskStatusAsync(GanttTask task)
    {
        if (task == null) return;

        var originalStatus = task.Status;
        
        // Cycle through statuses: NotStarted -> InProgress -> Completed
        task.Status = task.Status switch
        {
            Models.TaskStatus.NotStarted => Models.TaskStatus.InProgress,
            Models.TaskStatus.InProgress => Models.TaskStatus.Completed,
            Models.TaskStatus.Completed => Models.TaskStatus.NotStarted,
            _ => Models.TaskStatus.InProgress
        };

        // Update progress based on status
        task.Progress = task.Status switch
        {
            Models.TaskStatus.NotStarted => 0,
            Models.TaskStatus.InProgress => Math.Max(task.Progress, 1), // At least 1% if in progress
            Models.TaskStatus.Completed => 100,
            _ => task.Progress
        };

        // Set completion date if completed
        if (task.Status == Models.TaskStatus.Completed && originalStatus != Models.TaskStatus.Completed)
        {
            task.CompletionDate = DateTime.Now;
        }
        else if (task.Status != Models.TaskStatus.Completed)
        {
            task.CompletionDate = null;
        }

        try
        {
            await _dataService.UpdateTaskAsync(task);
            await RefreshDayTasksAsync(); // Refresh to update summaries
            
            _logger.LogInformation("Task {TaskName} status changed from {OldStatus} to {NewStatus}", 
                task.Name, originalStatus, task.Status);

            // If task completed early, trigger dependency shifting
            if (task.Status == Models.TaskStatus.Completed && task.IsCompletedEarly)
            {
                // This would trigger dependency shifting logic
                _logger.LogInformation("Task {TaskName} completed early, may affect dependencies", task.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status for {TaskName}", task.Name);
            
            // Revert changes on error
            task.Status = originalStatus;
            SetStatus($"Error updating task: {ex.Message}");
        }
    }

    /// <summary>
    /// Refreshes the tasks for the selected date
    /// </summary>
    private async Task RefreshDayTasksAsync()
    {
        try
        {
            IsLoading = true;
            
            DayTasks.Clear();
            
            // Filter tasks that fall on the selected date
            var selectedDate = SelectedDate.Date;
            var tasksForDay = AllTasks.Where(task =>
                task.StartDate.Date <= selectedDate && task.EndDate.Date >= selectedDate)
                .OrderBy(t => t.Priority == TaskPriority.Critical ? 0 : 
                             t.Priority == TaskPriority.High ? 1 :
                             t.Priority == TaskPriority.Normal ? 2 : 3)
                .ThenBy(t => t.StartDate)
                .ThenBy(t => t.Name);

            foreach (var task in tasksForDay)
            {
                DayTasks.Add(task);
            }

            // Notify property changes for computed properties
            OnPropertyChanged(nameof(DayTasksSummary));
            OnPropertyChanged(nameof(CompletionSummary));
            OnPropertyChanged(nameof(DayCompletionPercentage));
            OnPropertyChanged(nameof(HasDayTasks));

            SetStatus($"Showing {DayTasks.Count} tasks for {SelectedDate:dddd, MMM dd}");
            
            _logger.LogDebug("Refreshed day tasks for {Date}: {TaskCount} tasks found", 
                SelectedDate.ToShortDateString(), DayTasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing day tasks for {Date}", SelectedDate.ToShortDateString());
            SetStatus($"Error loading tasks: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handles date selection changes
    /// </summary>
    partial void OnSelectedDateChanged(DateTime value)
    {
        _ = RefreshDayTasksAsync();
    }

    /// <summary>
    /// Updates tasks when the project tasks change
    /// </summary>
    /// <param name="updatedTasks">The updated task collection</param>
    public async Task UpdateTasksAsync(IEnumerable<GanttTask> updatedTasks)
    {
        AllTasks.Clear();
        foreach (var task in updatedTasks)
        {
            AllTasks.Add(task);
        }

        await RefreshDayTasksAsync();
        
        _logger.LogDebug("Updated day view with {TaskCount} tasks", AllTasks.Count);
    }
}