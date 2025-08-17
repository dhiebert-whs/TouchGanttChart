using FluentAssertions;
using TouchGanttChart.Models;
using Xunit;

namespace UnitTests.Models;

/// <summary>
/// Unit tests for the GanttTask model.
/// </summary>
public class GanttTaskTests
{
    [Fact]
    public void GanttTask_Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var task = new GanttTask();

        // Assert
        task.Id.Should().Be(0);
        task.Name.Should().BeEmpty();
        task.Description.Should().BeEmpty();
        task.StartDate.Should().Be(DateTime.Today);
        task.EndDate.Should().Be(DateTime.Today.AddDays(1));
        task.Progress.Should().Be(0);
        task.Status.Should().Be(TouchGanttChart.Models.TaskStatus.NotStarted);
        task.Priority.Should().Be(TaskPriority.Normal);
        task.Assignee.Should().BeEmpty();
        task.EstimatedHours.Should().Be(0);
        task.ActualHours.Should().Be(0);
        task.ParentTaskId.Should().BeNull();
        task.ParentTask.Should().BeNull();
        task.SubTasks.Should().NotBeNull().And.BeEmpty();
        task.DependentTasks.Should().NotBeNull().And.BeEmpty();
        task.Dependencies.Should().NotBeNull().And.BeEmpty();
        task.IsSelected.Should().BeFalse();
        task.IsExpanded.Should().BeTrue();
    }

    [Fact]
    public void Duration_CalculatesCorrectly()
    {
        // Arrange
        var task = new GanttTask
        {
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 5)
        };

        // Act
        var duration = task.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromDays(4));
    }

    [Fact]
    public void IsOverdue_ReturnsTrueWhenTaskIsOverdue()
    {
        // Arrange
        var task = new GanttTask
        {
            EndDate = DateTime.Today.AddDays(-1),
            Status = TouchGanttChart.Models.TaskStatus.InProgress
        };

        // Act & Assert
        task.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_ReturnsFalseWhenTaskIsCompleted()
    {
        // Arrange
        var task = new GanttTask
        {
            EndDate = DateTime.Today.AddDays(-1),
            Status = TouchGanttChart.Models.TaskStatus.Completed
        };

        // Act & Assert
        task.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_ReturnsFalseWhenTaskIsNotOverdue()
    {
        // Arrange
        var task = new GanttTask
        {
            EndDate = DateTime.Today.AddDays(1),
            Status = TouchGanttChart.Models.TaskStatus.InProgress
        };

        // Act & Assert
        task.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsMilestone_ReturnsTrueForZeroDuration()
    {
        // Arrange
        var task = new GanttTask
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today
        };

        // Act & Assert
        task.IsMilestone.Should().BeTrue();
    }

    [Fact]
    public void IsMilestone_ReturnsFalseForNonZeroDuration()
    {
        // Arrange
        var task = new GanttTask
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };

        // Act & Assert
        task.IsMilestone.Should().BeFalse();
    }

    [Fact]
    public void HasSubTasks_ReturnsTrueWhenSubTasksExist()
    {
        // Arrange
        var task = new GanttTask();
        task.SubTasks.Add(new GanttTask { Name = "Subtask 1" });

        // Act & Assert
        task.HasSubTasks.Should().BeTrue();
    }

    [Fact]
    public void HasSubTasks_ReturnsFalseWhenNoSubTasks()
    {
        // Arrange
        var task = new GanttTask();

        // Act & Assert
        task.HasSubTasks.Should().BeFalse();
    }

    [Fact]
    public void ProgressDisplay_FormatsProgressCorrectly()
    {
        // Arrange
        var task = new GanttTask { Progress = 75 };

        // Act & Assert
        task.ProgressDisplay.Should().Be("75%");
    }

    [Theory]
    [InlineData(0, "Milestone")]
    [InlineData(1, "1 day")]
    [InlineData(2.5, "2.5 days")]
    [InlineData(10, "10.0 days")]
    public void DurationDisplay_FormatsCorrectly(double days, string expected)
    {
        // Arrange
        var task = new GanttTask
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(days)
        };

        // Act & Assert
        task.DurationDisplay.Should().Be(expected);
    }

    [Fact]
    public void Task_CanSetAllProperties()
    {
        // Arrange
        var project = new Project { Id = 1, Name = "Test Project" };
        var parentTask = new GanttTask { Id = 1, Name = "Parent Task" };
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 5);
        var createdDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var modifiedDate = new DateTime(2024, 1, 2, 15, 30, 0);

        // Act
        var task = new GanttTask
        {
            Id = 100,
            Name = "Test Task",
            Description = "Test Description",
            StartDate = startDate,
            EndDate = endDate,
            Progress = 50,
            Status = TouchGanttChart.Models.TaskStatus.InProgress,
            Priority = TaskPriority.High,
            Assignee = "John Doe",
            EstimatedHours = 40,
            ActualHours = 25,
            ParentTaskId = 1,
            ParentTask = parentTask,
            ProjectId = 1,
            Project = project,
            IsSelected = true,
            IsExpanded = false,
            CreatedDate = createdDate,
            LastModifiedDate = modifiedDate
        };

        // Assert
        task.Id.Should().Be(100);
        task.Name.Should().Be("Test Task");
        task.Description.Should().Be("Test Description");
        task.StartDate.Should().Be(startDate);
        task.EndDate.Should().Be(endDate);
        task.Progress.Should().Be(50);
        task.Status.Should().Be(TouchGanttChart.Models.TaskStatus.InProgress);
        task.Priority.Should().Be(TaskPriority.High);
        task.Assignee.Should().Be("John Doe");
        task.EstimatedHours.Should().Be(40);
        task.ActualHours.Should().Be(25);
        task.ParentTaskId.Should().Be(1);
        task.ParentTask.Should().Be(parentTask);
        task.ProjectId.Should().Be(1);
        task.Project.Should().Be(project);
        task.IsSelected.Should().BeTrue();
        task.IsExpanded.Should().BeFalse();
        task.CreatedDate.Should().Be(createdDate);
        task.LastModifiedDate.Should().Be(modifiedDate);
    }
}