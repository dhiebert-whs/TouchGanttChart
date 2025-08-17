using System.ComponentModel.DataAnnotations;

namespace TouchGanttChart.Models;

/// <summary>
/// Represents a project template that can be used to create new projects
/// with predefined structure and task hierarchy.
/// </summary>
public class ProjectTemplate
{
    /// <summary>
    /// Gets or sets the unique identifier for the template.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template description.
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template category.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated duration in days.
    /// </summary>
    public int EstimatedDurationDays { get; set; }

    /// <summary>
    /// Gets or sets the estimated budget.
    /// </summary>
    public decimal EstimatedBudget { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a built-in template.
    /// </summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this template is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the template icon or identifier.
    /// </summary>
    [StringLength(50)]
    public string Icon { get; set; } = "📋";

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modified date.
    /// </summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of task templates.
    /// </summary>
    public ICollection<TaskTemplate> TaskTemplates { get; set; } = new List<TaskTemplate>();

    /// <summary>
    /// Gets the usage count for this template.
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Gets a display-friendly duration description.
    /// </summary>
    public string DurationDisplay
    {
        get
        {
            if (EstimatedDurationDays == 0) return "Not specified";
            if (EstimatedDurationDays == 1) return "1 day";
            if (EstimatedDurationDays < 7) return $"{EstimatedDurationDays} days";
            if (EstimatedDurationDays < 30) return $"{EstimatedDurationDays / 7} weeks";
            return $"{EstimatedDurationDays / 30} months";
        }
    }

    /// <summary>
    /// Gets a display-friendly budget description.
    /// </summary>
    public string BudgetDisplay => EstimatedBudget > 0 ? $"${EstimatedBudget:N0}" : "Not specified";

    /// <summary>
    /// Gets the number of tasks in this template.
    /// </summary>
    public int TaskCount => TaskTemplates?.Count ?? 0;
}