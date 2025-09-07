using FluentAssertions;
using TouchGanttChart.Models;
using Xunit;

namespace UnitTests.Models;

/// <summary>
/// Unit tests for the ProjectTemplate model.
/// </summary>
public class ProjectTemplateTests
{
    [Fact]
    public void ProjectTemplate_Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var template = new ProjectTemplate();

        // Assert
        template.Id.Should().Be(0);
        template.Name.Should().BeEmpty();
        template.Description.Should().BeEmpty();
        template.Category.Should().BeEmpty();
        template.EstimatedBudget.Should().Be(0);
        template.EstimatedDurationDays.Should().Be(0);
        template.Icon.Should().Be("📋");
        template.IsActive.Should().BeTrue();
        template.IsBuiltIn.Should().BeFalse();
        template.UsageCount.Should().Be(0);
        template.TaskTemplates.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TaskCount_ReturnsCorrectCount()
    {
        // Arrange
        var template = new ProjectTemplate();
        template.TaskTemplates.Add(new TaskTemplate { Name = "Task 1" });
        template.TaskTemplates.Add(new TaskTemplate { Name = "Task 2" });
        template.TaskTemplates.Add(new TaskTemplate { Name = "Task 3" });

        // Act & Assert
        template.TaskCount.Should().Be(3);
    }

    [Fact]
    public void DurationDisplay_FormatsCorrectly()
    {
        // Arrange
        var template = new ProjectTemplate { EstimatedDurationDays = 30 };

        // Act & Assert
        template.DurationDisplay.Should().Be("1 months");
    }

    [Fact]
    public void BudgetDisplay_FormatsCorrectly()
    {
        // Arrange
        var template = new ProjectTemplate { EstimatedBudget = 50000m };

        // Act & Assert
        template.BudgetDisplay.Should().Be("$50,000");
    }

    [Fact]
    public void ProjectTemplate_CanSetAllProperties()
    {
        // Arrange
        var createdDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var modifiedDate = new DateTime(2024, 1, 2, 15, 30, 0);

        // Act
        var template = new ProjectTemplate
        {
            Id = 100,
            Name = "Test Template",
            Description = "Test Description",
            Category = "Software Development",
            EstimatedBudget = 25000m,
            EstimatedDurationDays = 60,
            Icon = "💻",
            IsActive = false,
            IsBuiltIn = true,
            UsageCount = 5,
            CreatedDate = createdDate,
            LastModifiedDate = modifiedDate
        };

        // Assert
        template.Id.Should().Be(100);
        template.Name.Should().Be("Test Template");
        template.Description.Should().Be("Test Description");
        template.Category.Should().Be("Software Development");
        template.EstimatedBudget.Should().Be(25000m);
        template.EstimatedDurationDays.Should().Be(60);
        template.Icon.Should().Be("💻");
        template.IsActive.Should().BeFalse();
        template.IsBuiltIn.Should().BeTrue();
        template.UsageCount.Should().Be(5);
        template.CreatedDate.Should().Be(createdDate);
        template.LastModifiedDate.Should().Be(modifiedDate);
    }

    [Theory]
    [InlineData(0, "Not specified")]
    [InlineData(1, "1 day")]
    [InlineData(7, "1 weeks")]
    [InlineData(14, "2 weeks")]
    [InlineData(30, "1 months")]
    [InlineData(90, "3 months")]
    [InlineData(365, "12 months")]
    public void DurationDisplay_HandlesVariousDurations(int days, string expected)
    {
        // Arrange
        var template = new ProjectTemplate { EstimatedDurationDays = days };

        // Act & Assert
        template.DurationDisplay.Should().Be(expected);
    }

    [Fact]
    public void TotalEstimatedHours_CalculatesCorrectly()
    {
        // Arrange
        var template = new ProjectTemplate();
        template.TaskTemplates.Add(new TaskTemplate { EstimatedHours = 10m });
        template.TaskTemplates.Add(new TaskTemplate { EstimatedHours = 20m });
        template.TaskTemplates.Add(new TaskTemplate { EstimatedHours = 15.5m });

        // Act & Assert
        var totalHours = template.TaskTemplates.Sum(t => t.EstimatedHours);
        totalHours.Should().Be(45.5m);
    }

    [Fact]
    public void TaskCount_CalculatesCorrectlyForVariousTemplates()
    {
        // Arrange
        var simpleTemplate = new ProjectTemplate();
        simpleTemplate.TaskTemplates.Add(new TaskTemplate());
        simpleTemplate.TaskTemplates.Add(new TaskTemplate());

        var mediumTemplate = new ProjectTemplate();
        for (int i = 0; i < 8; i++)
            mediumTemplate.TaskTemplates.Add(new TaskTemplate());

        var complexTemplate = new ProjectTemplate();
        for (int i = 0; i < 25; i++)
            complexTemplate.TaskTemplates.Add(new TaskTemplate());

        // Act & Assert
        simpleTemplate.TaskCount.Should().Be(2);
        mediumTemplate.TaskCount.Should().Be(8);
        complexTemplate.TaskCount.Should().Be(25);
    }
}