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
    /// Gets or sets the task category (e.g., Mechanical, Electrical, Software, etc.).
    /// </summary>
    [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
    public string Category { get; set; } = "General";

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

    /// <summary>
    /// Gets or sets the actual completion date (when task was completed).
    /// </summary>
    public DateTime? CompletionDate { get; set; }

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

    /// <summary>
    /// Gets a value indicating whether the task is completed ahead of schedule.
    /// </summary>
    public bool IsCompletedEarly => CompletionDate.HasValue && CompletionDate.Value < EndDate;

    /// <summary>
    /// Gets the number of days the task was completed early (positive) or late (negative).
    /// </summary>
    public double CompletionVarianceDays
    {
        get
        {
            if (!CompletionDate.HasValue) return 0;
            return (EndDate - CompletionDate.Value).TotalDays;
        }
    }

    /// <summary>
    /// Gets whether this task is a leaf task (has no subtasks).
    /// Leaf tasks can have manually editable progress.
    /// </summary>
    public bool IsLeafTask => SubTasks == null || SubTasks.Count == 0;

    /// <summary>
    /// Gets whether this task is a parent task (has subtasks).
    /// Parent tasks have auto-calculated progress.
    /// </summary>
    public bool IsParentTask => SubTasks?.Count > 0;

    /// <summary>
    /// Gets the hierarchical level/depth of this task (0 for root tasks).
    /// </summary>
    public int HierarchyLevel
    {
        get
        {
            var level = 0;
            var current = ParentTask;
            while (current != null)
            {
                level++;
                current = current.ParentTask;
            }
            return level;
        }
    }

    /// <summary>
    /// Gets the calculated progress for parent tasks based on subtask completion.
    /// For leaf tasks, returns the manually set progress.
    /// </summary>
    public int CalculatedProgress
    {
        get
        {
            if (IsLeafTask)
                return Progress;

            if (SubTasks == null || SubTasks.Count == 0)
                return Progress;

            // Calculate weighted average based on estimated hours
            var totalWeight = SubTasks.Sum(t => Math.Max(t.EstimatedHours, 1.0)); // Minimum weight of 1
            if (totalWeight == 0) return 0;

            var weightedProgress = SubTasks.Sum(t => t.CalculatedProgress * Math.Max(t.EstimatedHours, 1.0));
            return (int)Math.Round(weightedProgress / totalWeight);
        }
    }

    /// <summary>
    /// Gets all descendant tasks (recursive)
    /// </summary>
    public IEnumerable<GanttTask> GetAllDescendants()
    {
        if (SubTasks == null) yield break;

        foreach (var child in SubTasks)
        {
            yield return child;
            foreach (var grandChild in child.GetAllDescendants())
            {
                yield return grandChild;
            }
        }
    }

    /// <summary>
    /// Gets all ancestor tasks (recursive up to root)
    /// </summary>
    public IEnumerable<GanttTask> GetAllAncestors()
    {
        var current = ParentTask;
        while (current != null)
        {
            yield return current;
            current = current.ParentTask;
        }
    }

    /// <summary>
    /// Gets the root task of this hierarchy
    /// </summary>
    public GanttTask RootTask
    {
        get
        {
            var current = this;
            while (current.ParentTask != null)
            {
                current = current.ParentTask;
            }
            return current;
        }
    }

    /// <summary>
    /// Gets tasks that this task depends on (prerequisites)
    /// </summary>
    public List<GanttTask> GetPrerequisites()
    {
        return Dependencies?.ToList() ?? new List<GanttTask>();
    }

    /// <summary>
    /// Gets tasks that depend on this task (successors)
    /// </summary>
    public List<GanttTask> GetSuccessors()
    {
        return DependentTasks?.ToList() ?? new List<GanttTask>();
    }

    /// <summary>
    /// Gets the status button text for the day view
    /// </summary>
    public string StatusButtonText => Status switch
    {
        TaskStatus.NotStarted => "Start",
        TaskStatus.InProgress => "In Progress",
        TaskStatus.Completed => "✓ Done",
        TaskStatus.OnHold => "On Hold",
        TaskStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the display text for hierarchy indentation
    /// </summary>
    public string HierarchyIndent => new string(' ', HierarchyLevel * 4);

    /// <summary>
    /// Gets the display name with hierarchy prefix
    /// </summary>
    public string HierarchicalName => $"{HierarchyIndent}{Name}";

    /// <summary>
    /// Updates the progress of this task and all ancestor tasks
    /// </summary>
    public void UpdateHierarchicalProgress()
    {
        // Update all ancestor tasks' progress
        var current = ParentTask;
        while (current != null)
        {
            // Parent task progress is automatically calculated
            // Note: Property change notification would be handled by the UI layer
            current = current.ParentTask;
        }
    }
}