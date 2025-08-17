using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TouchGanttChart.Data;
using TouchGanttChart.Models;
using Xunit;

namespace IntegrationTests.Data;

/// <summary>
/// Integration tests for the AppDbContext.
/// </summary>
public class AppDbContextTests : IDisposable
{
    private readonly AppDbContext _context;

    public AppDbContextTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CanCreateProject()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test Description",
            ProjectManager = "Test Manager",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1),
            Status = TouchGanttChart.Models.TaskStatus.NotStarted,
            Priority = TaskPriority.Normal,
            Budget = 10000m
        };

        // Act
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Assert
        var savedProject = await _context.Projects.FindAsync(project.Id);
        savedProject.Should().NotBeNull();
        savedProject!.Name.Should().Be("Test Project");
        savedProject.Description.Should().Be("Test Description");
        savedProject.ProjectManager.Should().Be("Test Manager");
        savedProject.Budget.Should().Be(10000m);
    }

    [Fact]
    public async Task CanCreateTaskWithProject()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1)
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new GanttTask
        {
            Name = "Test Task",
            Description = "Test Description",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(5),
            Status = TouchGanttChart.Models.TaskStatus.NotStarted,
            Priority = TaskPriority.Normal,
            Progress = 0,
            ProjectId = project.Id,
            Assignee = "Test Assignee",
            EstimatedHours = 40,
            ActualHours = 0
        };

        // Act
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Assert
        var savedTask = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == task.Id);

        savedTask.Should().NotBeNull();
        savedTask!.Name.Should().Be("Test Task");
        savedTask.Project.Should().NotBeNull();
        savedTask.Project.Name.Should().Be("Test Project");
    }

    [Fact]
    public async Task CanCreateSubTasks()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1)
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var parentTask = new GanttTask
        {
            Name = "Parent Task",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(10),
            ProjectId = project.Id
        };

        _context.Tasks.Add(parentTask);
        await _context.SaveChangesAsync();

        var subTask = new GanttTask
        {
            Name = "Sub Task",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(5),
            ProjectId = project.Id,
            ParentTaskId = parentTask.Id
        };

        // Act
        _context.Tasks.Add(subTask);
        await _context.SaveChangesAsync();

        // Assert
        var savedParentTask = await _context.Tasks
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == parentTask.Id);

        var savedSubTask = await _context.Tasks
            .Include(t => t.ParentTask)
            .FirstOrDefaultAsync(t => t.Id == subTask.Id);

        savedParentTask.Should().NotBeNull();
        savedParentTask!.SubTasks.Should().HaveCount(1);
        savedParentTask.SubTasks.First().Name.Should().Be("Sub Task");

        savedSubTask.Should().NotBeNull();
        savedSubTask!.ParentTask.Should().NotBeNull();
        savedSubTask.ParentTask!.Name.Should().Be("Parent Task");
    }

    [Fact]
    public async Task CanCreateTaskDependencies()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1)
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var prerequisiteTask = new GanttTask
        {
            Name = "Prerequisite Task",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(5),
            ProjectId = project.Id
        };

        var dependentTask = new GanttTask
        {
            Name = "Dependent Task",
            StartDate = DateTime.Today.AddDays(6),
            EndDate = DateTime.Today.AddDays(10),
            ProjectId = project.Id
        };

        _context.Tasks.AddRange(prerequisiteTask, dependentTask);
        await _context.SaveChangesAsync();

        // Act
        dependentTask.Dependencies.Add(prerequisiteTask);
        await _context.SaveChangesAsync();

        // Assert
        var savedDependentTask = await _context.Tasks
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == dependentTask.Id);

        var savedPrerequisiteTask = await _context.Tasks
            .Include(t => t.DependentTasks)
            .FirstOrDefaultAsync(t => t.Id == prerequisiteTask.Id);

        savedDependentTask.Should().NotBeNull();
        savedDependentTask!.Dependencies.Should().HaveCount(1);
        savedDependentTask.Dependencies.First().Name.Should().Be("Prerequisite Task");

        savedPrerequisiteTask.Should().NotBeNull();
        savedPrerequisiteTask!.DependentTasks.Should().HaveCount(1);
        savedPrerequisiteTask.DependentTasks.First().Name.Should().Be("Dependent Task");
    }

    [Fact]
    public async Task ProjectCascadeDelete_DeletesAssociatedTasks()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1)
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task1 = new GanttTask
        {
            Name = "Task 1",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(5),
            ProjectId = project.Id
        };

        var task2 = new GanttTask
        {
            Name = "Task 2",
            StartDate = DateTime.Today.AddDays(6),
            EndDate = DateTime.Today.AddDays(10),
            ProjectId = project.Id
        };

        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        var taskCountBefore = await _context.Tasks.CountAsync();
        taskCountBefore.Should().Be(2);

        // Act
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        // Assert
        var taskCountAfter = await _context.Tasks.CountAsync();
        taskCountAfter.Should().Be(0);

        var projectExists = await _context.Projects.AnyAsync(p => p.Id == project.Id);
        projectExists.Should().BeFalse();
    }

    [Fact]
    public async Task OptimizeDatabaseAsync_ExecutesSuccessfully()
    {
        // Act & Assert
        var act = async () => await _context.OptimizeDatabaseAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SeedDataAsync_CreatesInitialData()
    {
        // Act
        await _context.SeedDataAsync();

        // Assert
        var projectCount = await _context.Projects.CountAsync();
        var taskCount = await _context.Tasks.CountAsync();

        projectCount.Should().BeGreaterThan(0);
        taskCount.Should().BeGreaterThan(0);

        var project = await _context.Projects.FirstAsync();
        project.Name.Should().Be("Touch Gantt Chart Development");
    }

    [Fact]
    public async Task SeedDataAsync_DoesNotCreateDuplicateData()
    {
        // Arrange
        await _context.SeedDataAsync();
        var initialProjectCount = await _context.Projects.CountAsync();
        var initialTaskCount = await _context.Tasks.CountAsync();

        // Act
        await _context.SeedDataAsync();

        // Assert
        var finalProjectCount = await _context.Projects.CountAsync();
        var finalTaskCount = await _context.Tasks.CountAsync();

        finalProjectCount.Should().Be(initialProjectCount);
        finalTaskCount.Should().Be(initialTaskCount);
    }

    [Fact]
    public async Task CanQueryProjectsWithTasks()
    {
        // Arrange
        await _context.SeedDataAsync();

        // Act
        var projects = await _context.Projects
            .Include(p => p.Tasks)
            .ToListAsync();

        // Assert
        projects.Should().NotBeEmpty();
        projects.First().Tasks.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CanQueryTasksWithProject()
    {
        // Arrange
        await _context.SeedDataAsync();

        // Act
        var tasks = await _context.Tasks
            .Include(t => t.Project)
            .ToListAsync();

        // Assert
        tasks.Should().NotBeEmpty();
        tasks.Should().AllSatisfy(task => task.Project.Should().NotBeNull());
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}