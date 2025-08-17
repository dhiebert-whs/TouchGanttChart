using TouchGanttChart.Models;

namespace TouchGanttChart.Services.Interfaces;

/// <summary>
/// Service interface for managing project templates.
/// </summary>
public interface IProjectTemplateService
{
    /// <summary>
    /// Gets all active project templates.
    /// </summary>
    /// <returns>A collection of project templates.</returns>
    Task<IEnumerable<ProjectTemplate>> GetTemplatesAsync();

    /// <summary>
    /// Gets project templates by category.
    /// </summary>
    /// <param name="category">The template category.</param>
    /// <returns>A collection of project templates in the specified category.</returns>
    Task<IEnumerable<ProjectTemplate>> GetTemplatesByCategoryAsync(string category);

    /// <summary>
    /// Gets all template categories.
    /// </summary>
    /// <returns>A collection of category names.</returns>
    Task<IEnumerable<string>> GetCategoriesAsync();

    /// <summary>
    /// Gets a specific project template by ID with all task templates.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <returns>The project template with task templates, or null if not found.</returns>
    Task<ProjectTemplate?> GetTemplateByIdAsync(int templateId);

    /// <summary>
    /// Creates a new project from a template.
    /// </summary>
    /// <param name="templateId">The template ID to use.</param>
    /// <param name="projectName">The name for the new project.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="startDate">The project start date.</param>
    /// <returns>The created project.</returns>
    Task<Project> CreateProjectFromTemplateAsync(int templateId, string projectName, string projectManager, DateTime startDate);

    /// <summary>
    /// Creates a new project template.
    /// </summary>
    /// <param name="template">The project template to create.</param>
    /// <returns>The created project template.</returns>
    Task<ProjectTemplate> CreateTemplateAsync(ProjectTemplate template);

    /// <summary>
    /// Updates an existing project template.
    /// </summary>
    /// <param name="template">The project template to update.</param>
    /// <returns>The updated project template.</returns>
    Task<ProjectTemplate> UpdateTemplateAsync(ProjectTemplate template);

    /// <summary>
    /// Deletes a project template.
    /// </summary>
    /// <param name="templateId">The template ID to delete.</param>
    /// <returns>True if the template was deleted successfully.</returns>
    Task<bool> DeleteTemplateAsync(int templateId);

    /// <summary>
    /// Creates a template from an existing project.
    /// </summary>
    /// <param name="projectId">The project ID to create a template from.</param>
    /// <param name="templateName">The name for the new template.</param>
    /// <param name="category">The template category.</param>
    /// <param name="description">The template description.</param>
    /// <returns>The created project template.</returns>
    Task<ProjectTemplate> CreateTemplateFromProjectAsync(int projectId, string templateName, string category, string description);

    /// <summary>
    /// Gets the most popular templates based on usage count.
    /// </summary>
    /// <param name="count">The number of templates to return.</param>
    /// <returns>A collection of the most popular templates.</returns>
    Task<IEnumerable<ProjectTemplate>> GetPopularTemplatesAsync(int count = 5);

    /// <summary>
    /// Searches for templates by name or description.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <returns>A collection of matching templates.</returns>
    Task<IEnumerable<ProjectTemplate>> SearchTemplatesAsync(string searchTerm);

    /// <summary>
    /// Increments the usage count for a template.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task IncrementUsageCountAsync(int templateId);
}