using TouchGanttChart.Models;

namespace TouchGanttChart.Services.Interfaces;

/// <summary>
/// Interface for PDF export operations.
/// </summary>
public interface IPdfExportService
{
    /// <summary>
    /// Exports a project and its tasks to a PDF file asynchronously.
    /// </summary>
    /// <param name="project">The project to export.</param>
    /// <param name="tasks">The tasks to include in the export.</param>
    /// <param name="filePath">Optional custom file path. If not provided, a default path will be used.</param>
    /// <returns>The path to the generated PDF file.</returns>
    Task<string> ExportProjectToPdfAsync(Project project, IEnumerable<GanttTask> tasks, string? filePath = null);

    /// <summary>
    /// Exports a Gantt chart timeline to a PDF file asynchronously.
    /// </summary>
    /// <param name="project">The project to export.</param>
    /// <param name="tasks">The tasks to include in the timeline.</param>
    /// <param name="startDate">The timeline start date.</param>
    /// <param name="endDate">The timeline end date.</param>
    /// <param name="filePath">Optional custom file path. If not provided, a default path will be used.</param>
    /// <returns>The path to the generated PDF file.</returns>
    Task<string> ExportTimelineToPdfAsync(Project project, IEnumerable<GanttTask> tasks, DateTime startDate, DateTime endDate, string? filePath = null);

    /// <summary>
    /// Exports project statistics and summary to a PDF file asynchronously.
    /// </summary>
    /// <param name="project">The project to export.</param>
    /// <param name="statistics">The project statistics.</param>
    /// <param name="filePath">Optional custom file path. If not provided, a default path will be used.</param>
    /// <returns>The path to the generated PDF file.</returns>
    Task<string> ExportProjectSummaryToPdfAsync(Project project, ProjectStatistics statistics, string? filePath = null);

    /// <summary>
    /// Exports task details to a PDF file asynchronously.
    /// </summary>
    /// <param name="tasks">The tasks to export.</param>
    /// <param name="filePath">Optional custom file path. If not provided, a default path will be used.</param>
    /// <returns>The path to the generated PDF file.</returns>
    Task<string> ExportTaskListToPdfAsync(IEnumerable<GanttTask> tasks, string? filePath = null);

    /// <summary>
    /// Gets the default export directory path.
    /// </summary>
    /// <returns>The default export directory path.</returns>
    string GetDefaultExportPath();

    /// <summary>
    /// Validates the export parameters.
    /// </summary>
    /// <param name="project">The project to validate.</param>
    /// <param name="tasks">The tasks to validate.</param>
    /// <returns>True if parameters are valid for export, false otherwise.</returns>
    bool ValidateExportParameters(Project project, IEnumerable<GanttTask> tasks);
}