namespace TouchGanttChart.Models;

/// <summary>
/// Represents the priority level of a Gantt task.
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Low priority task.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority task (default).
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority task.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority task requiring immediate attention.
    /// </summary>
    Critical = 3
}