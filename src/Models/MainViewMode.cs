namespace TouchGanttChart.Models;

/// <summary>
/// Defines the main view modes for the application.
/// </summary>
public enum MainViewMode
{
    /// <summary>
    /// Full Gantt chart view with project timeline.
    /// </summary>
    GanttView,

    /// <summary>
    /// Daily todo list view showing tasks for a specific day.
    /// </summary>
    DailyTodo,

    /// <summary>
    /// Daily Gantt view showing a single day's tasks in Gantt format.
    /// </summary>
    DailyGantt
}

/// <summary>
/// Provides configuration and utilities for different main view modes.
/// </summary>
public static class MainViewConfig
{
    /// <summary>
    /// Gets the display name for a main view mode.
    /// </summary>
    /// <param name="mode">The main view mode.</param>
    /// <returns>The display name.</returns>
    public static string GetDisplayName(MainViewMode mode)
    {
        return mode switch
        {
            MainViewMode.GanttView => "Gantt Chart",
            MainViewMode.DailyTodo => "Daily Todo",
            MainViewMode.DailyGantt => "Daily Gantt",
            _ => "Unknown View"
        };
    }

    /// <summary>
    /// Gets the icon for a main view mode.
    /// </summary>
    /// <param name="mode">The main view mode.</param>
    /// <returns>The icon character.</returns>
    public static string GetIcon(MainViewMode mode)
    {
        return mode switch
        {
            MainViewMode.GanttView => "📊",
            MainViewMode.DailyTodo => "📅",
            MainViewMode.DailyGantt => "📆",
            _ => "📋"
        };
    }

    /// <summary>
    /// Gets all available main view modes.
    /// </summary>
    /// <returns>An array of all main view modes.</returns>
    public static MainViewMode[] GetAllModes()
    {
        return Enum.GetValues<MainViewMode>();
    }
}