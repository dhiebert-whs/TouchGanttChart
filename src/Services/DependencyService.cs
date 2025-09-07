using Microsoft.Extensions.Logging;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;

namespace TouchGanttChart.Services;

/// <summary>
/// Service for managing task dependencies and date adjustments
/// </summary>
public class DependencyService : IDependencyService
{
    private readonly ILogger<DependencyService> _logger;

    public DependencyService(ILogger<DependencyService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Shifts dependent task dates when a predecessor task completion date changes
    /// </summary>
    public async Task<List<GanttTask>> ShiftDependentTasksAsync(GanttTask completedTask, List<GanttTask> allTasks)
    {
        var updatedTasks = new List<GanttTask>();

        if (completedTask.CompletionDate == null)
        {
            _logger.LogDebug("Task {TaskName} has no completion date, no dependency shifting needed", completedTask.Name);
            return updatedTasks;
        }

        var completionVariance = completedTask.CompletionVarianceDays;
        if (Math.Abs(completionVariance) < 0.1) // Less than 0.1 days difference
        {
            _logger.LogDebug("Task {TaskName} completed on schedule, no dependency shifting needed", completedTask.Name);
            return updatedTasks;
        }

        _logger.LogInformation("Task {TaskName} completed with {Variance:F1} day variance, checking dependencies", 
            completedTask.Name, completionVariance);

        // Find all tasks that depend on this completed task
        var dependentTasks = FindDirectDependents(completedTask, allTasks);
        
        foreach (var dependentTask in dependentTasks)
        {
            // Only shift if the dependent task hasn't started yet or is in progress
            if (dependentTask.Status == TaskStatus.Completed || dependentTask.Status == TaskStatus.Cancelled)
                continue;

            var originalStart = dependentTask.StartDate;
            var originalEnd = dependentTask.EndDate;

            // Calculate new start date based on completion date
            var newStartDate = completedTask.CompletionDate.Value.AddDays(1); // Start day after completion
            
            // Only shift forward if the new start date is later than the original
            if (newStartDate <= dependentTask.StartDate)
                continue;

            // Preserve task duration
            var duration = dependentTask.EndDate - dependentTask.StartDate;
            dependentTask.StartDate = newStartDate;
            dependentTask.EndDate = newStartDate.Add(duration);
            dependentTask.LastModifiedDate = DateTime.UtcNow;

            updatedTasks.Add(dependentTask);

            _logger.LogInformation("Shifted dependent task {TaskName} from {OriginalStart} to {NewStart}", 
                dependentTask.Name, originalStart.ToShortDateString(), newStartDate.ToShortDateString());

            // Recursively shift tasks that depend on this shifted task
            var nestedDependents = await ShiftDependentTasksAsync(dependentTask, allTasks);
            updatedTasks.AddRange(nestedDependents);
        }

        return updatedTasks;
    }

    /// <summary>
    /// Validates that task dependencies don't create circular references
    /// </summary>
    public bool ValidateDependencies(GanttTask task, List<GanttTask> allTasks)
    {
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();

        return !HasCircularDependency(task, allTasks, visited, recursionStack);
    }

    /// <summary>
    /// Gets the critical path for a project
    /// </summary>
    public List<GanttTask> GetCriticalPath(List<GanttTask> tasks)
    {
        // Simple critical path implementation - find the longest path
        var criticalPath = new List<GanttTask>();
        
        // Find tasks with no dependencies (project start)
        var startTasks = tasks.Where(t => t.Dependencies == null || t.Dependencies.Count == 0).ToList();
        
        foreach (var startTask in startTasks)
        {
            var path = FindLongestPath(startTask, tasks, new HashSet<int>());
            if (path.Sum(t => t.Duration.TotalDays) > criticalPath.Sum(t => t.Duration.TotalDays))
            {
                criticalPath = path;
            }
        }

        _logger.LogInformation("Critical path identified with {TaskCount} tasks and {Duration:F1} day duration", 
            criticalPath.Count, criticalPath.Sum(t => t.Duration.TotalDays));

        return criticalPath;
    }

    private List<GanttTask> FindDirectDependents(GanttTask task, List<GanttTask> allTasks)
    {
        return allTasks.Where(t => t.Dependencies != null && t.Dependencies.Any(d => d.Id == task.Id)).ToList();
    }

    private bool HasCircularDependency(GanttTask task, List<GanttTask> allTasks, HashSet<int> visited, HashSet<int> recursionStack)
    {
        if (recursionStack.Contains(task.Id))
            return true; // Circular dependency found

        if (visited.Contains(task.Id))
            return false; // Already processed

        visited.Add(task.Id);
        recursionStack.Add(task.Id);

        if (task.Dependencies != null)
        {
            foreach (var dependency in task.Dependencies)
            {
                if (HasCircularDependency(dependency, allTasks, visited, recursionStack))
                    return true;
            }
        }

        recursionStack.Remove(task.Id);
        return false;
    }

    private List<GanttTask> FindLongestPath(GanttTask currentTask, List<GanttTask> allTasks, HashSet<int> visited)
    {
        if (visited.Contains(currentTask.Id))
            return new List<GanttTask>(); // Avoid infinite loops

        visited.Add(currentTask.Id);
        var longestPath = new List<GanttTask> { currentTask };

        var dependents = FindDirectDependents(currentTask, allTasks);
        var maxSubPath = new List<GanttTask>();

        foreach (var dependent in dependents)
        {
            var subPath = FindLongestPath(dependent, allTasks, new HashSet<int>(visited));
            if (subPath.Sum(t => t.Duration.TotalDays) > maxSubPath.Sum(t => t.Duration.TotalDays))
            {
                maxSubPath = subPath;
            }
        }

        longestPath.AddRange(maxSubPath);
        return longestPath;
    }
}