using FluentAssertions;
using TouchGanttChart.Models;
using Xunit;

namespace UnitTests.Models;

/// <summary>
/// Unit tests for the TaskTemplate model.
/// </summary>
public class TaskTemplateTests
{
    [Fact]
    public void TaskTemplate_Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var template = new TaskTemplate();

        // Assert
        template.Id.Should().Be(0);
        template.Name.Should().BeEmpty();
        template.Description.Should().BeEmpty();
        template.DefaultAssigneeRole.Should().BeEmpty();
        template.EstimatedHours.Should().Be(0);
        template.EstimatedDurationDays.Should().Be(0);
        template.StartOffsetDays.Should().Be(0);
        template.Priority.Should().Be(TaskPriority.Normal);
        template.Order.Should().Be(0);
        template.IsMilestone.Should().BeFalse();
        template.IsCriticalPath.Should().BeFalse();
        template.ParentTaskTemplateId.Should().BeNull();
        template.ParentTaskTemplate.Should().BeNull();
        template.ChildTaskTemplates.Should().NotBeNull().And.BeEmpty();
        template.Dependencies.Should().NotBeNull().And.BeEmpty();
        template.Dependents.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ChildTaskTemplates_CanBeQueried()
    {
        // Arrange
        var template = new TaskTemplate();
        template.ChildTaskTemplates.Add(new TaskTemplate { Name = "Child Task 1" });

        // Act & Assert
        template.ChildTaskTemplates.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Dependencies_CanBeAddedToTask()
    {
        // Arrange
        var template = new TaskTemplate();
        var dependency = new TaskTemplateDependency
        {
            DependentTaskTemplate = template,
            PrerequisiteTaskTemplate = new TaskTemplate { Name = "Prerequisite Task" },
            DependencyType = DependencyType.FinishToStart,
            LagDays = 0
        };
        template.Dependencies.Add(dependency);

        // Act & Assert
        template.Dependencies.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void DurationDisplay_FormatsCorrectly()
    {
        // Arrange
        var template = new TaskTemplate { EstimatedDurationDays = 5 };

        // Act & Assert
        template.DurationDisplay.Should().Be("5 days");
    }

    [Fact]
    public void HoursDisplay_FormatsCorrectly()
    {
        // Arrange
        var template = new TaskTemplate { EstimatedHours = 40.5m };

        // Act & Assert
        template.HoursDisplay.Should().Be("40.5h");
    }


    [Fact]
    public void TaskTemplate_CanSetAllProperties()
    {
        // Arrange
        var projectTemplate = new ProjectTemplate { Id = 1, Name = "Test Project Template" };
        var parentTemplate = new TaskTemplate { Id = 1, Name = "Parent Task Template" };
        var createdDate = new DateTime(2024, 1, 1, 10, 0, 0);

        // Act
        var template = new TaskTemplate
        {
            Id = 100,
            Name = "Test Task Template",
            Description = "Test Description",
            DefaultAssigneeRole = "Software Engineer",
            EstimatedHours = 40m,
            EstimatedDurationDays = 5,
            StartOffsetDays = 2,
            Priority = TaskPriority.High,
            Order = 3,
            IsMilestone = true,
            IsCriticalPath = true,
            ParentTaskTemplateId = 1,
            ParentTaskTemplate = parentTemplate,
            ProjectTemplateId = 1,
            ProjectTemplate = projectTemplate,
            CreatedDate = createdDate
        };

        // Assert
        template.Id.Should().Be(100);
        template.Name.Should().Be("Test Task Template");
        template.Description.Should().Be("Test Description");
        template.DefaultAssigneeRole.Should().Be("Software Engineer");
        template.EstimatedHours.Should().Be(40m);
        template.EstimatedDurationDays.Should().Be(5);
        template.StartOffsetDays.Should().Be(2);
        template.Priority.Should().Be(TaskPriority.High);
        template.Order.Should().Be(3);
        template.IsMilestone.Should().BeTrue();
        template.IsCriticalPath.Should().BeTrue();
        template.ParentTaskTemplateId.Should().Be(1);
        template.ParentTaskTemplate.Should().Be(parentTemplate);
        template.ProjectTemplateId.Should().Be(1);
        template.ProjectTemplate.Should().Be(projectTemplate);
        template.CreatedDate.Should().Be(createdDate);
    }

    [Theory]
    [InlineData(0, "Not specified")]
    [InlineData(1, "1 day")]
    [InlineData(5, "5 days")]
    [InlineData(10, "10 days")]
    [InlineData(30, "30 days")]
    public void DurationDisplay_HandlesVariousDurations(int days, string expected)
    {
        // Arrange
        var template = new TaskTemplate { EstimatedDurationDays = days };

        // Act & Assert
        template.DurationDisplay.Should().Be(expected);
    }

    [Fact]
    public void Dependencies_CanBeAdded()
    {
        // Arrange
        var template = new TaskTemplate();
        var dependency1 = new TaskTemplateDependency();
        var dependency2 = new TaskTemplateDependency();
        template.Dependencies.Add(dependency1);
        template.Dependencies.Add(dependency2);

        // Act & Assert
        template.Dependencies.Count.Should().Be(2);
    }

    [Fact]
    public void ChildTaskTemplates_CanBeAdded()
    {
        // Arrange
        var template = new TaskTemplate();
        template.ChildTaskTemplates.Add(new TaskTemplate());
        template.ChildTaskTemplates.Add(new TaskTemplate());
        template.ChildTaskTemplates.Add(new TaskTemplate());

        // Act & Assert
        template.ChildTaskTemplates.Count.Should().Be(3);
    }
}