namespace TouchGanttChart.Models;

/// <summary>
/// Represents the current status of a Gantt task.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task has not been started yet.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Task is currently being worked on.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task has been completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Task is temporarily on hold.
    /// </summary>
    OnHold = 3,

    /// <summary>
    /// Task has been cancelled and will not be completed.
    /// </summary>
    Cancelled = 4
}