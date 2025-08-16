using IronPdf;
using Microsoft.Extensions.Logging;
using System.Text;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;

namespace TouchGanttChart.Services.Implementations;

/// <summary>
/// Implementation of PDF export operations using IronPDF.
/// </summary>
public class PdfExportService : IPdfExportService
{
    private readonly ILogger<PdfExportService> _logger;
    private const string DefaultExportPath = "Exports";

    /// <summary>
    /// Initializes a new instance of the PdfExportService class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public PdfExportService(ILogger<PdfExportService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Initialize IronPDF
        License.LicenseKey = ""; // Free version for now
        EnsureExportDirectoryExists();
    }

    /// <inheritdoc/>
    public async Task<string> ExportProjectToPdfAsync(Project project, IEnumerable<GanttTask> tasks, string? filePath = null)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(tasks);

            _logger.LogInformation("Exporting project '{ProjectName}' to PDF", project.Name);

            if (!ValidateExportParameters(project, tasks))
            {
                throw new ArgumentException("Invalid export parameters");
            }

            filePath ??= GenerateDefaultFilePath($"{SanitizeFileName(project.Name)}_Project_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            var html = GenerateProjectHtml(project, tasks.ToList());
            var renderer = new ChromePdfRenderer();
            
            // Configure PDF settings for professional output
            renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
            renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
            renderer.RenderingOptions.MarginTop = 40;
            renderer.RenderingOptions.MarginBottom = 40;
            renderer.RenderingOptions.MarginLeft = 20;
            renderer.RenderingOptions.MarginRight = 20;
            renderer.RenderingOptions.PrintHtmlBackgrounds = true;

            var pdf = await renderer.RenderHtmlAsPdfAsync(html);
            await pdf.SaveAsAsync(filePath);

            _logger.LogInformation("Successfully exported project '{ProjectName}' to {FilePath}", 
                project.Name, filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting project '{ProjectName}' to PDF", project?.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> ExportTimelineToPdfAsync(Project project, IEnumerable<GanttTask> tasks, 
        DateTime startDate, DateTime endDate, string? filePath = null)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(tasks);

            _logger.LogInformation("Exporting timeline for project '{ProjectName}' from {StartDate} to {EndDate}", 
                project.Name, startDate.ToShortDateString(), endDate.ToShortDateString());

            filePath ??= GenerateDefaultFilePath($"{SanitizeFileName(project.Name)}_Timeline_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            var html = GenerateTimelineHtml(project, tasks.ToList(), startDate, endDate);
            var renderer = new ChromePdfRenderer();
            
            // Configure for landscape timeline view
            renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
            renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
            renderer.RenderingOptions.MarginTop = 20;
            renderer.RenderingOptions.MarginBottom = 20;
            renderer.RenderingOptions.MarginLeft = 15;
            renderer.RenderingOptions.MarginRight = 15;
            renderer.RenderingOptions.PrintHtmlBackgrounds = true;

            var pdf = await renderer.RenderHtmlAsPdfAsync(html);
            await pdf.SaveAsAsync(filePath);

            _logger.LogInformation("Successfully exported timeline to {FilePath}", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting timeline for project '{ProjectName}'", project?.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> ExportProjectSummaryToPdfAsync(Project project, ProjectStatistics statistics, string? filePath = null)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(statistics);

            _logger.LogInformation("Exporting project summary for '{ProjectName}'", project.Name);

            filePath ??= GenerateDefaultFilePath($"{SanitizeFileName(project.Name)}_Summary_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            var html = GenerateProjectSummaryHtml(project, statistics);
            var renderer = new ChromePdfRenderer();
            
            renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
            renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
            renderer.RenderingOptions.MarginTop = 40;
            renderer.RenderingOptions.MarginBottom = 40;
            renderer.RenderingOptions.MarginLeft = 20;
            renderer.RenderingOptions.MarginRight = 20;
            renderer.RenderingOptions.PrintHtmlBackgrounds = true;

            var pdf = await renderer.RenderHtmlAsPdfAsync(html);
            await pdf.SaveAsAsync(filePath);

            _logger.LogInformation("Successfully exported project summary to {FilePath}", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting project summary for '{ProjectName}'", project?.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> ExportTaskListToPdfAsync(IEnumerable<GanttTask> tasks, string? filePath = null)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(tasks);

            var taskList = tasks.ToList();
            _logger.LogInformation("Exporting task list with {TaskCount} tasks", taskList.Count);

            filePath ??= GenerateDefaultFilePath($"TaskList_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            var html = GenerateTaskListHtml(taskList);
            var renderer = new ChromePdfRenderer();
            
            renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
            renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
            renderer.RenderingOptions.MarginTop = 40;
            renderer.RenderingOptions.MarginBottom = 40;
            renderer.RenderingOptions.MarginLeft = 20;
            renderer.RenderingOptions.MarginRight = 20;
            renderer.RenderingOptions.PrintHtmlBackgrounds = true;

            var pdf = await renderer.RenderHtmlAsPdfAsync(html);
            await pdf.SaveAsAsync(filePath);

            _logger.LogInformation("Successfully exported task list to {FilePath}", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting task list");
            throw;
        }
    }

    /// <inheritdoc/>
    public string GetDefaultExportPath()
    {
        return Path.GetFullPath(DefaultExportPath);
    }

    /// <inheritdoc/>
    public bool ValidateExportParameters(Project project, IEnumerable<GanttTask> tasks)
    {
        if (project == null)
        {
            _logger.LogWarning("Project is null for export validation");
            return false;
        }

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            _logger.LogWarning("Project name is empty for export validation");
            return false;
        }

        if (tasks == null)
        {
            _logger.LogWarning("Tasks collection is null for export validation");
            return false;
        }

        return true;
    }

    #region HTML Generation Methods

    private string GenerateProjectHtml(Project project, List<GanttTask> tasks)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='utf-8'>");
        html.AppendLine("<title>Project Report</title>");
        html.AppendLine(GetCommonStyles());
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Header
        html.AppendLine("<div class='header'>");
        html.AppendLine($"<h1>{EscapeHtml(project.Name)}</h1>");
        html.AppendLine($"<p class='subtitle'>Project Report - Generated {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
        html.AppendLine("</div>");
        
        // Project Information
        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Project Information</h2>");
        html.AppendLine("<table class='info-table'>");
        html.AppendLine($"<tr><td><strong>Project Manager:</strong></td><td>{EscapeHtml(project.ProjectManager)}</td></tr>");
        html.AppendLine($"<tr><td><strong>Start Date:</strong></td><td>{project.StartDate:yyyy-MM-dd}</td></tr>");
        html.AppendLine($"<tr><td><strong>End Date:</strong></td><td>{project.EndDate:yyyy-MM-dd}</td></tr>");
        html.AppendLine($"<tr><td><strong>Status:</strong></td><td>{project.Status}</td></tr>");
        html.AppendLine($"<tr><td><strong>Priority:</strong></td><td>{project.Priority}</td></tr>");
        html.AppendLine($"<tr><td><strong>Progress:</strong></td><td>{project.ProgressDisplay}</td></tr>");
        html.AppendLine($"<tr><td><strong>Duration:</strong></td><td>{project.DurationDisplay}</td></tr>");
        if (project.Budget > 0)
        {
            html.AppendLine($"<tr><td><strong>Budget:</strong></td><td>{project.Budget:C}</td></tr>");
            html.AppendLine($"<tr><td><strong>Actual Cost:</strong></td><td>{project.ActualCost:C}</td></tr>");
        }
        html.AppendLine("</table>");
        
        if (!string.IsNullOrWhiteSpace(project.Description))
        {
            html.AppendLine($"<p><strong>Description:</strong></p>");
            html.AppendLine($"<p>{EscapeHtml(project.Description)}</p>");
        }
        html.AppendLine("</div>");
        
        // Task List
        if (tasks.Any())
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Tasks</h2>");
            html.AppendLine(GenerateTaskTable(tasks));
            html.AppendLine("</div>");
        }
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    private string GenerateTimelineHtml(Project project, List<GanttTask> tasks, DateTime startDate, DateTime endDate)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='utf-8'>");
        html.AppendLine("<title>Timeline Report</title>");
        html.AppendLine(GetCommonStyles());
        html.AppendLine(GetTimelineStyles());
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Header
        html.AppendLine("<div class='header'>");
        html.AppendLine($"<h1>{EscapeHtml(project.Name)} - Timeline</h1>");
        html.AppendLine($"<p class='subtitle'>{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}</p>");
        html.AppendLine("</div>");
        
        // Timeline visualization (simplified for PDF)
        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Project Timeline</h2>");
        html.AppendLine(GenerateSimpleTimeline(tasks, startDate, endDate));
        html.AppendLine("</div>");
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    private string GenerateProjectSummaryHtml(Project project, ProjectStatistics statistics)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='utf-8'>");
        html.AppendLine("<title>Project Summary</title>");
        html.AppendLine(GetCommonStyles());
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Header
        html.AppendLine("<div class='header'>");
        html.AppendLine($"<h1>{EscapeHtml(project.Name)} - Summary</h1>");
        html.AppendLine($"<p class='subtitle'>Generated {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
        html.AppendLine("</div>");
        
        // Statistics
        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Project Statistics</h2>");
        html.AppendLine("<table class='info-table'>");
        html.AppendLine($"<tr><td><strong>Total Tasks:</strong></td><td>{statistics.TotalTasks}</td></tr>");
        html.AppendLine($"<tr><td><strong>Completed Tasks:</strong></td><td>{statistics.CompletedTasks}</td></tr>");
        html.AppendLine($"<tr><td><strong>In Progress Tasks:</strong></td><td>{statistics.InProgressTasks}</td></tr>");
        html.AppendLine($"<tr><td><strong>Overdue Tasks:</strong></td><td>{statistics.OverdueTasks}</td></tr>");
        html.AppendLine($"<tr><td><strong>Progress:</strong></td><td>{statistics.ProgressPercentage:F1}%</td></tr>");
        html.AppendLine($"<tr><td><strong>Estimated Hours:</strong></td><td>{statistics.TotalEstimatedHours:F1}</td></tr>");
        html.AppendLine($"<tr><td><strong>Actual Hours:</strong></td><td>{statistics.TotalActualHours:F1}</td></tr>");
        if (statistics.TotalEstimatedHours > 0)
        {
            var efficiency = (statistics.TotalActualHours / statistics.TotalEstimatedHours) * 100;
            html.AppendLine($"<tr><td><strong>Time Efficiency:</strong></td><td>{efficiency:F1}%</td></tr>");
        }
        html.AppendLine("</table>");
        html.AppendLine("</div>");
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    private string GenerateTaskListHtml(List<GanttTask> tasks)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='utf-8'>");
        html.AppendLine("<title>Task List</title>");
        html.AppendLine(GetCommonStyles());
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Header
        html.AppendLine("<div class='header'>");
        html.AppendLine("<h1>Task List</h1>");
        html.AppendLine($"<p class='subtitle'>Generated {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
        html.AppendLine("</div>");
        
        // Task List
        html.AppendLine("<div class='section'>");
        html.AppendLine(GenerateTaskTable(tasks));
        html.AppendLine("</div>");
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    private string GenerateTaskTable(List<GanttTask> tasks)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<table class='task-table'>");
        html.AppendLine("<thead>");
        html.AppendLine("<tr>");
        html.AppendLine("<th>Task Name</th>");
        html.AppendLine("<th>Start Date</th>");
        html.AppendLine("<th>End Date</th>");
        html.AppendLine("<th>Status</th>");
        html.AppendLine("<th>Priority</th>");
        html.AppendLine("<th>Assignee</th>");
        html.AppendLine("<th>Progress</th>");
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");
        
        foreach (var task in tasks.OrderBy(t => t.StartDate))
        {
            var statusClass = task.Status.ToString().ToLowerInvariant();
            var priorityClass = task.Priority.ToString().ToLowerInvariant();
            
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{EscapeHtml(task.Name)}</td>");
            html.AppendLine($"<td>{task.StartDate:yyyy-MM-dd}</td>");
            html.AppendLine($"<td>{task.EndDate:yyyy-MM-dd}</td>");
            html.AppendLine($"<td><span class='status-{statusClass}'>{task.Status}</span></td>");
            html.AppendLine($"<td><span class='priority-{priorityClass}'>{task.Priority}</span></td>");
            html.AppendLine($"<td>{EscapeHtml(task.Assignee)}</td>");
            html.AppendLine($"<td>{task.ProgressDisplay}</td>");
            html.AppendLine("</tr>");
        }
        
        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        
        return html.ToString();
    }

    private string GenerateSimpleTimeline(List<GanttTask> tasks, DateTime startDate, DateTime endDate)
    {
        var html = new StringBuilder();
        var totalDays = (endDate - startDate).Days;
        
        html.AppendLine("<div class='timeline-container'>");
        
        foreach (var task in tasks.OrderBy(t => t.StartDate))
        {
            var taskStartDays = Math.Max(0, (task.StartDate - startDate).Days);
            var taskEndDays = Math.Min(totalDays, (task.EndDate - startDate).Days);
            var taskDuration = Math.Max(1, taskEndDays - taskStartDays);
            
            var leftPercent = (double)taskStartDays / totalDays * 100;
            var widthPercent = (double)taskDuration / totalDays * 100;
            
            var statusClass = task.Status.ToString().ToLowerInvariant();
            
            html.AppendLine("<div class='timeline-row'>");
            html.AppendLine($"<div class='task-label'>{EscapeHtml(task.Name)}</div>");
            html.AppendLine("<div class='timeline-bar-container'>");
            html.AppendLine($"<div class='timeline-bar timeline-{statusClass}' style='left: {leftPercent:F1}%; width: {widthPercent:F1}%;'></div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
        }
        
        html.AppendLine("</div>");
        
        return html.ToString();
    }

    #endregion

    #region Style Methods

    private string GetCommonStyles()
    {
        return @"
<style>
body { 
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
    line-height: 1.6; 
    color: #333; 
    margin: 0; 
    padding: 20px; 
}
.header { 
    text-align: center; 
    margin-bottom: 30px; 
    border-bottom: 2px solid #2c3e50; 
    padding-bottom: 20px; 
}
.header h1 { 
    color: #2c3e50; 
    margin: 0; 
    font-size: 2em; 
}
.subtitle { 
    color: #7f8c8d; 
    margin: 10px 0 0 0; 
    font-style: italic; 
}
.section { 
    margin-bottom: 30px; 
}
.section h2 { 
    color: #34495e; 
    border-bottom: 1px solid #bdc3c7; 
    padding-bottom: 10px; 
}
.info-table { 
    width: 100%; 
    border-collapse: collapse; 
    margin-bottom: 20px; 
}
.info-table td { 
    padding: 8px 12px; 
    border-bottom: 1px solid #ecf0f1; 
}
.info-table td:first-child { 
    width: 30%; 
    background-color: #f8f9fa; 
}
.task-table { 
    width: 100%; 
    border-collapse: collapse; 
    margin-bottom: 20px; 
}
.task-table th, .task-table td { 
    padding: 8px 12px; 
    text-align: left; 
    border-bottom: 1px solid #ddd; 
}
.task-table th { 
    background-color: #f8f9fa; 
    font-weight: bold; 
    color: #2c3e50; 
}
.task-table tr:nth-child(even) { 
    background-color: #f8f9fa; 
}
.status-completed { background-color: #d4edda; color: #155724; padding: 2px 6px; border-radius: 3px; }
.status-inprogress { background-color: #fff3cd; color: #856404; padding: 2px 6px; border-radius: 3px; }
.status-notstarted { background-color: #f8d7da; color: #721c24; padding: 2px 6px; border-radius: 3px; }
.status-onhold { background-color: #e2e3e5; color: #383d41; padding: 2px 6px; border-radius: 3px; }
.status-cancelled { background-color: #f5c6cb; color: #721c24; padding: 2px 6px; border-radius: 3px; }
.priority-low { color: #28a745; }
.priority-normal { color: #6c757d; }
.priority-high { color: #fd7e14; }
.priority-critical { color: #dc3545; font-weight: bold; }
</style>";
    }

    private string GetTimelineStyles()
    {
        return @"
<style>
.timeline-container { 
    width: 100%; 
    margin: 20px 0; 
}
.timeline-row { 
    display: flex; 
    align-items: center; 
    margin-bottom: 15px; 
    height: 30px; 
}
.task-label { 
    width: 200px; 
    font-size: 12px; 
    padding-right: 10px; 
    white-space: nowrap; 
    overflow: hidden; 
    text-overflow: ellipsis; 
}
.timeline-bar-container { 
    flex: 1; 
    height: 20px; 
    background-color: #f8f9fa; 
    position: relative; 
    border: 1px solid #dee2e6; 
}
.timeline-bar { 
    height: 100%; 
    position: absolute; 
    top: 0; 
}
.timeline-completed { background-color: #28a745; }
.timeline-inprogress { background-color: #ffc107; }
.timeline-notstarted { background-color: #6c757d; }
.timeline-onhold { background-color: #17a2b8; }
.timeline-cancelled { background-color: #dc3545; }
</style>";
    }

    #endregion

    #region Helper Methods

    private void EnsureExportDirectoryExists()
    {
        var exportPath = GetDefaultExportPath();
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
            _logger.LogInformation("Created export directory: {ExportPath}", exportPath);
        }
    }

    private string GenerateDefaultFilePath(string fileName)
    {
        return Path.Combine(GetDefaultExportPath(), fileName);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text.Replace("&", "&amp;")
                   .Replace("<", "&lt;")
                   .Replace(">", "&gt;")
                   .Replace("\"", "&quot;")
                   .Replace("'", "&#39;");
    }

    #endregion
}