using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TouchGanttChart.Data;
using TouchGanttChart.Models;
using TouchGanttChart.Services.Implementations;
using Xunit;

namespace IntegrationTests.Services;

/// <summary>
/// Integration tests for the DataService class.
/// Tests the service with a real in-memory database to verify CRUD operations and business logic.
/// </summary>
public class DataServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DataService _dataService;
    private readonly Mock<ILogger<DataService>> _mockLogger;

    public DataServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<DataService>>();
        _dataService = new DataService(_context, _mockLogger.Object);
    }

    #region Project Tests

    [Fact]
    public async Task GetProjectsAsync_ReturnsAllProjects()
    {
        // Arrange
        var project1 = new Project { Name = "Project 1", Description = "First project" };
        var project2 = new Project { Name = "Project 2", Description = "Second project" };
        
        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetProjectsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Project 1");
        result.Should().Contain(p => p.Name == "Project 2");
    }

    [Fact]
    public async Task GetProjectByIdAsync_ReturnsCorrectProject()
    {
        // Arrange
        var project = new Project { Name = "Test Project", Description = "Test Description" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetProjectByIdAsync(project.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Project");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task CreateProjectAsync_CreatesProjectSuccessfully()
    {
        // Arrange
        var project = new Project
        {
            Name = "New Project",
            Description = "New Description",
            ProjectManager = "John Doe",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1),
            Budget = 50000m
        };

        // Act
        var result = await _dataService.CreateProjectAsync(project);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var savedProject = await _context.Projects.FindAsync(result.Id);
        savedProject.Should().NotBeNull();
        savedProject!.Name.Should().Be("New Project");
    }

    [Fact]
    public async Task UpdateProjectAsync_UpdatesProjectSuccessfully()
    {
        // Arrange
        var project = new Project { Name = "Original Name", Description = "Original Description" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        project.Name = "Updated Name";
        project.Description = "Updated Description";
        project.Budget = 75000m;

        // Act
        var result = await _dataService.UpdateProjectAsync(project);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Budget.Should().Be(75000m);
        result.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var savedProject = await _context.Projects.FindAsync(project.Id);
        savedProject!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteProjectAsync_DeletesProjectAndTasks()
    {
        // Arrange
        var project = new Project { Name = "Project to Delete" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new GanttTask
        {
            Name = "Task to Delete",
            ProjectId = project.Id,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        await _dataService.DeleteProjectAsync(project.Id);

        // Assert
        var deletedProject = await _context.Projects.FindAsync(project.Id);
        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        
        deletedProject.Should().BeNull();
        deletedTask.Should().BeNull();
    }

    #endregion

    #region Task Tests

    [Fact]
    public async Task GetTasksByProjectIdAsync_ReturnsProjectTasks()
    {
        // Arrange
        var project1 = new Project { Name = "Project 1" };
        var project2 = new Project { Name = "Project 2" };
        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        var task1 = new GanttTask { Name = "Task 1", ProjectId = project1.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        var task2 = new GanttTask { Name = "Task 2", ProjectId = project1.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        var task3 = new GanttTask { Name = "Task 3", ProjectId = project2.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        _context.Tasks.AddRange(task1, task2, task3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetTasksByProjectIdAsync(project1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Task 1");
        result.Should().Contain(t => t.Name == "Task 2");
        result.Should().NotContain(t => t.Name == "Task 3");
    }

    [Fact]
    public async Task GetTaskByIdAsync_ReturnsCorrectTask()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new GanttTask
        {
            Name = "Test Task",
            Description = "Test Description",
            ProjectId = project.Id,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(5),
            Assignee = "John Doe",
            EstimatedHours = 40
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetTaskByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Task");
        result.Description.Should().Be("Test Description");
        result.Assignee.Should().Be("John Doe");
        result.EstimatedHours.Should().Be(40);
        result.Project.Should().NotBeNull();
        result.Project.Name.Should().Be("Test Project");
    }

    [Fact]
    public async Task CreateTaskAsync_CreatesTaskSuccessfully()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new GanttTask
        {
            Name = "New Task",
            Description = "New Description",
            ProjectId = project.Id,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(3),
            Status = TaskStatus.NotStarted,
            Priority = TaskPriority.High,
            Assignee = "Jane Smith",
            EstimatedHours = 24
        };

        // Act
        var result = await _dataService.CreateTaskAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var savedTask = await _context.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == result.Id);
        savedTask.Should().NotBeNull();
        savedTask!.Name.Should().Be("New Task");
        savedTask.Project.Name.Should().Be("Test Project");
    }

    [Fact]
    public async Task UpdateTaskAsync_UpdatesTaskSuccessfully()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new GanttTask
        {
            Name = "Original Task",
            ProjectId = project.Id,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1),
            Progress = 0
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        task.Name = "Updated Task";
        task.Progress = 50;
        task.Status = TaskStatus.InProgress;
        task.ActualHours = 20;

        // Act
        var result = await _dataService.UpdateTaskAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Task");
        result.Progress.Should().Be(50);
        result.Status.Should().Be(TaskStatus.InProgress);
        result.ActualHours.Should().Be(20);
        result.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var savedTask = await _context.Tasks.FindAsync(task.Id);
        savedTask!.Name.Should().Be("Updated Task");
        savedTask.Progress.Should().Be(50);
    }

    [Fact]
    public async Task DeleteTaskAsync_DeletesTaskSuccessfully()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new GanttTask
        {
            Name = "Task to Delete",
            ProjectId = project.Id,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        await _dataService.DeleteTaskAsync(task.Id);

        // Assert
        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    #endregion

    #region Hierarchical Task Tests

    [Fact]
    public async Task CreateTaskAsync_WithParent_EstablishesHierarchy()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var parentTask = new GanttTask
        {
            Name = "Parent Task",
            ProjectId = project.Id,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(10)
        };
        _context.Tasks.Add(parentTask);
        await _context.SaveChangesAsync();

        var childTask = new GanttTask
        {
            Name = "Child Task",
            ProjectId = project.Id,
            ParentTaskId = parentTask.Id,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5)
        };

        // Act
        var result = await _dataService.CreateTaskAsync(childTask);

        // Assert
        result.ParentTaskId.Should().Be(parentTask.Id);

        var savedParentTask = await _context.Tasks
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == parentTask.Id);

        savedParentTask!.SubTasks.Should().HaveCount(1);
        savedParentTask.SubTasks.First().Name.Should().Be("Child Task");
    }

    #endregion

    #region Task Dependencies Tests

    [Fact]
    public async Task GetTaskDependenciesAsync_ReturnsCorrectDependencies()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task1 = new GanttTask { Name = "Task 1", ProjectId = project.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        var task2 = new GanttTask { Name = "Task 2", ProjectId = project.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        var task3 = new GanttTask { Name = "Task 3", ProjectId = project.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        
        _context.Tasks.AddRange(task1, task2, task3);
        await _context.SaveChangesAsync();

        // Create dependencies: task3 depends on task1 and task2
        task3.Dependencies.Add(task1);
        task3.Dependencies.Add(task2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetTaskDependenciesAsync(task3.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Task 1");
        result.Should().Contain(t => t.Name == "Task 2");
    }

    [Fact]
    public async Task AddTaskDependencyAsync_CreatesCorrectDependency()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var prerequisiteTask = new GanttTask { Name = "Prerequisite", ProjectId = project.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        var dependentTask = new GanttTask { Name = "Dependent", ProjectId = project.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        
        _context.Tasks.AddRange(prerequisiteTask, dependentTask);
        await _context.SaveChangesAsync();

        // Act
        await _dataService.AddTaskDependencyAsync(dependentTask.Id, prerequisiteTask.Id);

        // Assert
        var dependencies = await _dataService.GetTaskDependenciesAsync(dependentTask.Id);
        dependencies.Should().HaveCount(1);
        dependencies.First().Name.Should().Be("Prerequisite");

        var savedTask = await _context.Tasks
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == dependentTask.Id);

        savedTask!.Dependencies.Should().HaveCount(1);
        savedTask.Dependencies.First().Name.Should().Be("Prerequisite");
    }

    [Fact]
    public async Task RemoveTaskDependencyAsync_RemovesDependencySuccessfully()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var prerequisiteTask = new GanttTask { Name = "Prerequisite", ProjectId = project.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        var dependentTask = new GanttTask { Name = "Dependent", ProjectId = project.Id, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        
        _context.Tasks.AddRange(prerequisiteTask, dependentTask);
        await _context.SaveChangesAsync();

        dependentTask.Dependencies.Add(prerequisiteTask);
        await _context.SaveChangesAsync();

        // Act
        await _dataService.RemoveTaskDependencyAsync(dependentTask.Id, prerequisiteTask.Id);

        // Assert
        var dependencies = await _dataService.GetTaskDependenciesAsync(dependentTask.Id);
        dependencies.Should().BeEmpty();

        var savedTask = await _context.Tasks
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == dependentTask.Id);

        savedTask!.Dependencies.Should().BeEmpty();
    }

    #endregion

    #region Search and Filter Tests

    [Fact]
    public async Task GetTasksByProjectIdAsync_WithDateFilter_ReturnsFilteredTasks()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var filterDate = DateTime.Today.AddDays(5);
        
        var task1 = new GanttTask 
        { 
            Name = "Task 1", 
            ProjectId = project.Id, 
            StartDate = DateTime.Today, 
            EndDate = DateTime.Today.AddDays(3) // Before filter date
        };
        var task2 = new GanttTask 
        { 
            Name = "Task 2", 
            ProjectId = project.Id, 
            StartDate = DateTime.Today.AddDays(4), 
            EndDate = DateTime.Today.AddDays(7) // Spans filter date
        };
        var task3 = new GanttTask 
        { 
            Name = "Task 3", 
            ProjectId = project.Id, 
            StartDate = DateTime.Today.AddDays(10), 
            EndDate = DateTime.Today.AddDays(15) // After filter date
        };
        
        _context.Tasks.AddRange(task1, task2, task3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataService.GetTasksForDateAsync(project.Id, filterDate);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Task 2");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetProjectByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _dataService.GetProjectByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTaskByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _dataService.GetTaskByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProjectAsync_WithInvalidId_DoesNotThrow()
    {
        // Act & Assert
        var act = async () => await _dataService.DeleteProjectAsync(999);
        await act.Should().NotThrowAsync();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}