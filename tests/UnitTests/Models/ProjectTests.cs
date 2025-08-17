using FluentAssertions;
using TouchGanttChart.Models;
using Xunit;

namespace UnitTests.Models;

/// <summary>
/// Unit tests for the Project model.
/// </summary>
public class ProjectTests
{
    [Fact]
    public void Project_Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var project = new Project();

        // Assert
        project.Id.Should().Be(0);
        project.Name.Should().BeEmpty();
        project.Description.Should().BeEmpty();
        project.StartDate.Should().Be(DateTime.Today);
        project.EndDate.Should().Be(DateTime.Today.AddMonths(1));
        project.ProjectManager.Should().BeEmpty();
        project.Status.Should().Be(TouchGanttChart.Models.TaskStatus.NotStarted);
        project.Priority.Should().Be(TaskPriority.Normal);
        project.Budget.Should().Be(0);
        project.ActualCost.Should().Be(0);
        project.IsArchived.Should().BeFalse();
        project.Color.Should().Be("#3498db");
        project.Tasks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TaskCount_ReturnsCorrectCount()
    {
        // Arrange
        var project = new Project();
        project.Tasks.Add(new GanttTask { Name = "Task 1" });
        project.Tasks.Add(new GanttTask { Name = "Task 2" });
        project.Tasks.Add(new GanttTask { Name = "Task 3" });

        // Act & Assert
        project.TaskCount.Should().Be(3);
    }

    [Fact]
    public void CompletedTaskCount_ReturnsCorrectCount()
    {
        // Arrange
        var project = new Project();
        project.Tasks.Add(new GanttTask { Name = "Task 1", Status = TouchGanttChart.Models.TaskStatus.Completed });
        project.Tasks.Add(new GanttTask { Name = "Task 2", Status = TouchGanttChart.Models.TaskStatus.InProgress });
        project.Tasks.Add(new GanttTask { Name = "Task 3", Status = TouchGanttChart.Models.TaskStatus.Completed });

        // Act & Assert
        project.CompletedTaskCount.Should().Be(2);
    }

    [Fact]
    public void InProgressTaskCount_ReturnsCorrectCount()
    {
        // Arrange
        var project = new Project();
        project.Tasks.Add(new GanttTask { Name = "Task 1", Status = TouchGanttChart.Models.TaskStatus.Completed });
        project.Tasks.Add(new GanttTask { Name = "Task 2", Status = TouchGanttChart.Models.TaskStatus.InProgress });
        project.Tasks.Add(new GanttTask { Name = "Task 3", Status = TouchGanttChart.Models.TaskStatus.InProgress });
        project.Tasks.Add(new GanttTask { Name = "Task 4", Status = TouchGanttChart.Models.TaskStatus.NotStarted });

        // Act & Assert
        project.InProgressTaskCount.Should().Be(2);
    }

    [Fact]
    public void OverdueTaskCount_ReturnsCorrectCount()
    {
        // Arrange
        var project = new Project();
        project.Tasks.Add(new GanttTask 
        { 
            Name = "Task 1", 
            EndDate = DateTime.Today.AddDays(-1), 
            Status = TouchGanttChart.Models.TaskStatus.InProgress 
        });
        project.Tasks.Add(new GanttTask 
        { 
            Name = "Task 2", 
            EndDate = DateTime.Today.AddDays(-2), 
            Status = TouchGanttChart.Models.TaskStatus.Completed 
        });
        project.Tasks.Add(new GanttTask 
        { 
            Name = "Task 3", 
            EndDate = DateTime.Today.AddDays(-1), 
            Status = TouchGanttChart.Models.TaskStatus.NotStarted 
        });

        // Act & Assert
        project.OverdueTaskCount.Should().Be(2); // Tasks 1 and 3 are overdue
    }

    [Fact]
    public void ProgressPercentage_CalculatesCorrectly()
    {
        // Arrange
        var project = new Project();
        project.Tasks.Add(new GanttTask { Name = "Task 1", Status = TouchGanttChart.Models.TaskStatus.Completed });
        project.Tasks.Add(new GanttTask { Name = "Task 2", Status = TouchGanttChart.Models.TaskStatus.Completed });
        project.Tasks.Add(new GanttTask { Name = "Task 3", Status = TouchGanttChart.Models.TaskStatus.InProgress });
        project.Tasks.Add(new GanttTask { Name = "Task 4", Status = TouchGanttChart.Models.TaskStatus.NotStarted });

        // Act & Assert
        project.ProgressPercentage.Should().Be(50.0); // 2 out of 4 tasks completed
    }

    [Fact]
    public void ProgressPercentage_ReturnsZeroForEmptyProject()
    {
        // Arrange
        var project = new Project();

        // Act & Assert
        project.ProgressPercentage.Should().Be(0);
    }

