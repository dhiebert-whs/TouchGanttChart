using System.ComponentModel.DataAnnotations;

namespace TouchGanttChart.Models;

/// <summary>
/// Represents a project containing multiple Gantt tasks.
/// </summary>
public class Project
{
    /// <summary>
    /// Gets or sets the unique identifier for the project.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the project.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project start date.
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the project end date.
    /// </summary>
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

    /// <summary>
    /// Gets or sets the project manager or owner.
    /// </summary>
    [StringLength(100, ErrorMessage = "Project manager name cannot exceed 100 characters")]
    public string ProjectManager { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project status.
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

    /// <summary>
    /// Gets or sets the project priority.
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;

    /// <summary>
    /// Gets or sets the estimated budget for the project.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Budget must be non-negative")]
    public decimal Budget { get; set; }

    /// <summary>
    /// Gets or sets the actual cost of the project.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Actual cost must be non-negative")]
    public decimal ActualCost { get; set; }

    /// <summary>
    /// Gets or sets when the project was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets when the project was last modified.
    /// </summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets whether the project is archived.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets the color theme for the project (hex color code).
    /// </summary>
    [StringLength(7, ErrorMessage = "Color must be a valid hex code")]
    public string Color { get; set; } = "#3498db";

    // Navigation properties
    /// <summary>
    /// Gets or sets the collection of tasks in this project.
    /// </summary>
    public List<GanttTask> Tasks { get; set; } = new();

    // Calculated properties
    /// <summary>
    /// Gets the total number of tasks in the project.
    /// </summary>
    public int TaskCount => Tasks.Count;

    /// <summary>
    /// Gets the number of completed tasks.
    /// </summary>
    public int CompletedTaskCount => Tasks.Count(t => t.Status == TaskStatus.Completed);

    /// <summary>
    /// Gets the number of tasks in progress.
    /// </summary>
    public int InProgressTaskCount => Tasks.Count(t => t.Status == TaskStatus.InProgress);

    /// <summary>
    /// Gets the number of overdue tasks.
    /// </summary>
    public int OverdueTaskCount => Tasks.Count(t => t.IsOverdue);

    /// <summary>
    /// Gets the overall progress percentage of the project.
    /// </summary>
    public double ProgressPercentage => TaskCount > 0 ? (double)CompletedTaskCount / TaskCount * 100 : 0;

    /// <summary>
    /// Gets the project duration.
    /// </summary>
    public TimeSpan Duration => EndDate - StartDate;

    /// <summary>
    /// Gets whether the project is overdue.
    /// </summary>
    public bool IsOverdue => EndDate < DateTime.Today && Status != TaskStatus.Completed;

    /// <summary>
    /// Gets the total estimated hours for all tasks.
    /// </summary>
    public double TotalEstimatedHours => Tasks.Sum(t => t.EstimatedHours);

    /// <summary>
    /// Gets the total actual hours spent on all tasks.
    /// </summary>
    public double TotalActualHours => Tasks.Sum(t => t.ActualHours);

    /// <summary>
    /// Gets the budget utilization percentage.
    /// </summary>
    public double BudgetUtilization => Budget > 0 ? (double)(ActualCost / Budget) * 100 : 0;

    /// <summary>
    /// Gets the project health status based on progress and timeline.
    /// </summary>
    public string HealthStatus
    {
        get
        {
            if (Status == TaskStatus.Completed) return "Completed";
            if (IsOverdue) return "Overdue";
            if (ProgressPercentage >= 75) return "On Track";
            if (ProgressPercentage >= 50) return "At Risk";
            return "Behind Schedule";
        }
    }

    /// <summary>
    /// Gets a display-friendly duration string.
    /// </summary>
    public string DurationDisplay
    {
        get
        {
            var days = Duration.TotalDays;
            return days switch
            {
                < 1 => "Less than 1 day",
                1 => "1 day",
                < 30 => $"{days:F0} days",
                < 365 => $"{days / 30:F1} months",
                _ => $"{days / 365:F1} years"
            };
        }
    }

    /// <summary>
    /// Gets the progress display string.
    /// </summary>
    public string ProgressDisplay => $"{ProgressPercentage:F1}%";
}