using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.ViewModels.Base;

namespace TouchGanttChart.ViewModels;

/// <summary>
/// ViewModel for the TaskDependencyDialog, handling task dependency management
/// with touch-optimized interface.
/// </summary>
public partial class TaskDependencyDialogViewModel : ViewModelBase, IDisposable
{
    private readonly IDataService _dataService;
    private readonly ILogger<TaskDependencyDialogViewModel> _logger;
    private GanttTask _currentTask = new();
    private readonly List<GanttTask> _originalDependencies = new();

    [ObservableProperty]
    private string _taskName = string.Empty;

    [ObservableProperty]
    private GanttTask? _selectedAvailableTask;

    [ObservableProperty]
    private GanttTask? _selectedCurrentDependency;

    /// <summary>
    /// Event fired when a dialog result is requested
    /// </summary>
    public event EventHandler<bool?>? DialogResultRequested;

    /// <summary>
    /// Available tasks that can be added as dependencies
    /// </summary>
    public ObservableCollection<GanttTask> AvailableTasks { get; } = new();

    /// <summary>
    /// Current dependencies of the task
    /// </summary>
    public ObservableCollection<GanttTask> CurrentDependencies { get; } = new();

    public TaskDependencyDialogViewModel(IDataService dataService, ILogger<TaskDependencyDialogViewModel> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sets up the dialog for managing dependencies of a specific task
    /// </summary>
    /// <param name="task">The task to manage dependencies for</param>
    /// <param name="allTasks">All available tasks in the project</param>
    public void SetTask(GanttTask task, IEnumerable<GanttTask> allTasks)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (allTasks == null) throw new ArgumentNullException(nameof(allTasks));

        _currentTask = task;
        TaskName = task.Name;

        // Store original dependencies for cancellation
        _originalDependencies.Clear();
        _originalDependencies.AddRange(task.Dependencies);

        // Populate current dependencies
        CurrentDependencies.Clear();
        foreach (var dependency in task.Dependencies)
        {
            CurrentDependencies.Add(dependency);
        }

        // Populate available tasks (exclude self and current dependencies)
        AvailableTasks.Clear();
        var availableTasks = allTasks
            .Where(t => t.Id != task.Id && !task.Dependencies.Any(d => d.Id == t.Id))
            .OrderBy(t => t.StartDate)
            .ThenBy(t => t.Name);

        foreach (var availableTask in availableTasks)
        {
            AvailableTasks.Add(availableTask);
        }

        // Update UI state
        OnPropertyChanged(nameof(HasNoDependencies));
        OnPropertyChanged(nameof(HasSelectedAvailableTask));
        OnPropertyChanged(nameof(HasSelectedCurrentDependency));

        _logger.LogInformation("Task dependency dialog configured for task: {TaskId} - {TaskName}", 
            task.Id, task.Name);
    }

    /// <summary>
    /// Command to add a dependency
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasSelectedAvailableTask))]
    private void AddDependency()
    {
        if (SelectedAvailableTask == null) return;

        try
        {
            // Add to current dependencies
            CurrentDependencies.Add(SelectedAvailableTask);

            // Remove from available tasks
            AvailableTasks.Remove(SelectedAvailableTask);

            // Clear selection
            SelectedAvailableTask = null;

            // Update UI state
            OnPropertyChanged(nameof(HasNoDependencies));
            OnPropertyChanged(nameof(HasSelectedAvailableTask));

            _logger.LogDebug("Added dependency: {DependencyName}", SelectedAvailableTask?.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding dependency");
            SetStatus("Failed to add dependency");
        }
    }

    /// <summary>
    /// Command to remove a dependency
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasSelectedCurrentDependency))]
    private void RemoveDependency()
    {
        if (SelectedCurrentDependency == null) return;

        try
        {
            // Remove from current dependencies
            CurrentDependencies.Remove(SelectedCurrentDependency);

            // Add back to available tasks (insert in correct position)
            var insertIndex = AvailableTasks
                .TakeWhile(t => t.StartDate < SelectedCurrentDependency.StartDate || 
                              (t.StartDate == SelectedCurrentDependency.StartDate && 
                               string.Compare(t.Name, SelectedCurrentDependency.Name, StringComparison.OrdinalIgnoreCase) < 0))
                .Count();

            AvailableTasks.Insert(insertIndex, SelectedCurrentDependency);

            // Clear selection
            SelectedCurrentDependency = null;

            // Update UI state
            OnPropertyChanged(nameof(HasNoDependencies));
            OnPropertyChanged(nameof(HasSelectedCurrentDependency));

            _logger.LogDebug("Removed dependency: {DependencyName}", SelectedCurrentDependency?.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing dependency");
            SetStatus("Failed to remove dependency");
        }
    }

    /// <summary>
    /// Command to save changes
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            SetStatus("Saving dependencies...");

            // Update the task's dependencies
            _currentTask.Dependencies.Clear();
            foreach (var dependency in CurrentDependencies)
            {
                _currentTask.Dependencies.Add(dependency);
            }

            // Save to database
            await _dataService.UpdateTaskAsync(_currentTask);

            _logger.LogInformation("Successfully updated dependencies for task: {TaskId}", _currentTask.Id);
            SetStatus("Dependencies saved successfully");

            DialogResultRequested?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving task dependencies: {TaskName}", _currentTask.Name);
            SetStatus($"Error saving dependencies: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to cancel changes
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // Restore original dependencies
        _currentTask.Dependencies.Clear();
        foreach (var dependency in _originalDependencies)
        {
            _currentTask.Dependencies.Add(dependency);
        }

        _logger.LogInformation("Task dependency editing cancelled");
        DialogResultRequested?.Invoke(this, false);
    }

    /// <summary>
    /// Gets a value indicating whether there are no current dependencies
    /// </summary>
    public bool HasNoDependencies => CurrentDependencies.Count == 0;

    /// <summary>
    /// Gets a value indicating whether an available task is selected
    /// </summary>
    public bool HasSelectedAvailableTask => SelectedAvailableTask != null;

    /// <summary>
    /// Gets a value indicating whether a current dependency is selected
    /// </summary>
    public bool HasSelectedCurrentDependency => SelectedCurrentDependency != null;

    /// <summary>
    /// Updates command states when selection changes
    /// </summary>
    partial void OnSelectedAvailableTaskChanged(GanttTask? value)
    {
        AddDependencyCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(HasSelectedAvailableTask));
    }

    /// <summary>
    /// Updates command states when selection changes
    /// </summary>
    partial void OnSelectedCurrentDependencyChanged(GanttTask? value)
    {
        RemoveDependencyCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(HasSelectedCurrentDependency));
    }

    /// <summary>
    /// Cleanup when the view model is disposed
    /// </summary>
    public void Dispose()
    {
        // No cleanup needed for this implementation
    }
}