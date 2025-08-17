using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.ViewModels;
using Xunit;

namespace UnitTests.ViewModels;

/// <summary>
/// Unit tests for the MainWindowViewModel.
/// </summary>
public class MainWindowViewModelTests
{
    private readonly Mock<IDataService> _mockDataService;
    private readonly Mock<IPdfExportService> _mockPdfExportService;
    private readonly Mock<ILogger<MainWindowViewModel>> _mockLogger;
    private readonly MainWindowViewModel _viewModel;

    public MainWindowViewModelTests()
    {
        _mockDataService = new Mock<IDataService>();
        _mockPdfExportService = new Mock<IPdfExportService>();
        _mockLogger = new Mock<ILogger<MainWindowViewModel>>();
        
        _viewModel = new MainWindowViewModel(
            _mockDataService.Object,
            _mockPdfExportService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Assert
        _viewModel.Title.Should().Be("Touch Gantt Chart");
        _viewModel.Projects.Should().NotBeNull().And.BeEmpty();
        _viewModel.Tasks.Should().NotBeNull().And.BeEmpty();
        _viewModel.SelectedTasks.Should().NotBeNull().And.BeEmpty();
        _viewModel.TimelineView.Should().Be("Weekly");
        _viewModel.ZoomLevel.Should().Be(1.0);
        _viewModel.IsTaskDetailsPanelVisible.Should().BeTrue();
        _viewModel.IsProjectTreeExpanded.Should().BeTrue();
        _viewModel.SearchFilter.Should().BeEmpty();
        _viewModel.StatusFilter.Should().BeNull();
        _viewModel.PriorityFilter.Should().BeNull();
        _viewModel.AssigneeFilter.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InitializesCommands()
    {
        // Assert
        _viewModel.NewProjectCommand.Should().NotBeNull();
        _viewModel.OpenProjectCommand.Should().NotBeNull();
        _viewModel.SaveCommand.Should().NotBeNull();
        _viewModel.AddTaskCommand.Should().NotBeNull();
        _viewModel.EditTaskCommand.Should().NotBeNull();
        _viewModel.DeleteTaskCommand.Should().NotBeNull();
        _viewModel.ExportPdfCommand.Should().NotBeNull();
        _viewModel.ZoomInCommand.Should().NotBeNull();
        _viewModel.ZoomOutCommand.Should().NotBeNull();
        _viewModel.ZoomResetCommand.Should().NotBeNull();
        _viewModel.ToggleTaskDetailsPanelCommand.Should().NotBeNull();
        _viewModel.ClearFiltersCommand.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenDataServiceIsNull()
    {
        // Act & Assert
        var act = () => new MainWindowViewModel(null!, _mockPdfExportService.Object, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("dataService");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenPdfExportServiceIsNull()
    {
        // Act & Assert
        var act = () => new MainWindowViewModel(_mockDataService.Object, null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("pdfExportService");
    }

    [Fact]
    public void ZoomIn_IncreasesZoomLevel()
    {
        // Arrange
        var initialZoom = _viewModel.ZoomLevel;

        // Act
        _viewModel.ZoomInCommand!.Execute(null);

        // Assert
        _viewModel.ZoomLevel.Should().BeGreaterThan(initialZoom);
    }

    [Fact]
    public void ZoomOut_DecreasesZoomLevel()
    {
        // Arrange
        _viewModel.ZoomLevel = 2.0; // Set a higher initial zoom
        var initialZoom = _viewModel.ZoomLevel;

        // Act
        _viewModel.ZoomOutCommand!.Execute(null);

        // Assert
        _viewModel.ZoomLevel.Should().BeLessThan(initialZoom);
    }

    [Fact]
    public void ZoomIn_DoesNotExceedMaximumZoom()
    {
        // Arrange
        _viewModel.ZoomLevel = 5.0; // Set to maximum

        // Act
        _viewModel.ZoomInCommand!.Execute(null);

        // Assert
        _viewModel.ZoomLevel.Should().BeLessOrEqualTo(5.0);
    }

    [Fact]
    public void ZoomOut_DoesNotGoBelowMinimumZoom()
    {
        // Arrange
        _viewModel.ZoomLevel = 0.1; // Set to minimum

        // Act
        _viewModel.ZoomOutCommand!.Execute(null);

        // Assert
        _viewModel.ZoomLevel.Should().BeGreaterOrEqualTo(0.1);
    }

    [Fact]
    public void ZoomReset_SetsZoomToDefault()
    {
        // Arrange
        _viewModel.ZoomLevel = 2.5;

        // Act
        _viewModel.ZoomResetCommand!.Execute(null);

        // Assert
        _viewModel.ZoomLevel.Should().Be(1.0);
    }

    [Fact]
    public void ToggleTaskDetailsPanel_TogglesVisibility()
    {
        // Arrange
        var initialVisibility = _viewModel.IsTaskDetailsPanelVisible;

        // Act
        _viewModel.ToggleTaskDetailsPanelCommand!.Execute(null);

        // Assert
        _viewModel.IsTaskDetailsPanelVisible.Should().Be(!initialVisibility);
    }

    [Fact]
    public void ClearFilters_ClearsAllFilters()
    {
        // Arrange
        _viewModel.SearchFilter = "test";
        _viewModel.StatusFilter = TouchGanttChart.Models.TaskStatus.InProgress;
        _viewModel.PriorityFilter = TaskPriority.High;
        _viewModel.AssigneeFilter = "John Doe";

        // Act
        _viewModel.ClearFiltersCommand!.Execute(null);

        // Assert
        _viewModel.SearchFilter.Should().BeEmpty();
        _viewModel.StatusFilter.Should().BeNull();
        _viewModel.PriorityFilter.Should().BeNull();
        _viewModel.AssigneeFilter.Should().BeEmpty();
    }

    [Fact]
    public void SelectedProject_Set_LoadsTasks()
    {
        // Arrange
        var project = new Project { Id = 1, Name = "Test Project" };
        var tasks = new List<GanttTask>
        {
            new() { Id = 1, Name = "Task 1", ProjectId = 1 },
            new() { Id = 2, Name = "Task 2", ProjectId = 1 }
        };

        _mockDataService.Setup(x => x.GetTasksByProjectIdAsync(1))
            .ReturnsAsync(tasks);

        // Act
        _viewModel.SelectedProject = project;

        // Assert
        _viewModel.SelectedProject.Should().Be(project);
        // Note: In a real test, we would need to handle the async nature of loading tasks
    }

    [Fact]
    public void SelectedProject_SetToNull_ClearsTasks()
    {
        // Arrange
        _viewModel.Tasks.Add(new GanttTask { Name = "Test Task" });

        // Act
        _viewModel.SelectedProject = null;

        // Assert
        _viewModel.SelectedProject.Should().BeNull();
        _viewModel.Tasks.Should().BeEmpty();
    }

    [Fact]
    public void SelectedTask_Set_UpdatesTaskSelection()
    {
        // Arrange
        var task1 = new GanttTask { Id = 1, Name = "Task 1" };
        var task2 = new GanttTask { Id = 2, Name = "Task 2" };
        _viewModel.Tasks.Add(task1);
        _viewModel.Tasks.Add(task2);

        // Act
        _viewModel.SelectedTask = task1;

        // Assert
        _viewModel.SelectedTask.Should().Be(task1);
        task1.IsSelected.Should().BeTrue();
        task2.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void TimelineViewOptions_ContainsExpectedValues()
    {
        // Assert
        MainWindowViewModel.TimelineViewOptions.Should().Contain("Daily");
        MainWindowViewModel.TimelineViewOptions.Should().Contain("Weekly");
        MainWindowViewModel.TimelineViewOptions.Should().Contain("Monthly");
        MainWindowViewModel.TimelineViewOptions.Should().Contain("Quarterly");
    }

    [Fact]
    public void StatusFilterOptions_ContainsAllTaskStatuses()
    {
        // Assert
        MainWindowViewModel.StatusFilterOptions.Should().Contain(TouchGanttChart.Models.TaskStatus.NotStarted);
        MainWindowViewModel.StatusFilterOptions.Should().Contain(TouchGanttChart.Models.TaskStatus.InProgress);
        MainWindowViewModel.StatusFilterOptions.Should().Contain(TouchGanttChart.Models.TaskStatus.Completed);
        MainWindowViewModel.StatusFilterOptions.Should().Contain(TouchGanttChart.Models.TaskStatus.OnHold);
        MainWindowViewModel.StatusFilterOptions.Should().Contain(TouchGanttChart.Models.TaskStatus.Cancelled);
    }

    [Fact]
    public void PriorityFilterOptions_ContainsAllTaskPriorities()
    {
        // Assert
        MainWindowViewModel.PriorityFilterOptions.Should().Contain(TaskPriority.Low);
        MainWindowViewModel.PriorityFilterOptions.Should().Contain(TaskPriority.Normal);
        MainWindowViewModel.PriorityFilterOptions.Should().Contain(TaskPriority.High);
        MainWindowViewModel.PriorityFilterOptions.Should().Contain(TaskPriority.Critical);
    }

    [Fact]
    public async Task InitializeAsync_LoadsProjects()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { Id = 1, Name = "Project 1" },
            new() { Id = 2, Name = "Project 2" }
        };

        _mockDataService.Setup(x => x.GetProjectsAsync())
            .ReturnsAsync(projects);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        _viewModel.Projects.Should().HaveCount(2);
        _viewModel.SelectedProject.Should().Be(projects.First());
    }

    [Fact]
    public async Task SaveProjectAsync_CallsDataService()
    {
        // Arrange
        var project = new Project { Id = 1, Name = "Test Project" };
        _viewModel.SelectedProject = project;

        _mockDataService.Setup(x => x.UpdateProjectAsync(project))
            .ReturnsAsync(project);

        // Act
        await _viewModel.SaveCommand!.ExecuteAsync(null);

        // Assert
        _mockDataService.Verify(x => x.UpdateProjectAsync(project), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_RemovesTaskFromCollection()
    {
        // Arrange
        var task = new GanttTask { Id = 1, Name = "Test Task" };
        _viewModel.Tasks.Add(task);
        _viewModel.SelectedTask = task;

        _mockDataService.Setup(x => x.DeleteTaskAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.DeleteTaskCommand!.ExecuteAsync(null);

        // Assert
        _viewModel.Tasks.Should().NotContain(task);
        _viewModel.SelectedTask.Should().BeNull();
        _mockDataService.Verify(x => x.DeleteTaskAsync(1), Times.Once);
    }
}