using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.ViewModels.Base;

namespace TouchGanttChart.ViewModels;

/// <summary>
/// ViewModel for the TaskEditDialog, handling task creation and editing
/// with touch-optimized interface and validation.
/// </summary>
public partial class TaskEditDialogViewModel : ViewModelBase, IDisposable
{
    private readonly IDataService _dataService;
    private readonly ILogger<TaskEditDialogViewModel> _logger;
    private GanttTask _originalTask;

    [ObservableProperty]
    private GanttTask _task = new();

    [ObservableProperty]
    private string _windowTitle = "Task Details";

    [ObservableProperty]
    private string _saveButtonText = "Save";

    [ObservableProperty]
    private bool _canSave = true;

    [ObservableProperty]
    private bool _isNewTask;

    /// <summary>
    /// Gets whether the progress field should be editable (only for leaf tasks)
    /// </summary>
    public bool IsProgressEditable => Task?.IsLeafTask ?? true;

    /// <summary>
    /// Gets the effective progress to display (calculated for parent tasks, manual for leaf tasks)
    /// </summary>
    public int EffectiveProgress => Task?.CalculatedProgress ?? 0;

    /// <summary>
    /// Gets the progress label text
    /// </summary>
    public string ProgressLabel => IsProgressEditable ? "Progress %" : "Progress % (Auto-calculated)";

    /// <summary>
    /// Gets whether this task has subtasks
    /// </summary>
    public bool HasSubTasks => Task?.HasSubTasks ?? false;

    /// <summary>
    /// Gets the hierarchy level of this task
    /// </summary>
    public int HierarchyLevel => Task?.HierarchyLevel ?? 0;

    /// <summary>
    /// Gets the hierarchy indent text for display
    /// </summary>
    public string HierarchyIndent => Task?.HierarchyIndent ?? "";

    /// <summary>
    /// Gets the completion status text for display
    /// </summary>
    public string CompletionStatusText
    {
        get
        {
            if (Task.CompletionDate == null) return string.Empty;
            
            var variance = Task.CompletionVarianceDays;
            if (variance > 0)
                return $"Completed {variance:F1} days early";
            else if (variance < 0)
                return $"Completed {Math.Abs(variance):F1} days late";
            else
                return "Completed on schedule";
        }
    }

    /// <summary>
    /// Gets the completion status color for display
    /// </summary>
    public string CompletionStatusColor
    {
        get
        {
            if (Task.CompletionDate == null) return "#6c757d";
            
            var variance = Task.CompletionVarianceDays;
            if (variance > 0)
                return "#28a745"; // Green for early
            else if (variance < 0)
                return "#dc3545"; // Red for late
            else
                return "#007bff"; // Blue for on time
        }
    }

    /// <summary>
    /// Event fired when a dialog result is requested
    /// </summary>
    public event EventHandler<bool?>? DialogResultRequested;

    /// <summary>
    /// Event fired when a task's completion date changes and dependencies need shifting
    /// </summary>
    public event EventHandler<GanttTask>? TaskCompletionChanged;

    /// <summary>
    /// Available task status options
    /// </summary>
    public ReadOnlyCollection<TouchGanttChart.Models.TaskStatus> StatusOptions { get; } = new(
        Enum.GetValues<TouchGanttChart.Models.TaskStatus>().ToList());

    /// <summary>
    /// Available task priority options
    /// </summary>
    public ReadOnlyCollection<TaskPriority> PriorityOptions { get; } = new(
        Enum.GetValues<TaskPriority>().ToList());

    /// <summary>
    /// Available task category options
    /// </summary>
    public ReadOnlyCollection<string> CategoryOptions { get; } = new(new[]
    {
        "General",
        "Mechanical",
        "Electrical",
        "Software",
        "Documentation",
        "Testing",
        "Design",
        "Research",
        "Planning",
        "Marketing"
    });

    /// <summary>
    /// Available assignee options (team roles)
    /// </summary>
    public ReadOnlyCollection<string> AssigneeOptions { get; } = new(new[]
    {
        "",
        "Mechanical",
        "Electrical", 
        "Programming",
        "PR",
        "Leadership"
    });

    public TaskEditDialogViewModel(IDataService dataService, ILogger<TaskEditDialogViewModel> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _originalTask = new GanttTask();
        IsNewTask = true;
        
        // Set default values for new task
        Task.StartDate = DateTime.Today;
        Task.EndDate = DateTime.Today.AddDays(1);
        Task.Status = TouchGanttChart.Models.TaskStatus.NotStarted;
        Task.Priority = TaskPriority.Normal;
        
        // Note: Property change validation will be handled through explicit calls
        
        UpdateWindowState();
        ValidateTask();
    }

    /// <summary>
    /// Sets up the dialog for editing an existing task
    /// </summary>
    /// <param name="task">The task to edit</param>
    public void SetTask(GanttTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        
        _originalTask = task;
        IsNewTask = false;
        
        // Create a copy to edit
        Task = new GanttTask
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            StartDate = task.StartDate,
            EndDate = task.EndDate,
            Status = task.Status,
            Priority = task.Priority,
            Progress = task.Progress,
            Assignee = task.Assignee,
            Category = task.Category,
            EstimatedHours = task.EstimatedHours,
            ActualHours = task.ActualHours,
            CompletionDate = task.CompletionDate,
            ProjectId = task.ProjectId,
            ParentTaskId = task.ParentTaskId,
            CreatedDate = task.CreatedDate,
            LastModifiedDate = task.LastModifiedDate
        };
        
