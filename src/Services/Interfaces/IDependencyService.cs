using TouchGanttChart.Models;

namespace TouchGanttChart.Services.Interfaces;

/// <summary>
/// Service for managing task dependencies and date adjustments
/// </summary>
public interface IDependencyService
{
    /// <summary>
    /// Shifts dependent task dates when a predecessor task completion date changes
    /// </summary>
    /// <param name="completedTask">The task that was completed</param>
    /// <param name="allTasks">All tasks in the project</param>
    /// <returns>List of tasks that were updated</returns>
    Task<List<GanttTask>> ShiftDependentTasksAsync(GanttTask completedTask, List<GanttTask> allTasks);

    /// <summary>
    /// Validates that task dependencies don't create circular references
    /// </summary>
    /// <param name="task">The task to validate</param>
    /// <param name="allTasks">All tasks in the project</param>
    /// <returns>True if dependencies are valid</returns>
    bool ValidateDependencies(GanttTask task, List<GanttTask> allTasks);

    /// <summary>
    /// Gets the critical path for a project
    /// </summary>
    /// <param name="tasks">All tasks in the project</param>
    /// <returns>List of tasks on the critical path</returns>
    List<GanttTask> GetCriticalPath(List<GanttTask> tasks);
}