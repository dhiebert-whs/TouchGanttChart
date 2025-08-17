using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;

namespace TouchGanttChart.Services.Implementations;

/// <summary>
/// Service for exporting Gantt chart data to PDF format using QuestPDF (free library).
/// Provides professional-looking reports optimized for touch devices and large displays.
/// </summary>
public class PdfExportService : IPdfExportService
{
    private readonly ILogger<PdfExportService> _logger;
    private const string DefaultExportPath = "Exports";

    public PdfExportService(ILogger<PdfExportService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Configure QuestPDF license for community usage (free)
        QuestPDF.Settings.License = LicenseType.Community;
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

            filePath ??= GenerateDefaultFilePath($"{SanitizeFileName(project.Name)}_Project_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                        page.Header()
                            .Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text($"Project: {project.Name}")
                                        .FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);
                                    column.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}")
                                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                                });
                            });

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                // Project Overview
                                column.Item().Element(container => ProjectOverviewSection(container, project));
                                
                                column.Item().PaddingTop(0.5f, Unit.Centimetre);
                                
                                // Tasks Table
                                column.Item().Element(container => TasksTableSection(container, tasks));
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(text => 
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                    });
                });
                
                document.GeneratePdf(filePath);
            });

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

            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                        page.Header()
                            .Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text($"Timeline: {project.Name}")
                                        .FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                                    column.Item().Text($"Project Duration: {project.DurationDisplay}")
                                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                                });
                            });

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Element(container => TimelineSection(container, project, tasks, startDate, endDate));

                        page.Footer()
                            .AlignCenter()
                            .Text(text => 
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                    });
                });
                
                document.GeneratePdf(filePath);
            });

            _logger.LogInformation("Timeline PDF export completed: {FilePath}", filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting timeline to PDF: {ProjectName}", project?.Name);
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

            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                        page.Header()
                            .Text($"Project Summary: {project.Name}")
                            .FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Element(container => ProjectSummarySection(container, project, statistics));

                        page.Footer()
                            .AlignCenter()
                            .Text(text => 
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                    });
                });
                
                document.GeneratePdf(filePath);
            });

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

            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                        page.Header()
                            .Text("Detailed Task Report")
                            .FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Element(container => DetailedTasksSection(container, tasks));

                        page.Footer()
                            .AlignCenter()
                            .Text(text => 
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                    });
                });
                
                document.GeneratePdf(filePath);
            });

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

    #region Private Helper Methods

    private void ProjectOverviewSection(IContainer container, Project project)
    {
        container.Column(column =>
        {
            column.Item().Text("Project Overview").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken1);
            
            column.Item().PaddingTop(0.2f, Unit.Centimetre).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(3, Unit.Centimetre);
                    columns.RelativeColumn();
                });

                table.Cell().Element(CellStyle).Text("Project Manager:").SemiBold();
                table.Cell().Element(CellStyle).Text(project.ProjectManager);

                table.Cell().Element(CellStyle).Text("Start Date:").SemiBold();
                table.Cell().Element(CellStyle).Text(project.StartDate.ToString("yyyy-MM-dd"));

                table.Cell().Element(CellStyle).Text("End Date:").SemiBold();
                table.Cell().Element(CellStyle).Text(project.EndDate.ToString("yyyy-MM-dd"));

                table.Cell().Element(CellStyle).Text("Status:").SemiBold();
                table.Cell().Element(CellStyle).Text(project.Status.ToString());

                table.Cell().Element(CellStyle).Text("Progress:").SemiBold();
                table.Cell().Element(CellStyle).Text(project.ProgressDisplay);

                table.Cell().Element(CellStyle).Text("Budget:").SemiBold();
                table.Cell().Element(CellStyle).Text($"${project.Budget:N2}");

                table.Cell().Element(CellStyle).Text("Health Status:").SemiBold();
                table.Cell().Element(CellStyle).Text(project.HealthStatus);
            });
        });

        static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        }
    }

    private void TasksTableSection(IContainer container, IEnumerable<GanttTask> tasks)
    {
        container.Column(column =>
        {
            column.Item().Text("Tasks Summary").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken1);
            
            column.Item().PaddingTop(0.2f, Unit.Centimetre).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Task Name
                    columns.ConstantColumn(2, Unit.Centimetre); // Status
                    columns.ConstantColumn(2, Unit.Centimetre); // Progress
                    columns.ConstantColumn(2.5f, Unit.Centimetre); // Start Date
                    columns.ConstantColumn(2.5f, Unit.Centimetre); // End Date
                    columns.RelativeColumn(2); // Assignee
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("Task Name").SemiBold();
                    header.Cell().Element(HeaderStyle).Text("Status").SemiBold();
                    header.Cell().Element(HeaderStyle).Text("Progress").SemiBold();
                    header.Cell().Element(HeaderStyle).Text("Start Date").SemiBold();
                    header.Cell().Element(HeaderStyle).Text("End Date").SemiBold();
                    header.Cell().Element(HeaderStyle).Text("Assignee").SemiBold();
                });

                // Rows
                foreach (var task in tasks.Take(20)) // Limit to prevent overcrowding
                {
                    table.Cell().Element(CellStyle).Text(task.Name);
                    table.Cell().Element(CellStyle).Text(task.Status.ToString());
                    table.Cell().Element(CellStyle).Text(task.ProgressDisplay);
                    table.Cell().Element(CellStyle).Text(task.StartDate.ToString("MM/dd"));
                    table.Cell().Element(CellStyle).Text(task.EndDate.ToString("MM/dd"));
                    table.Cell().Element(CellStyle).Text(task.Assignee);
                }
            });
        });

        static IContainer HeaderStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.SemiBold())
                          .PaddingVertical(5)
                          .BorderBottom(1)
                          .BorderColor(Colors.Black)
                          .Background(Colors.Grey.Lighten3);
        }

        static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1)
                          .BorderColor(Colors.Grey.Lighten2)
                          .PaddingVertical(5);
        }
    }

    private void TimelineSection(IContainer container, Project project, IEnumerable<GanttTask> tasks, DateTime startDate, DateTime endDate)
    {
        // Simplified timeline representation
        container.Column(column =>
        {
            column.Item().Text("Timeline Overview").FontSize(14).SemiBold();
            
            var sortedTasks = tasks.OrderBy(t => t.StartDate).ToList();
            
            foreach (var task in sortedTasks.Take(15)) // Limit for space
            {
                column.Item().PaddingTop(0.1f, Unit.Centimetre).Row(row =>
                {
                    row.ConstantItem(3, Unit.Centimetre).Text(task.Name).FontSize(9);
                    
                    // Simple timeline bar representation using background color
                    row.RelativeItem().Background(GetTaskColor(task.Status))
                        .Height(10).AlignMiddle();
                });
            }
        });
    }

    private void DetailedTasksSection(IContainer container, IEnumerable<GanttTask> tasks)
    {
        container.Column(column =>
        {
            foreach (var task in tasks)
            {
                column.Item().PaddingBottom(0.5f, Unit.Centimetre).Border(1)
                    .BorderColor(Colors.Grey.Lighten1).Padding(0.3f, Unit.Centimetre)
                    .Column(taskColumn =>
                    {
                        taskColumn.Item().Text(task.Name).FontSize(12).SemiBold();
                        taskColumn.Item().Text($"Description: {task.Description}").FontSize(10);
                        taskColumn.Item().Text($"Status: {task.Status}, Priority: {task.Priority}").FontSize(10);
                        taskColumn.Item().Text($"Duration: {task.DurationDisplay}, Progress: {task.ProgressDisplay}").FontSize(10);
                        taskColumn.Item().Text($"Assignee: {task.Assignee}").FontSize(10);
                    });
            }
        });
    }

    private void ProjectSummarySection(IContainer container, Project project, ProjectStatistics statistics)
    {
        container.Column(column =>
        {
            column.Item().Text("Project Statistics").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken1);
            
            column.Item().PaddingTop(0.2f, Unit.Centimetre).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(4, Unit.Centimetre);
                    columns.RelativeColumn();
                });

                table.Cell().Element(CellStyle).Text("Total Tasks:").SemiBold();
                table.Cell().Element(CellStyle).Text(statistics.TotalTasks.ToString());

                table.Cell().Element(CellStyle).Text("Completed Tasks:").SemiBold();
                table.Cell().Element(CellStyle).Text(statistics.CompletedTasks.ToString());

                table.Cell().Element(CellStyle).Text("In Progress Tasks:").SemiBold();
                table.Cell().Element(CellStyle).Text(statistics.InProgressTasks.ToString());

                table.Cell().Element(CellStyle).Text("Overdue Tasks:").SemiBold();
                table.Cell().Element(CellStyle).Text(statistics.OverdueTasks.ToString());

                table.Cell().Element(CellStyle).Text("Progress:").SemiBold();
                table.Cell().Element(CellStyle).Text($"{statistics.ProgressPercentage:F1}%");

                table.Cell().Element(CellStyle).Text("Estimated Hours:").SemiBold();
                table.Cell().Element(CellStyle).Text($"{statistics.TotalEstimatedHours:F1}");

                table.Cell().Element(CellStyle).Text("Actual Hours:").SemiBold();
                table.Cell().Element(CellStyle).Text($"{statistics.TotalActualHours:F1}");
            });
        });

        static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        }
    }

    private string GetTaskColor(TouchGanttChart.Models.TaskStatus status)
    {
        return status switch
        {
            TouchGanttChart.Models.TaskStatus.NotStarted => Colors.Grey.Medium,
            TouchGanttChart.Models.TaskStatus.InProgress => Colors.Blue.Medium,
            TouchGanttChart.Models.TaskStatus.Completed => Colors.Green.Medium,
            TouchGanttChart.Models.TaskStatus.OnHold => Colors.Orange.Medium,
            TouchGanttChart.Models.TaskStatus.Cancelled => Colors.Red.Medium,
            _ => Colors.Grey.Medium
        };
    }

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

    #endregion
}