        UpdateWindowState();
        ValidateTask();
        
        _logger.LogInformation("Task edit dialog configured for task: {TaskId} - {TaskName}", 
            task.Id, task.Name);
    }

    /// <summary>
    /// Sets up the dialog for creating a new task
    /// </summary>
    /// <param name="projectId">The project ID for the new task</param>
    public void SetNewTask(int projectId)
    {
        _originalTask = new GanttTask();
        IsNewTask = true;
        
        Task = new GanttTask
        {
            ProjectId = projectId,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1),
            Status = TouchGanttChart.Models.TaskStatus.NotStarted,
            Priority = TaskPriority.Normal,
            Progress = 0,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        
        UpdateWindowState();
        ValidateTask();
        
        _logger.LogInformation("Task edit dialog configured for new task in project: {ProjectId}", projectId);
    }

    /// <summary>
    /// Command to save the task
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            
            if (!ValidateTask())
            {
                StatusMessage = "Please fix validation errors before saving.";
                return;
            }

            Task.LastModifiedDate = DateTime.UtcNow;

            // Check if completion date changed for dependency shifting
            bool completionDateChanged = false;
            if (!IsNewTask && _originalTask.CompletionDate != Task.CompletionDate)
            {
                completionDateChanged = true;
                _logger.LogInformation("Task {TaskName} completion date changed from {OldDate} to {NewDate}", 
                    Task.Name, 
                    _originalTask.CompletionDate?.ToShortDateString() ?? "None",
                    Task.CompletionDate?.ToShortDateString() ?? "None");
            }

            if (IsNewTask)
            {
                Task.CreatedDate = DateTime.UtcNow;
                var createdTask = await _dataService.CreateTaskAsync(Task);
                _logger.LogInformation("Created new task: {TaskId} - {TaskName}", 
                    createdTask.Id, createdTask.Name);
                StatusMessage = "Task created successfully.";
            }
            else
            {
                var updatedTask = await _dataService.UpdateTaskAsync(Task);
                _logger.LogInformation("Updated task: {TaskId} - {TaskName}", 
                    updatedTask.Id, updatedTask.Name);
                StatusMessage = "Task updated successfully.";

                // Fire completion changed event if needed
                if (completionDateChanged)
                {
                    TaskCompletionChanged?.Invoke(this, updatedTask);
                }
            }

            DialogResultRequested?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving task: {TaskName}", Task.Name);
            StatusMessage = $"Error saving task: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to cancel editing
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInformation("Task editing cancelled");
        DialogResultRequested?.Invoke(this, false);
    }

    /// <summary>
    /// Updates the window state based on whether we're creating or editing
    /// </summary>
    private void UpdateWindowState()
    {
        if (IsNewTask)
        {
            WindowTitle = "Create New Task";
            SaveButtonText = "Create Task";
            IsNewTask = true;
        }
        else
        {
            WindowTitle = $"Edit Task: {Task.Name}";
            SaveButtonText = "Update Task";
            IsNewTask = false;
        }
    }

    /// <summary>
    /// Validates the current task and updates CanSave
    /// </summary>
    /// <returns>True if the task is valid</returns>
    private bool ValidateTask()
    {
        var errors = new List<string>();

        // Required field validation
        if (string.IsNullOrWhiteSpace(Task.Name))
        {
            errors.Add("Task name is required.");
        }

        // Date validation
        if (Task.EndDate < Task.StartDate)
        {
            errors.Add("End date must be after start date.");
        }

        // Progress validation
        if (Task.Progress < 0 || Task.Progress > 100)
        {
            errors.Add("Progress must be between 0 and 100.");
        }

        // Hours validation
        if (Task.EstimatedHours < 0)
        {
            errors.Add("Estimated hours cannot be negative.");
        }

        if (Task.ActualHours < 0)
        {
            errors.Add("Actual hours cannot be negative.");
        }

        var isValid = errors.Count == 0;
        CanSave = isValid && !IsLoading;

        if (!isValid)
        {
            StatusMessage = string.Join(" ", errors);
        }
        else
        {
            StatusMessage = "Ready to save.";
        }

        return isValid;
    }

    /// <summary>
    /// Triggers validation and updates UI when task properties change
    /// Call this method after modifying task properties
    /// </summary>
    public void OnTaskChanged()
    {
        ValidateTask();
        
        // Update window title if name changes
        if (!IsNewTask)
        {
            WindowTitle = $"Edit Task: {Task.Name}";
        }

        // Notify property changes for hierarchical properties
        OnPropertyChanged(nameof(IsProgressEditable));
        OnPropertyChanged(nameof(EffectiveProgress));
        OnPropertyChanged(nameof(ProgressLabel));
        OnPropertyChanged(nameof(HasSubTasks));
        OnPropertyChanged(nameof(HierarchyLevel));
        OnPropertyChanged(nameof(HierarchyIndent));
        OnPropertyChanged(nameof(CompletionStatusText));
        OnPropertyChanged(nameof(CompletionStatusColor));
    }

    /// <summary>
    /// Cleanup when the view model is disposed
    /// </summary>
    public void Dispose()
    {
        // No cleanup needed for this implementation
    }
}