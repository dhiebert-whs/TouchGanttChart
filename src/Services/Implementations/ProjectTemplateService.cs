using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TouchGanttChart.Data;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;

namespace TouchGanttChart.Services.Implementations;

/// <summary>
/// Service for managing project templates with database operations.
/// </summary>
public class ProjectTemplateService : IProjectTemplateService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectTemplateService> _logger;

    public ProjectTemplateService(AppDbContext context, ILogger<ProjectTemplateService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProjectTemplate>> GetTemplatesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all active project templates");
            
            var templates = await _context.ProjectTemplates
                .Where(pt => pt.IsActive)
                .Include(pt => pt.TaskTemplates)
                .OrderBy(pt => pt.Category)
                .ThenBy(pt => pt.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {TemplateCount} project templates", templates.Count);
            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project templates");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProjectTemplate>> GetTemplatesByCategoryAsync(string category)
    {
        try
        {
            _logger.LogInformation("Retrieving project templates for category: {Category}", category);
            
            var templates = await _context.ProjectTemplates
                .Where(pt => pt.IsActive && pt.Category == category)
                .Include(pt => pt.TaskTemplates)
                .OrderBy(pt => pt.Name)
                .ToListAsync();

            _logger.LogInformation("Retrieved {TemplateCount} templates for category {Category}", templates.Count, category);
            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates for category: {Category}", category);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving project template categories");
            
            var categories = await _context.ProjectTemplates
                .Where(pt => pt.IsActive)
                .Select(pt => pt.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            _logger.LogInformation("Retrieved {CategoryCount} template categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template categories");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ProjectTemplate?> GetTemplateByIdAsync(int templateId)
    {
        try
        {
            _logger.LogInformation("Retrieving project template with ID: {TemplateId}", templateId);
            
            var template = await _context.ProjectTemplates
                .Include(pt => pt.TaskTemplates.OrderBy(tt => tt.Order))
                    .ThenInclude(tt => tt.Dependencies)
                        .ThenInclude(d => d.PrerequisiteTaskTemplate)
                .Include(pt => pt.TaskTemplates)
                    .ThenInclude(tt => tt.ChildTaskTemplates)
                .FirstOrDefaultAsync(pt => pt.Id == templateId && pt.IsActive);

            if (template != null)
            {
                _logger.LogInformation("Found project template: {TemplateName}", template.Name);
            }
            else
            {
                _logger.LogWarning("Project template with ID {TemplateId} not found", templateId);
            }

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project template with ID: {TemplateId}", templateId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Project> CreateProjectFromTemplateAsync(int templateId, string projectName, string projectManager, DateTime startDate)
    {
        try
        {
            _logger.LogInformation("Creating project from template ID: {TemplateId}", templateId);
            
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null)
            {
                throw new ArgumentException($"Template with ID {templateId} not found", nameof(templateId));
            }

            // Create the new project
            var project = new Project
            {
                Name = projectName,
                Description = template.Description,
                ProjectManager = projectManager,
                StartDate = startDate,
                EndDate = startDate.AddDays(template.EstimatedDurationDays),
                Status = Models.TaskStatus.NotStarted,
                Priority = Models.TaskPriority.Normal,
                Budget = template.EstimatedBudget,
                Color = "#3498db",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Create tasks from task templates
            await CreateTasksFromTemplatesAsync(project, template.TaskTemplates, startDate);

            // Increment template usage count
            await IncrementUsageCountAsync(templateId);

            _logger.LogInformation("Successfully created project '{ProjectName}' from template '{TemplateName}'", 
                projectName, template.Name);

            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project from template ID: {TemplateId}", templateId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ProjectTemplate> CreateTemplateAsync(ProjectTemplate template)
    {
        try
        {
            _logger.LogInformation("Creating new project template: {TemplateName}", template.Name);
            
            template.CreatedDate = DateTime.UtcNow;
            template.LastModifiedDate = DateTime.UtcNow;
            
            _context.ProjectTemplates.Add(template);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created project template: {TemplateName} with ID: {TemplateId}", 
                template.Name, template.Id);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project template: {TemplateName}", template.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ProjectTemplate> UpdateTemplateAsync(ProjectTemplate template)
    {
        try
        {
            _logger.LogInformation("Updating project template: {TemplateId}", template.Id);
            
            template.LastModifiedDate = DateTime.UtcNow;
            
            _context.ProjectTemplates.Update(template);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated project template: {TemplateId}", template.Id);
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project template: {TemplateId}", template.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteTemplateAsync(int templateId)
    {
        try
        {
            _logger.LogInformation("Deleting project template: {TemplateId}", templateId);
            
            var template = await _context.ProjectTemplates.FindAsync(templateId);
            if (template == null)
            {
                _logger.LogWarning("Project template {TemplateId} not found for deletion", templateId);
                return false;
            }

            // Don't delete built-in templates, just mark as inactive
            if (template.IsBuiltIn)
            {
                template.IsActive = false;
                template.LastModifiedDate = DateTime.UtcNow;
                _context.ProjectTemplates.Update(template);
            }
            else
            {
                _context.ProjectTemplates.Remove(template);
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Successfully deleted/deactivated project template: {TemplateId}", templateId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project template: {TemplateId}", templateId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ProjectTemplate> CreateTemplateFromProjectAsync(int projectId, string templateName, string category, string description)
    {
        try
        {
            _logger.LogInformation("Creating template from project ID: {ProjectId}", projectId);
            
            var project = await _context.Projects
                .Include(p => p.Tasks.OrderBy(t => t.StartDate))
                    .ThenInclude(t => t.Dependencies)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                throw new ArgumentException($"Project with ID {projectId} not found", nameof(projectId));
            }

            // Calculate duration
            var duration = project.Tasks.Any() 
                ? (int)(project.Tasks.Max(t => t.EndDate) - project.Tasks.Min(t => t.StartDate)).TotalDays
                : 0;

            // Create the template
            var template = new ProjectTemplate
            {
                Name = templateName,
                Description = description,
                Category = category,
                EstimatedDurationDays = duration,
                EstimatedBudget = project.Budget,
                IsBuiltIn = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.ProjectTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Create task templates from tasks
            await CreateTaskTemplatesFromTasksAsync(template, project.Tasks, project.StartDate);

            _logger.LogInformation("Successfully created template '{TemplateName}' from project '{ProjectName}'", 
                templateName, project.Name);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template from project ID: {ProjectId}", projectId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProjectTemplate>> GetPopularTemplatesAsync(int count = 5)
    {
        try
        {
            _logger.LogInformation("Retrieving {Count} most popular templates", count);
            
            var templates = await _context.ProjectTemplates
                .Where(pt => pt.IsActive)
                .Include(pt => pt.TaskTemplates)
                .OrderByDescending(pt => pt.UsageCount)
                .ThenBy(pt => pt.Name)
                .Take(count)
                .ToListAsync();

            _logger.LogInformation("Retrieved {TemplateCount} popular templates", templates.Count);
            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular templates");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProjectTemplate>> SearchTemplatesAsync(string searchTerm)
    {
        try
        {
            _logger.LogInformation("Searching templates with term: {SearchTerm}", searchTerm);
            
            var templates = await _context.ProjectTemplates
                .Where(pt => pt.IsActive && 
                    (pt.Name.Contains(searchTerm) || pt.Description.Contains(searchTerm)))
                .Include(pt => pt.TaskTemplates)
                .OrderBy(pt => pt.Category)
                .ThenBy(pt => pt.Name)
                .ToListAsync();

            _logger.LogInformation("Found {TemplateCount} templates matching search term", templates.Count);
            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching templates with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task IncrementUsageCountAsync(int templateId)
    {
        try
        {
            var template = await _context.ProjectTemplates.FindAsync(templateId);
            if (template != null)
            {
                template.UsageCount++;
                template.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Incremented usage count for template: {TemplateId}", templateId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing usage count for template: {TemplateId}", templateId);
            // Don't throw, as this is not critical
        }
    }

    #region Private Helper Methods

    private async Task CreateTasksFromTemplatesAsync(Project project, ICollection<TaskTemplate> taskTemplates, DateTime projectStartDate)
    {
        var taskMapping = new Dictionary<int, GanttTask>();
        var rootTasks = taskTemplates.Where(tt => tt.ParentTaskTemplateId == null).OrderBy(tt => tt.Order).ToList();

        // Create tasks in hierarchical order
        await CreateTasksRecursively(project, rootTasks, taskMapping, projectStartDate, null);

        // Create dependencies after all tasks are created
        await CreateTaskDependenciesAsync(taskTemplates, taskMapping);
    }

    private async Task CreateTasksRecursively(Project project, IEnumerable<TaskTemplate> taskTemplates, 
        Dictionary<int, GanttTask> taskMapping, DateTime projectStartDate, int? parentTaskId)
    {
        foreach (var taskTemplate in taskTemplates.OrderBy(tt => tt.Order))
        {
            var task = new GanttTask
            {
                Name = taskTemplate.Name,
                Description = taskTemplate.Description,
                StartDate = projectStartDate.AddDays(taskTemplate.StartOffsetDays),
                EndDate = projectStartDate.AddDays(taskTemplate.StartOffsetDays + taskTemplate.EstimatedDurationDays),
                Status = Models.TaskStatus.NotStarted,
                Priority = taskTemplate.Priority,
                Progress = 0,
                ProjectId = project.Id,
                ParentTaskId = parentTaskId,
                Assignee = taskTemplate.DefaultAssigneeRole,
                EstimatedHours = (double)taskTemplate.EstimatedHours,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            taskMapping[taskTemplate.Id] = task;

            // Create child tasks recursively
            var childTemplates = taskTemplate.ChildTaskTemplates.OrderBy(ct => ct.Order);
            if (childTemplates.Any())
            {
                await CreateTasksRecursively(project, childTemplates, taskMapping, projectStartDate, task.Id);
            }
        }
    }

    private async Task CreateTaskDependenciesAsync(ICollection<TaskTemplate> taskTemplates, Dictionary<int, GanttTask> taskMapping)
    {
        foreach (var taskTemplate in taskTemplates)
        {
            if (taskTemplate.Dependencies.Any() && taskMapping.ContainsKey(taskTemplate.Id))
            {
                var dependentTask = taskMapping[taskTemplate.Id];

                foreach (var dependency in taskTemplate.Dependencies)
                {
                    if (taskMapping.ContainsKey(dependency.PrerequisiteTaskTemplateId))
                    {
                        var prerequisiteTask = taskMapping[dependency.PrerequisiteTaskTemplateId];
                        dependentTask.Dependencies.Add(prerequisiteTask);
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task CreateTaskTemplatesFromTasksAsync(ProjectTemplate template, ICollection<GanttTask> tasks, DateTime projectStartDate)
    {
        var taskMapping = new Dictionary<int, TaskTemplate>();
        var order = 1;

        // Create task templates
        foreach (var task in tasks.OrderBy(t => t.StartDate))
        {
            var startOffset = (int)(task.StartDate - projectStartDate).TotalDays;
            var duration = (int)(task.EndDate - task.StartDate).TotalDays;

            var taskTemplate = new TaskTemplate
            {
                ProjectTemplateId = template.Id,
                Name = task.Name,
                Description = task.Description,
                EstimatedDurationDays = duration,
                EstimatedHours = (decimal)task.EstimatedHours,
                Priority = task.Priority,
                Order = order++,
                DefaultAssigneeRole = task.Assignee,
                StartOffsetDays = startOffset,
                IsMilestone = duration == 0,
                CreatedDate = DateTime.UtcNow
            };

            _context.TaskTemplates.Add(taskTemplate);
            await _context.SaveChangesAsync();

            taskMapping[task.Id] = taskTemplate;
        }

        // Create dependencies
        foreach (var task in tasks)
        {
            if (task.Dependencies.Any() && taskMapping.ContainsKey(task.Id))
            {
                var taskTemplate = taskMapping[task.Id];

                foreach (var dependency in task.Dependencies)
                {
                    if (taskMapping.ContainsKey(dependency.Id))
                    {
                        var dependencyTemplate = new TaskTemplateDependency
                        {
                            DependentTaskTemplateId = taskTemplate.Id,
                            PrerequisiteTaskTemplateId = taskMapping[dependency.Id].Id,
                            DependencyType = DependencyType.FinishToStart,
                            LagDays = 0
                        };

                        _context.Set<TaskTemplateDependency>().Add(dependencyTemplate);
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion
}