using System.ComponentModel.DataAnnotations;

namespace TouchGanttChart.Models;

/// <summary>
/// Represents a task template within a project template.
/// </summary>
public class TaskTemplate
{
    /// <summary>
    /// Gets or sets the unique identifier for the task template.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the project template ID this task belongs to.
    /// </summary>
    public int ProjectTemplateId { get; set; }

    /// <summary>
    /// Gets or sets the task name.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task description.
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated duration in days.
    /// </summary>
    public int EstimatedDurationDays { get; set; }

    /// <summary>
    /// Gets or sets the estimated hours for the task.
    /// </summary>
    public decimal EstimatedHours { get; set; }

    /// <summary>
    /// Gets or sets the task priority.
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;

    /// <summary>
    /// Gets or sets the order of this task within the template.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the parent task template ID for hierarchical tasks.
    /// </summary>
    public int? ParentTaskTemplateId { get; set; }

    /// <summary>
    /// Gets or sets the default assignee role or title.
    /// </summary>
    [StringLength(100)]
    public string DefaultAssigneeRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start offset in days from project start.
    /// </summary>
    public int StartOffsetDays { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this task is a milestone.
    /// </summary>
    public bool IsMilestone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this task is on the critical path.
    /// </summary>
    public bool IsCriticalPath { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation properties
    /// </summary>
    public ProjectTemplate ProjectTemplate { get; set; } = null!;
    public TaskTemplate? ParentTaskTemplate { get; set; }
    public ICollection<TaskTemplate> ChildTaskTemplates { get; set; } = new List<TaskTemplate>();
    public ICollection<TaskTemplateDependency> Dependencies { get; set; } = new List<TaskTemplateDependency>();
    public ICollection<TaskTemplateDependency> Dependents { get; set; } = new List<TaskTemplateDependency>();

    /// <summary>
    /// Gets a display-friendly duration description.
    /// </summary>
    public string DurationDisplay
    {
        get
        {
            if (IsMilestone) return "Milestone";
            if (EstimatedDurationDays == 0) return "Not specified";
            if (EstimatedDurationDays == 1) return "1 day";
            return $"{EstimatedDurationDays} days";
        }
    }

    /// <summary>
    /// Gets a display-friendly hours description.
    /// </summary>
    public string HoursDisplay => EstimatedHours > 0 ? $"{EstimatedHours:F1}h" : "Not specified";
}