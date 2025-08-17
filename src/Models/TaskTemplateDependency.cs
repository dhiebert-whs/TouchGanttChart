namespace TouchGanttChart.Models;

/// <summary>
/// Represents a dependency relationship between task templates.
/// </summary>
public class TaskTemplateDependency
{
    /// <summary>
    /// Gets or sets the dependent task template ID (the task that depends on another).
    /// </summary>
    public int DependentTaskTemplateId { get; set; }

    /// <summary>
    /// Gets or sets the prerequisite task template ID (the task that must be completed first).
    /// </summary>
    public int PrerequisiteTaskTemplateId { get; set; }

    /// <summary>
    /// Gets or sets the dependency type.
    /// </summary>
    public DependencyType DependencyType { get; set; } = DependencyType.FinishToStart;

    /// <summary>
    /// Gets or sets the lag time in days (can be negative for lead time).
    /// </summary>
    public int LagDays { get; set; }

    /// <summary>
    /// Navigation properties
    /// </summary>
    public TaskTemplate DependentTaskTemplate { get; set; } = null!;
    public TaskTemplate PrerequisiteTaskTemplate { get; set; } = null!;
}

/// <summary>
/// Defines the types of dependencies between tasks.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// Finish-to-Start: The prerequisite task must finish before the dependent task can start.
    /// </summary>
    FinishToStart,

    /// <summary>
    /// Start-to-Start: Both tasks must start at the same time.
    /// </summary>
    StartToStart,

    /// <summary>
    /// Finish-to-Finish: Both tasks must finish at the same time.
    /// </summary>
    FinishToFinish,

    /// <summary>
    /// Start-to-Finish: The dependent task cannot finish until the prerequisite task starts.
    /// </summary>
    StartToFinish
}