    [Fact]
    public void Duration_CalculatesCorrectly()
    {
        // Arrange
        var project = new Project
        {
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31)
        };

        // Act
        var duration = project.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromDays(30));
    }

    [Fact]
    public void IsOverdue_ReturnsTrueWhenProjectIsOverdue()
    {
        // Arrange
        var project = new Project
        {
            EndDate = DateTime.Today.AddDays(-1),
            Status = TouchGanttChart.Models.TaskStatus.InProgress
        };

        // Act & Assert
        project.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_ReturnsFalseWhenProjectIsCompleted()
    {
        // Arrange
        var project = new Project
        {
            EndDate = DateTime.Today.AddDays(-1),
            Status = TouchGanttChart.Models.TaskStatus.Completed
        };

        // Act & Assert
        project.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void TotalEstimatedHours_CalculatesCorrectly()
    {
        // Arrange
        var project = new Project();
        project.Tasks.Add(new GanttTask { Name = "Task 1", EstimatedHours = 10 });
        project.Tasks.Add(new GanttTask { Name = "Task 2", EstimatedHours = 15 });
        project.Tasks.Add(new GanttTask { Name = "Task 3", EstimatedHours = 20 });

        // Act & Assert
        project.TotalEstimatedHours.Should().Be(45);
    }

    [Fact]
    public void TotalActualHours_CalculatesCorrectly()
    {
        // Arrange
        var project = new Project();
        project.Tasks.Add(new GanttTask { Name = "Task 1", ActualHours = 12 });
        project.Tasks.Add(new GanttTask { Name = "Task 2", ActualHours = 18 });
        project.Tasks.Add(new GanttTask { Name = "Task 3", ActualHours = 8 });

        // Act & Assert
        project.TotalActualHours.Should().Be(38);
    }

    [Fact]
    public void BudgetUtilization_CalculatesCorrectly()
    {
        // Arrange
        var project = new Project
        {
            Budget = 10000m,
            ActualCost = 7500m
        };

        // Act & Assert
        project.BudgetUtilization.Should().Be(75.0);
    }

    [Fact]
    public void BudgetUtilization_ReturnsZeroForZeroBudget()
    {
        // Arrange
        var project = new Project
        {
            Budget = 0m,
            ActualCost = 5000m
        };

        // Act & Assert
        project.BudgetUtilization.Should().Be(0);
    }

    [Theory]
    [InlineData(TouchGanttChart.Models.TaskStatus.Completed, "Completed")]
    [InlineData(TouchGanttChart.Models.TaskStatus.InProgress, "At Risk")] // Assuming 50% progress for this test
    public void HealthStatus_ReturnsCorrectStatus(TaskStatus status, string expectedHealth)
    {
        // Arrange
        var project = new Project { Status = status };
        
        if (status == TouchGanttChart.Models.TaskStatus.InProgress)
        {
            // Add tasks to get 50% progress
            project.Tasks.Add(new GanttTask { Status = TouchGanttChart.Models.TaskStatus.Completed });
            project.Tasks.Add(new GanttTask { Status = TouchGanttChart.Models.TaskStatus.InProgress });
        }

        // Act & Assert
        project.HealthStatus.Should().Be(expectedHealth);
    }

    [Fact]
    public void HealthStatus_ReturnsOverdueForOverdueProject()
    {
        // Arrange
        var project = new Project
        {
            EndDate = DateTime.Today.AddDays(-1),
            Status = TouchGanttChart.Models.TaskStatus.InProgress
        };

        // Act & Assert
        project.HealthStatus.Should().Be("Overdue");
    }

    [Theory]
    [InlineData(0.5, "Less than 1 day")]
    [InlineData(1, "1 day")]
    [InlineData(5, "5 days")]
    [InlineData(35, "1.2 months")]
    [InlineData(400, "1.1 years")]
    public void DurationDisplay_FormatsCorrectly(double days, string expected)
    {
        // Arrange
        var project = new Project
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(days)
        };

        // Act & Assert
        project.DurationDisplay.Should().Be(expected);
    }

    [Fact]
    public void ProgressDisplay_FormatsProgressCorrectly()
    {
        // Arrange
        var project = new Project();
        project.Tasks.Add(new GanttTask { Status = TouchGanttChart.Models.TaskStatus.Completed });
        project.Tasks.Add(new GanttTask { Status = TouchGanttChart.Models.TaskStatus.Completed });
        project.Tasks.Add(new GanttTask { Status = TouchGanttChart.Models.TaskStatus.InProgress });
        project.Tasks.Add(new GanttTask { Status = TouchGanttChart.Models.TaskStatus.NotStarted });

        // Act & Assert
        project.ProgressDisplay.Should().Be("50.0%");
    }
}