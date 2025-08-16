using TouchGanttChart.Models;

namespace TouchGanttChart.Services.Interfaces;

/// <summary>
/// Interface for data access operations for projects and tasks.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Gets all projects asynchronously.
    /// </summary>
    /// <returns>A collection of projects.</returns>
    Task<IEnumerable<Project>> GetProjectsAsync();

    /// <summary>
    /// Gets all non-archived projects asynchronously.
    /// </summary>
    /// <returns>A collection of active projects.</returns>
    Task<IEnumerable<Project>> GetActiveProjectsAsync();

    /// <summary>
    /// Gets a project by its ID asynchronously.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>The project if found, null otherwise.</returns>
    Task<Project?> GetProjectByIdAsync(int projectId);

    /// <summary>
    /// Creates a new project asynchronously.
    /// </summary>
    /// <param name="project">The project to create.</param>
    /// <returns>The created project with assigned ID.</returns>
    Task<Project> CreateProjectAsync(Project project);

    /// <summary>
    /// Updates an existing project asynchronously.
    /// </summary>
    /// <param name="project">The project to update.</param>
    /// <returns>The updated project.</returns>
    Task<Project> UpdateProjectAsync(Project project);

    /// <summary>
    /// Deletes a project asynchronously.
    /// </summary>
    /// <param name="projectId">The project ID to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteProjectAsync(int projectId);

    /// <summary>
    /// Archives a project asynchronously.
    /// </summary>
    /// <param name="projectId">The project ID to archive.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ArchiveProjectAsync(int projectId);

    /// <summary>
    /// Gets all tasks for a specific project asynchronously.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>A collection of tasks.</returns>
    Task<IEnumerable<GanttTask>> GetTasksByProjectIdAsync(int projectId);

    /// <summary>
    /// Gets a task by its ID asynchronously.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>The task if found, null otherwise.</returns>
    Task<GanttTask?> GetTaskByIdAsync(int taskId);

    /// <summary>
    /// Creates a new task asynchronously.
    /// </summary>
    /// <param name="task">The task to create.</param>
    /// <returns>The created task with assigned ID.</returns>
    Task<GanttTask> CreateTaskAsync(GanttTask task);

    /// <summary>
    /// Updates an existing task asynchronously.
    /// </summary>
    /// <param name="task">The task to update.</param>
    /// <returns>The updated task.</returns>
    Task<GanttTask> UpdateTaskAsync(GanttTask task);

    /// <summary>
    /// Deletes a task asynchronously.
    /// </summary>
    /// <param name="taskId">The task ID to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteTaskAsync(int taskId);

    /// <summary>
    /// Gets all subtasks for a parent task asynchronously.
    /// </summary>
    /// <param name="parentTaskId">The parent task ID.</param>
    /// <returns>A collection of subtasks.</returns>
    Task<IEnumerable<GanttTask>> GetSubTasksAsync(int parentTaskId);

    /// <summary>
    /// Gets all dependencies for a task asynchronously.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>A collection of dependency tasks.</returns>
    Task<IEnumerable<GanttTask>> GetTaskDependenciesAsync(int taskId);

    /// <summary>
    /// Adds a dependency relationship between tasks asynchronously.
    /// </summary>
    /// <param name="dependentTaskId">The dependent task ID.</param>
    /// <param name="prerequisiteTaskId">The prerequisite task ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddTaskDependencyAsync(int dependentTaskId, int prerequisiteTaskId);

    /// <summary>
    /// Removes a dependency relationship between tasks asynchronously.
    /// </summary>
    /// <param name="dependentTaskId">The dependent task ID.</param>
    /// <param name="prerequisiteTaskId">The prerequisite task ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveTaskDependencyAsync(int dependentTaskId, int prerequisiteTaskId);

    /// <summary>
    /// Gets tasks filtered by status asynchronously.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="status">The task status to filter by.</param>
    /// <returns>A collection of filtered tasks.</returns>
    Task<IEnumerable<GanttTask>> GetTasksByStatusAsync(int projectId, Models.TaskStatus status);

    /// <summary>
    /// Gets tasks filtered by assignee asynchronously.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="assignee">The assignee name to filter by.</param>
    /// <returns>A collection of filtered tasks.</returns>
    Task<IEnumerable<GanttTask>> GetTasksByAssigneeAsync(int projectId, string assignee);

    /// <summary>
    /// Gets overdue tasks for a project asynchronously.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>A collection of overdue tasks.</returns>
    Task<IEnumerable<GanttTask>> GetOverdueTasksAsync(int projectId);

    /// <summary>
    /// Searches tasks by name or description asynchronously.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <returns>A collection of matching tasks.</returns>
    Task<IEnumerable<GanttTask>> SearchTasksAsync(int projectId, string searchTerm);

    /// <summary>
    /// Gets project statistics asynchronously.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>Project statistics.</returns>
    Task<ProjectStatistics> GetProjectStatisticsAsync(int projectId);
}

/// <summary>
/// Represents project statistics.
/// </summary>
public class ProjectStatistics
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double ProgressPercentage { get; set; }
    public double TotalEstimatedHours { get; set; }
    public double TotalActualHours { get; set; }
    public DateTime EarliestStartDate { get; set; }
    public DateTime LatestEndDate { get; set; }
}