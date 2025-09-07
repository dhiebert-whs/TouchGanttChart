namespace TouchGanttChart.Models;

/// <summary>
/// Event arguments for when task dates are changed through dragging
/// </summary>
public class TaskDatesChangedEventArgs : EventArgs
{
    public GanttTask Task { get; }
    public DateTime NewStartDate { get; }
    public DateTime NewEndDate { get; }

    public TaskDatesChangedEventArgs(GanttTask task, DateTime newStartDate, DateTime newEndDate)
    {
        Task = task;
        NewStartDate = newStartDate;
        NewEndDate = newEndDate;
    }
}