using FluentAssertions;
using TouchGanttChart.Models;
using Xunit;

namespace UnitTests.Models;

/// <summary>
/// Unit tests for the TaskTemplateDependency model.
/// </summary>
public class TaskTemplateDependencyTests
{
    [Fact]
    public void TaskTemplateDependency_Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var dependency = new TaskTemplateDependency();

        // Assert
        dependency.DependentTaskTemplateId.Should().Be(0);
        dependency.PrerequisiteTaskTemplateId.Should().Be(0);
        dependency.DependencyType.Should().Be(DependencyType.FinishToStart);
        dependency.LagDays.Should().Be(0);
        dependency.DependentTaskTemplate.Should().BeNull();
        dependency.PrerequisiteTaskTemplate.Should().BeNull();
    }

    [Fact]
    public void TaskTemplateDependency_CanSetAllProperties()
    {
        // Arrange
        var dependentTask = new TaskTemplate { Id = 1, Name = "Dependent Task" };
        var prerequisiteTask = new TaskTemplate { Id = 2, Name = "Prerequisite Task" };

        // Act
        var dependency = new TaskTemplateDependency
        {
            DependentTaskTemplateId = 1,
            PrerequisiteTaskTemplateId = 2,
            DependencyType = DependencyType.StartToStart,
            LagDays = 3,
            DependentTaskTemplate = dependentTask,
            PrerequisiteTaskTemplate = prerequisiteTask
        };

        // Assert
        dependency.DependentTaskTemplateId.Should().Be(1);
        dependency.PrerequisiteTaskTemplateId.Should().Be(2);
        dependency.DependencyType.Should().Be(DependencyType.StartToStart);
        dependency.LagDays.Should().Be(3);
        dependency.DependentTaskTemplate.Should().Be(dependentTask);
        dependency.PrerequisiteTaskTemplate.Should().Be(prerequisiteTask);
    }

    [Theory]
    [InlineData(DependencyType.FinishToStart)]
    [InlineData(DependencyType.StartToStart)]
    [InlineData(DependencyType.FinishToFinish)]
    [InlineData(DependencyType.StartToFinish)]
    public void DependencyType_CanBeSetToValidValues(DependencyType dependencyType)
    {
        // Arrange
        var dependency = new TaskTemplateDependency { DependencyType = dependencyType };

        // Act & Assert
        dependency.DependencyType.Should().Be(dependencyType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(-2)]
    [InlineData(10)]
    public void LagDays_CanBeSetToAnyInteger(int lagDays)
    {
        // Arrange
        var dependency = new TaskTemplateDependency { LagDays = lagDays };

        // Act & Assert
        dependency.LagDays.Should().Be(lagDays);
    }

    [Fact]
    public void NavigationProperties_CanBeSet()
    {
        // Arrange
        var dependentTask = new TaskTemplate 
        { 
            Id = 1, 
            Name = "Dependent Task",
            Description = "This task depends on another"
        };
        var prerequisiteTask = new TaskTemplate 
        { 
            Id = 2, 
            Name = "Prerequisite Task",
            Description = "This task must complete first"
        };

        // Act
        var dependency = new TaskTemplateDependency
        {
            DependentTaskTemplate = dependentTask,
            PrerequisiteTaskTemplate = prerequisiteTask
        };

        // Assert
        dependency.DependentTaskTemplate.Should().Be(dependentTask);
        dependency.PrerequisiteTaskTemplate.Should().Be(prerequisiteTask);
        dependency.DependentTaskTemplate.Name.Should().Be("Dependent Task");
        dependency.PrerequisiteTaskTemplate.Name.Should().Be("Prerequisite Task");
    }

    [Fact]
    public void TaskTemplateDependency_SupportsAllDependencyTypes()
    {
        // Arrange & Act & Assert
        var finishToStartDep = new TaskTemplateDependency { DependencyType = DependencyType.FinishToStart };
        var startToStartDep = new TaskTemplateDependency { DependencyType = DependencyType.StartToStart };
        var finishToFinishDep = new TaskTemplateDependency { DependencyType = DependencyType.FinishToFinish };
        var startToFinishDep = new TaskTemplateDependency { DependencyType = DependencyType.StartToFinish };

        finishToStartDep.DependencyType.Should().Be(DependencyType.FinishToStart);
        startToStartDep.DependencyType.Should().Be(DependencyType.StartToStart);
        finishToFinishDep.DependencyType.Should().Be(DependencyType.FinishToFinish);
        startToFinishDep.DependencyType.Should().Be(DependencyType.StartToFinish);
    }

    [Fact]
    public void TaskTemplateDependency_ValidatesIds()
    {
        // Arrange
        var dependency = new TaskTemplateDependency
        {
            DependentTaskTemplateId = 1,
            PrerequisiteTaskTemplateId = 2
        };

        // Act & Assert
        dependency.DependentTaskTemplateId.Should().NotBe(dependency.PrerequisiteTaskTemplateId);
        dependency.DependentTaskTemplateId.Should().BeGreaterThan(0);
        dependency.PrerequisiteTaskTemplateId.Should().BeGreaterThan(0);
    }
}