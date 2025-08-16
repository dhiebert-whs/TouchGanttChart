using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TouchGanttChart.Data;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;

namespace TouchGanttChart.Services.Implementations;

/// <summary>
/// Implementation of data access operations for projects and tasks.
/// </summary>
public class DataService : IDataService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataService> _logger;

    /// <summary>
    /// Initializes a new instance of the DataService class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public DataService(AppDbContext context, ILogger<DataService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Project Operations

    /// <inheritdoc/>
    public async Task<IEnumerable<Project>> GetProjectsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all projects");
            
            var projects = await _context.Projects
                .Include(p => p.Tasks)
                .OrderBy(p => p.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {ProjectCount} projects", projects.Count);
            return projects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving active projects");
            
            var projects = await _context.Projects
                .Include(p => p.Tasks)
                .Where(p => !p.IsArchived)
                .OrderBy(p => p.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {ProjectCount} active projects", projects.Count);
            return projects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active projects");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Project?> GetProjectByIdAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Retrieving project with ID {ProjectId}", projectId);
            
            var project = await _context.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.SubTasks)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Dependencies)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                _logger.LogWarning("Project with ID {ProjectId} not found", projectId);
            }
            else
            {
                _logger.LogInformation("Retrieved project '{ProjectName}' with {TaskCount} tasks", 
                    project.Name, project.Tasks.Count);
            }

            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project with ID {ProjectId}", projectId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Project> CreateProjectAsync(Project project)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(project);
            
            _logger.LogInformation("Creating new project '{ProjectName}'", project.Name);
            
            project.CreatedDate = DateTime.Now;
            project.LastModifiedDate = DateTime.Now;
            
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created project with ID {ProjectId}", project.Id);
            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project '{ProjectName}'", project?.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Project> UpdateProjectAsync(Project project)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(project);
            
            _logger.LogInformation("Updating project '{ProjectName}' (ID: {ProjectId})", 
                project.Name, project.Id);
            
            var existingProject = await _context.Projects.FindAsync(project.Id);
            if (existingProject == null)
            {
                throw new InvalidOperationException($"Project with ID {project.Id} not found");
            }

            // Update properties
            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.ProjectManager = project.ProjectManager;
            existingProject.Status = project.Status;
            existingProject.Priority = project.Priority;
            existingProject.Budget = project.Budget;
            existingProject.ActualCost = project.ActualCost;
            existingProject.IsArchived = project.IsArchived;
            existingProject.Color = project.Color;
            existingProject.LastModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated project '{ProjectName}'", project.Name);
            return existingProject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project '{ProjectName}' (ID: {ProjectId})", 
                project?.Name, project?.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteProjectAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Deleting project with ID {ProjectId}", projectId);
            
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                _logger.LogWarning("Project with ID {ProjectId} not found for deletion", projectId);
                return;
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted project '{ProjectName}' (ID: {ProjectId})", 
                project.Name, projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project with ID {ProjectId}", projectId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ArchiveProjectAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Archiving project with ID {ProjectId}", projectId);
            
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found");
            }

            project.IsArchived = true;
            project.LastModifiedDate = DateTime.Now;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Archived project '{ProjectName}' (ID: {ProjectId})", 
                project.Name, projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving project with ID {ProjectId}", projectId);
            throw;
        }
    }

    #endregion

    #region Task Operations

    /// <inheritdoc/>
    public async Task<IEnumerable<GanttTask>> GetTasksByProjectIdAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Retrieving tasks for project ID {ProjectId}", projectId);
            
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.ParentTask)
                .Include(t => t.SubTasks)
                .Include(t => t.Dependencies)
                .Include(t => t.DependentTasks)
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.StartDate)
                .ThenBy(t => t.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {TaskCount} tasks for project ID {ProjectId}", 
                tasks.Count, projectId);
            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for project ID {ProjectId}", projectId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<GanttTask?> GetTaskByIdAsync(int taskId)
    {
        try
        {
            _logger.LogInformation("Retrieving task with ID {TaskId}", taskId);
            
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.ParentTask)
                .Include(t => t.SubTasks)
                .Include(t => t.Dependencies)
                .Include(t => t.DependentTasks)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", taskId);
            }
            else
            {
                _logger.LogInformation("Retrieved task '{TaskName}'", task.Name);
            }

            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<GanttTask> CreateTaskAsync(GanttTask task)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(task);
            
            _logger.LogInformation("Creating new task '{TaskName}' for project ID {ProjectId}", 
                task.Name, task.ProjectId);
            
            task.CreatedDate = DateTime.Now;
            task.LastModifiedDate = DateTime.Now;
            
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created task '{TaskName}' with ID {TaskId}", 
                task.Name, task.Id);
            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task '{TaskName}'", task?.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<GanttTask> UpdateTaskAsync(GanttTask task)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(task);
            
            _logger.LogInformation("Updating task '{TaskName}' (ID: {TaskId})", 
                task.Name, task.Id);
            
            var existingTask = await _context.Tasks.FindAsync(task.Id);
            if (existingTask == null)
            {
                throw new InvalidOperationException($"Task with ID {task.Id} not found");
            }

            // Update properties
            existingTask.Name = task.Name;
            existingTask.Description = task.Description;
            existingTask.StartDate = task.StartDate;
            existingTask.EndDate = task.EndDate;
            existingTask.Progress = task.Progress;
            existingTask.Status = task.Status;
            existingTask.Priority = task.Priority;
            existingTask.Assignee = task.Assignee;
            existingTask.EstimatedHours = task.EstimatedHours;
            existingTask.ActualHours = task.ActualHours;
            existingTask.ParentTaskId = task.ParentTaskId;
            existingTask.LastModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated task '{TaskName}'", task.Name);
            return existingTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task '{TaskName}' (ID: {TaskId})", 
                task?.Name, task?.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteTaskAsync(int taskId)
    {
        try
        {
            _logger.LogInformation("Deleting task with ID {TaskId}", taskId);
            
            var task = await _context.Tasks
                .Include(t => t.SubTasks)
                .Include(t => t.Dependencies)
                .Include(t => t.DependentTasks)
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for deletion", taskId);
                return;
            }

            // Remove dependencies
            task.Dependencies.Clear();
            task.DependentTasks.Clear();

            // Handle subtasks - either delete them or promote them
            foreach (var subTask in task.SubTasks.ToList())
            {
                subTask.ParentTaskId = task.ParentTaskId; // Promote to same level as parent
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted task '{TaskName}' (ID: {TaskId})", 
                task.Name, taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task with ID {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GanttTask>> GetSubTasksAsync(int parentTaskId)
    {
        try
        {
            _logger.LogInformation("Retrieving subtasks for parent task ID {ParentTaskId}", 
                parentTaskId);
            
            var subTasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.ParentTaskId == parentTaskId)
                .OrderBy(t => t.StartDate)
                .ThenBy(t => t.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {SubTaskCount} subtasks for parent task ID {ParentTaskId}", 
                subTasks.Count, parentTaskId);
            return subTasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subtasks for parent task ID {ParentTaskId}", 
                parentTaskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GanttTask>> GetTaskDependenciesAsync(int taskId)
    {
        try
        {
            _logger.LogInformation("Retrieving dependencies for task ID {TaskId}", taskId);
            
            var task = await _context.Tasks
                .Include(t => t.Dependencies)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", taskId);
                return Enumerable.Empty<GanttTask>();
            }

            _logger.LogInformation("Retrieved {DependencyCount} dependencies for task ID {TaskId}", 
                task.Dependencies.Count, taskId);
            return task.Dependencies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dependencies for task ID {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task AddTaskDependencyAsync(int dependentTaskId, int prerequisiteTaskId)
    {
        try
        {
            _logger.LogInformation("Adding dependency: Task {DependentTaskId} depends on Task {PrerequisiteTaskId}", 
                dependentTaskId, prerequisiteTaskId);

            if (dependentTaskId == prerequisiteTaskId)
            {
                throw new InvalidOperationException("A task cannot depend on itself");
            }

            var dependentTask = await _context.Tasks
                .Include(t => t.Dependencies)
                .FirstOrDefaultAsync(t => t.Id == dependentTaskId);
                
            var prerequisiteTask = await _context.Tasks.FindAsync(prerequisiteTaskId);

            if (dependentTask == null)
            {
                throw new InvalidOperationException($"Dependent task with ID {dependentTaskId} not found");
            }

            if (prerequisiteTask == null)
            {
                throw new InvalidOperationException($"Prerequisite task with ID {prerequisiteTaskId} not found");
            }

            // Check if dependency already exists
            if (dependentTask.Dependencies.Any(d => d.Id == prerequisiteTaskId))
            {
                _logger.LogWarning("Dependency already exists between tasks {DependentTaskId} and {PrerequisiteTaskId}", 
                    dependentTaskId, prerequisiteTaskId);
                return;
            }

            // TODO: Add circular dependency check

            dependentTask.Dependencies.Add(prerequisiteTask);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added dependency between tasks {DependentTaskId} and {PrerequisiteTaskId}", 
                dependentTaskId, prerequisiteTaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding dependency between tasks {DependentTaskId} and {PrerequisiteTaskId}", 
                dependentTaskId, prerequisiteTaskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RemoveTaskDependencyAsync(int dependentTaskId, int prerequisiteTaskId)
    {
        try
        {
            _logger.LogInformation("Removing dependency: Task {DependentTaskId} no longer depends on Task {PrerequisiteTaskId}", 
                dependentTaskId, prerequisiteTaskId);

            var dependentTask = await _context.Tasks
                .Include(t => t.Dependencies)
                .FirstOrDefaultAsync(t => t.Id == dependentTaskId);

            if (dependentTask == null)
            {
                _logger.LogWarning("Dependent task with ID {DependentTaskId} not found", dependentTaskId);
                return;
            }

            var prerequisiteTask = dependentTask.Dependencies
                .FirstOrDefault(d => d.Id == prerequisiteTaskId);

            if (prerequisiteTask == null)
            {
                _logger.LogWarning("Dependency not found between tasks {DependentTaskId} and {PrerequisiteTaskId}", 
                    dependentTaskId, prerequisiteTaskId);
                return;
            }

            dependentTask.Dependencies.Remove(prerequisiteTask);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed dependency between tasks {DependentTaskId} and {PrerequisiteTaskId}", 
                dependentTaskId, prerequisiteTaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing dependency between tasks {DependentTaskId} and {PrerequisiteTaskId}", 
                dependentTaskId, prerequisiteTaskId);
            throw;
        }
    }

    #endregion

    #region Filtering and Search Operations

    /// <inheritdoc/>
    public async Task<IEnumerable<GanttTask>> GetTasksByStatusAsync(int projectId, Models.TaskStatus status)
    {
        try
        {
            _logger.LogInformation("Retrieving tasks with status {Status} for project ID {ProjectId}", 
                status, projectId);
            
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.ProjectId == projectId && t.Status == status)
                .OrderBy(t => t.StartDate)
                .ThenBy(t => t.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {TaskCount} tasks with status {Status}", 
                tasks.Count, status);
            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks with status {Status} for project ID {ProjectId}", 
                status, projectId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GanttTask>> GetTasksByAssigneeAsync(int projectId, string assignee)
    {
        try
        {
            _logger.LogInformation("Retrieving tasks assigned to '{Assignee}' for project ID {ProjectId}", 
                assignee, projectId);
            
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.ProjectId == projectId && 
                           !string.IsNullOrEmpty(t.Assignee) && 
                           t.Assignee.Contains(assignee))
                .OrderBy(t => t.StartDate)
                .ThenBy(t => t.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {TaskCount} tasks assigned to '{Assignee}'", 
                tasks.Count, assignee);
            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks assigned to '{Assignee}' for project ID {ProjectId}", 
                assignee, projectId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GanttTask>> GetOverdueTasksAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Retrieving overdue tasks for project ID {ProjectId}", projectId);
            
            var today = DateTime.Today;
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.ProjectId == projectId && 
                           t.EndDate < today && 
                           t.Status != Models.TaskStatus.Completed)
                .OrderBy(t => t.EndDate)
                .ThenBy(t => t.Priority)
                .ToListAsync();

            _logger.LogInformation("Retrieved {TaskCount} overdue tasks", tasks.Count);
            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue tasks for project ID {ProjectId}", projectId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GanttTask>> SearchTasksAsync(int projectId, string searchTerm)
    {
        try
        {
            _logger.LogInformation("Searching tasks with term '{SearchTerm}' for project ID {ProjectId}", 
                searchTerm, projectId);
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetTasksByProjectIdAsync(projectId);
            }

            var lowerSearchTerm = searchTerm.ToLowerInvariant();
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.ProjectId == projectId && 
                           (t.Name.ToLower().Contains(lowerSearchTerm) ||
                            (!string.IsNullOrEmpty(t.Description) && t.Description.ToLower().Contains(lowerSearchTerm)) ||
                            (!string.IsNullOrEmpty(t.Assignee) && t.Assignee.ToLower().Contains(lowerSearchTerm))))
                .OrderBy(t => t.StartDate)
                .ThenBy(t => t.Name)
                .ToListAsync();

            _logger.LogInformation("Found {TaskCount} tasks matching search term '{SearchTerm}'", 
                tasks.Count, searchTerm);
            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tasks with term '{SearchTerm}' for project ID {ProjectId}", 
                searchTerm, projectId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ProjectStatistics> GetProjectStatisticsAsync(int projectId)
    {
        try
        {
            _logger.LogInformation("Calculating statistics for project ID {ProjectId}", projectId);
            
            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            if (!tasks.Any())
            {
                return new ProjectStatistics();
            }

            var statistics = new ProjectStatistics
            {
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
                InProgressTasks = tasks.Count(t => t.Status == TaskStatus.InProgress),
                OverdueTasks = tasks.Count(t => t.EndDate < DateTime.Today && t.Status != TaskStatus.Completed),
                TotalEstimatedHours = tasks.Sum(t => t.EstimatedHours),
                TotalActualHours = tasks.Sum(t => t.ActualHours),
                EarliestStartDate = tasks.Min(t => t.StartDate),
                LatestEndDate = tasks.Max(t => t.EndDate)
            };

            statistics.ProgressPercentage = statistics.TotalTasks > 0 
                ? (double)statistics.CompletedTasks / statistics.TotalTasks * 100 
                : 0;

            _logger.LogInformation("Calculated statistics for project ID {ProjectId}: {TotalTasks} total, {CompletedTasks} completed, {ProgressPercentage:F1}% progress", 
                projectId, statistics.TotalTasks, statistics.CompletedTasks, statistics.ProgressPercentage);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating statistics for project ID {ProjectId}", projectId);
            throw;
        }
    }

    #endregion
}