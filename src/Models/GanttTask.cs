using System.ComponentModel.DataAnnotations;

namespace TouchGanttChart.Models;

/// <summary>
/// Represents a task in the Gantt chart with touch-optimized properties.
/// </summary>
public class GanttTask
{
    /// <summary>
    /// Gets or sets the unique identifier for the task.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the task name.
    /// </summary>
    [Required(ErrorMessage = "Task name is required")]
    [StringLength(200, ErrorMessage = "Task name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the task.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task start date.
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the task end date.
    /// </summary>
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    /// <summary>
    /// Gets or sets the completion progress as a percentage (0-100).
    /// </summary>
    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
    public int Progress { get; set; }

    /// <summary>
    /// Gets or sets the current status of the task.
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

    /// <summary>
    /// Gets or sets the priority level of the task.
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;

    /// <summary>
    /// Gets or sets the assigned team member or resource.
    /// </summary>
    [StringLength(100, ErrorMessage = "Assignee name cannot exceed 100 characters")]
    public string Assignee { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated effort in hours.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
    public double EstimatedHours { get; set; }

    /// <summary>
    /// Gets or sets the actual effort spent in hours.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Actual hours must be non-negative")]
    public double ActualHours { get; set; }

    // Navigation properties for hierarchical tasks
    /// <summary>
    /// Gets or sets the parent task ID for subtasks.
    /// </summary>
    public int? ParentTaskId { get; set; }

    /// <summary>
    /// Gets or sets the parent task reference.
    /// </summary>
    public GanttTask? ParentTask { get; set; }

    /// <summary>
    /// Gets or sets the collection of subtasks.
    /// </summary>
    public List<GanttTask> SubTasks { get; set; } = new();

    /// <summary>
    /// Gets or sets the project ID this task belongs to.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project reference.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of tasks that depend on this task.
    /// </summary>
    public List<GanttTask> DependentTasks { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of tasks this task depends on.
    /// </summary>
    public List<GanttTask> Dependencies { get; set; } = new();

    // Touch-friendly UI properties
    /// <summary>
    /// Gets or sets whether the task is currently selected in the UI.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets whether the task hierarchy is expanded in the UI.
    /// </summary>
    public bool IsExpanded { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.Now;

    // Calculated properties
    /// <summary>
    /// Gets the duration of the task.
    /// </summary>
    public TimeSpan Duration => EndDate - StartDate;

    /// <summary>
    /// Gets whether the task is overdue.
    /// </summary>
    public bool IsOverdue => EndDate < DateTime.Today && Status != TaskStatus.Completed;

    /// <summary>
    /// Gets whether the task is a milestone (duration of 0).
    /// </summary>
    public bool IsMilestone => Duration.TotalDays <= 0;

    /// <summary>
    /// Gets whether the task has subtasks.
    /// </summary>
    public bool HasSubTasks => SubTasks.Count > 0;

    /// <summary>
    /// Gets the completion percentage for display.
    /// </summary>
    public string ProgressDisplay => $"{Progress}%";

    /// <summary>
    /// Gets the task duration in days for display.
    /// </summary>
    public string DurationDisplay
    {
        get
        {
            var days = Duration.TotalDays;
            return days switch
            {
                <= 0 => "Milestone",
                1 => "1 day",
                _ => $"{days:F1} days"
            };
        }
    }

    /// <summary>
    /// Gets a value indicating whether the priority should be visible (non-Normal priority).
    /// </summary>
    public bool IsPriorityVisible => Priority != TaskPriority.Normal;

    /// <summary>
    /// Gets a value indicating whether the task has an assignee.
    /// </summary>
    public bool HasAssignee => !string.IsNullOrWhiteSpace(Assignee);

    /// <summary>
    /// Gets a value indicating whether the task has progress greater than 0.
    /// </summary>
    public bool HasProgress => Progress > 0;
}