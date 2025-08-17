namespace TouchGanttChart.Models;

/// <summary>
/// Defines the different timeline view modes for the Gantt chart.
/// </summary>
public enum TimelineViewMode
{
    /// <summary>
    /// Daily view showing individual days with hourly detail.
    /// </summary>
    Daily,

    /// <summary>
    /// Weekly view showing weeks with daily detail (default).
    /// </summary>
    Weekly,

    /// <summary>
    /// Monthly view showing months with weekly detail.
    /// </summary>
    Monthly,

    /// <summary>
    /// Quarterly view showing quarters with monthly detail.
    /// </summary>
    Quarterly,

    /// <summary>
    /// Yearly view showing years with quarterly detail.
    /// </summary>
    Yearly
}

/// <summary>
/// Provides configuration and utilities for different timeline view modes.
/// </summary>
public static class TimelineViewConfig
{
    /// <summary>
    /// Gets the display name for a timeline view mode.
    /// </summary>
    /// <param name="mode">The timeline view mode.</param>
    /// <returns>The display name.</returns>
    public static string GetDisplayName(TimelineViewMode mode)
    {
        return mode switch
        {
            TimelineViewMode.Daily => "Daily View",
            TimelineViewMode.Weekly => "Weekly View",
            TimelineViewMode.Monthly => "Monthly View",
            TimelineViewMode.Quarterly => "Quarterly View",
            TimelineViewMode.Yearly => "Yearly View",
            _ => "Unknown View"
        };
    }

    /// <summary>
    /// Gets the icon for a timeline view mode.
    /// </summary>
    /// <param name="mode">The timeline view mode.</param>
    /// <returns>The icon character.</returns>
    public static string GetIcon(TimelineViewMode mode)
    {
        return mode switch
        {
            TimelineViewMode.Daily => "📅",
            TimelineViewMode.Weekly => "📆",
            TimelineViewMode.Monthly => "🗓️",
            TimelineViewMode.Quarterly => "📊",
            TimelineViewMode.Yearly => "📈",
            _ => "📋"
        };
    }

    /// <summary>
    /// Gets the minimum unit size in pixels for a timeline view mode.
    /// </summary>
    /// <param name="mode">The timeline view mode.</param>
    /// <returns>The minimum unit size in pixels.</returns>
    public static double GetMinimumUnitSize(TimelineViewMode mode)
    {
        return mode switch
        {
            TimelineViewMode.Daily => 40,     // 40px per day minimum
            TimelineViewMode.Weekly => 80,    // 80px per week minimum
            TimelineViewMode.Monthly => 120,  // 120px per month minimum
            TimelineViewMode.Quarterly => 200, // 200px per quarter minimum
            TimelineViewMode.Yearly => 300,   // 300px per year minimum
            _ => 80
        };
    }

    /// <summary>
    /// Gets the optimal zoom range for a timeline view mode.
    /// </summary>
    /// <param name="mode">The timeline view mode.</param>
    /// <returns>A tuple containing minimum and maximum zoom levels.</returns>
    public static (double min, double max) GetZoomRange(TimelineViewMode mode)
    {
        return mode switch
        {
            TimelineViewMode.Daily => (0.5, 3.0),
            TimelineViewMode.Weekly => (0.3, 2.0),
            TimelineViewMode.Monthly => (0.2, 1.5),
            TimelineViewMode.Quarterly => (0.1, 1.2),
            TimelineViewMode.Yearly => (0.1, 1.0),
            _ => (0.1, 2.0)
        };
    }

    /// <summary>
    /// Gets the time unit increment for a timeline view mode.
    /// </summary>
    /// <param name="mode">The timeline view mode.</param>
    /// <returns>The time increment in days.</returns>
    public static int GetTimeUnitDays(TimelineViewMode mode)
    {
        return mode switch
        {
            TimelineViewMode.Daily => 1,
            TimelineViewMode.Weekly => 7,
            TimelineViewMode.Monthly => 30,
            TimelineViewMode.Quarterly => 90,
            TimelineViewMode.Yearly => 365,
            _ => 7
        };
    }

    /// <summary>
    /// Gets the header format strings for a timeline view mode.
    /// </summary>
    /// <param name="mode">The timeline view mode.</param>
    /// <returns>A tuple containing primary and secondary header formats.</returns>
    public static (string primary, string secondary) GetHeaderFormats(TimelineViewMode mode)
    {
        return mode switch
        {
            TimelineViewMode.Daily => ("MMM yyyy", "dd"),
            TimelineViewMode.Weekly => ("MMM yyyy", "'Week' ww"),
            TimelineViewMode.Monthly => ("yyyy", "MMM"),
            TimelineViewMode.Quarterly => ("yyyy", "'Q'q"),
            TimelineViewMode.Yearly => ("yyyy", ""),
            _ => ("MMM yyyy", "'Week' ww")
        };
    }

    /// <summary>
    /// Determines the optimal timeline view mode based on project duration.
    /// </summary>
    /// <param name="projectDurationDays">The project duration in days.</param>
    /// <returns>The recommended timeline view mode.</returns>
    public static TimelineViewMode GetOptimalViewMode(int projectDurationDays)
    {
        return projectDurationDays switch
        {
            <= 30 => TimelineViewMode.Daily,
            <= 180 => TimelineViewMode.Weekly,
            <= 730 => TimelineViewMode.Monthly,
            <= 1460 => TimelineViewMode.Quarterly,
            _ => TimelineViewMode.Yearly
        };
    }

    /// <summary>
    /// Gets all available timeline view modes.
    /// </summary>
    /// <returns>An array of all timeline view modes.</returns>
    public static TimelineViewMode[] GetAllModes()
    {
        return Enum.GetValues<TimelineViewMode>();
    }
}