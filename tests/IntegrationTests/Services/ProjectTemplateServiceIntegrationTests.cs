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
/// Integration tests for the ProjectTemplateService class.
/// Tests template management, project creation from templates, and template data operations.
/// </summary>
public class ProjectTemplateServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProjectTemplateService _templateService;
    private readonly Mock<ILogger<ProjectTemplateService>> _mockLogger;

    public ProjectTemplateServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<ProjectTemplateService>>();
        _templateService = new ProjectTemplateService(_context, _mockLogger.Object);
    }

    #region Project Template CRUD Tests

    [Fact]
    public async Task GetAllTemplatesAsync_ReturnsAllActiveTemplates()
    {
        // Arrange
        var activeTemplate = new ProjectTemplate
        {
            Name = "Active Template",
            Description = "Active template description",
            Category = "Software",
            IsActive = true,
            IsBuiltIn = false
        };

        var inactiveTemplate = new ProjectTemplate
        {
            Name = "Inactive Template",
            Description = "Inactive template description", 
            Category = "Hardware",
            IsActive = false,
            IsBuiltIn = false
        };

        _context.ProjectTemplates.AddRange(activeTemplate, inactiveTemplate);
        await _context.SaveChangesAsync();

        // Act
        var result = await _templateService.GetAllTemplatesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active Template");
        result.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetTemplateByIdAsync_ReturnsCorrectTemplate()
    {
        // Arrange
        var template = new ProjectTemplate
        {
            Name = "Test Template",
            Description = "Test Description",
            Category = "Testing",
            EstimatedBudget = 25000m,
            EstimatedDurationDays = 30,
            Icon = "🧪"
        };

        _context.ProjectTemplates.Add(template);
        await _context.SaveChangesAsync();

        // Act
        var result = await _templateService.GetTemplateByIdAsync(template.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Template");
        result.Description.Should().Be("Test Description");
        result.Category.Should().Be("Testing");
        result.EstimatedBudget.Should().Be(25000m);
        result.EstimatedDurationDays.Should().Be(30);
        result.Icon.Should().Be("🧪");
    }

    [Fact]
    public async Task GetTemplatesByCategoryAsync_ReturnsFilteredTemplates()
    {
        // Arrange
        var softwareTemplate1 = new ProjectTemplate { Name = "Software 1", Category = "Software", IsActive = true };
        var softwareTemplate2 = new ProjectTemplate { Name = "Software 2", Category = "Software", IsActive = true };
        var hardwareTemplate = new ProjectTemplate { Name = "Hardware 1", Category = "Hardware", IsActive = true };

        _context.ProjectTemplates.AddRange(softwareTemplate1, softwareTemplate2, hardwareTemplate);
        await _context.SaveChangesAsync();

        // Act
        var result = await _templateService.GetTemplatesByCategoryAsync("Software");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.Category.Should().Be("Software"));
        result.Select(t => t.Name).Should().Contain(new[] { "Software 1", "Software 2" });
    }

    [Fact]
    public async Task CreateTemplateAsync_CreatesTemplateSuccessfully()
    {
        // Arrange
        var template = new ProjectTemplate
        {
            Name = "New Template",
            Description = "New template for testing",
            Category = "Custom",
            EstimatedBudget = 15000m,
            EstimatedDurationDays = 45,
            Icon = "🆕",
            IsActive = true,
            IsBuiltIn = false
        };

        // Act
        var result = await _templateService.CreateTemplateAsync(template);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var savedTemplate = await _context.ProjectTemplates.FindAsync(result.Id);
        savedTemplate.Should().NotBeNull();
        savedTemplate!.Name.Should().Be("New Template");
        savedTemplate.IsActive.Should().BeTrue();
        savedTemplate.IsBuiltIn.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTemplateAsync_UpdatesTemplateSuccessfully()
    {
        // Arrange
        var template = new ProjectTemplate
        {
            Name = "Original Name",
            Description = "Original Description",
            Category = "Original",
            EstimatedBudget = 10000m
        };

        _context.ProjectTemplates.Add(template);
        await _context.SaveChangesAsync();

        template.Name = "Updated Name";
        template.Description = "Updated Description";
        template.Category = "Updated";
        template.EstimatedBudget = 20000m;

        // Act
        var result = await _templateService.UpdateTemplateAsync(template);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Category.Should().Be("Updated");
        result.EstimatedBudget.Should().Be(20000m);
        result.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var savedTemplate = await _context.ProjectTemplates.FindAsync(template.Id);
        savedTemplate!.Name.Should().Be("Updated Name");
        savedTemplate.EstimatedBudget.Should().Be(20000m);
    }

    [Fact]
    public async Task DeleteTemplateAsync_DeletesNonBuiltInTemplateSuccessfully()
    {
        // Arrange
        var customTemplate = new ProjectTemplate
        {
            Name = "Custom Template",
            Category = "Custom",
            IsBuiltIn = false
        };

        _context.ProjectTemplates.Add(customTemplate);
        await _context.SaveChangesAsync();

        // Act
        await _templateService.DeleteTemplateAsync(customTemplate.Id);

        // Assert
        var deletedTemplate = await _context.ProjectTemplates.FindAsync(customTemplate.Id);
        deletedTemplate.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTemplateAsync_DoesNotDeleteBuiltInTemplate()
    {
        // Arrange
        var builtInTemplate = new ProjectTemplate
        {
            Name = "Built-in Template",
            Category = "System",
            IsBuiltIn = true
        };

        _context.ProjectTemplates.Add(builtInTemplate);
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _templateService.DeleteTemplateAsync(builtInTemplate.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete built-in templates");

        var existingTemplate = await _context.ProjectTemplates.FindAsync(builtInTemplate.Id);
        existingTemplate.Should().NotBeNull();
    }

    #endregion

    #region Template with Task Templates Tests

    [Fact]
    public async Task GetTemplateWithTasksAsync_ReturnsTemplateWithTasks()
    {
        // Arrange
        var template = new ProjectTemplate
        {
            Name = "Template with Tasks",
            Category = "Complete"
        };

        var taskTemplate1 = new TaskTemplate
        {
            Name = "Task 1",
            Description = "First task",
            EstimatedDurationDays = 5,
            EstimatedHours = 40m,
            Priority = TaskPriority.High,
            Order = 1,
            ProjectTemplate = template
        };

        var taskTemplate2 = new TaskTemplate
        {
            Name = "Task 2", 
            Description = "Second task",
            EstimatedDurationDays = 3,
            EstimatedHours = 24m,
            Priority = TaskPriority.Normal,
            Order = 2,
            ProjectTemplate = template
        };

        template.TaskTemplates.Add(taskTemplate1);
        template.TaskTemplates.Add(taskTemplate2);

        _context.ProjectTemplates.Add(template);
        await _context.SaveChangesAsync();

        // Act
        var result = await _templateService.GetTemplateWithTasksAsync(template.Id);

        // Assert
        result.Should().NotBeNull();
        result!.TaskTemplates.Should().HaveCount(2);
        
        var tasks = result.TaskTemplates.OrderBy(t => t.Order).ToList();
        tasks[0].Name.Should().Be("Task 1");
        tasks[0].EstimatedHours.Should().Be(40m);
        tasks[0].Priority.Should().Be(TaskPriority.High);
        
        tasks[1].Name.Should().Be("Task 2");
        tasks[1].EstimatedHours.Should().Be(24m);
        tasks[1].Priority.Should().Be(TaskPriority.Normal);
    }

    [Fact]
    public async Task CreateProjectFromTemplateAsync_CreatesCompleteProject()
    {
        // Arrange
        var template = new ProjectTemplate
        {
            Name = "Source Template",
            Description = "Template for project creation",
            Category = "Development",
            EstimatedBudget = 50000m,
            EstimatedDurationDays = 60
        };

        var taskTemplate1 = new TaskTemplate
        {
            Name = "Analysis Task",
            Description = "Analyze requirements",
            EstimatedDurationDays = 10,
            EstimatedHours = 80m,
            Priority = TaskPriority.High,
            Order = 1,
            StartOffsetDays = 0,
            ProjectTemplate = template
        };

        var taskTemplate2 = new TaskTemplate
        {
            Name = "Development Task",
            Description = "Implement solution",
            EstimatedDurationDays = 30,
            EstimatedHours = 240m,
            Priority = TaskPriority.Normal,
            Order = 2,
            StartOffsetDays = 10,
            ProjectTemplate = template
        };

        template.TaskTemplates.Add(taskTemplate1);
        template.TaskTemplates.Add(taskTemplate2);

        _context.ProjectTemplates.Add(template);
        await _context.SaveChangesAsync();

        var projectStartDate = DateTime.Today.AddDays(7);

        // Act
        var result = await _templateService.CreateProjectFromTemplateAsync(
            template.Id, 
            "New Project from Template",
            "John Manager",
            projectStartDate);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Project from Template");
        result.ProjectManager.Should().Be("John Manager");
        result.StartDate.Date.Should().Be(projectStartDate.Date);
        result.Budget.Should().Be(50000m);
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify tasks were created
        var createdTasks = await _context.Tasks
            .Where(t => t.ProjectId == result.Id)
            .OrderBy(t => t.StartDate)
            .ToListAsync();

        createdTasks.Should().HaveCount(2);
        
        var analysisTask = createdTasks[0];
        analysisTask.Name.Should().Be("Analysis Task");
        analysisTask.Description.Should().Be("Analyze requirements");
        analysisTask.StartDate.Date.Should().Be(projectStartDate.Date);
        analysisTask.EndDate.Date.Should().Be(projectStartDate.AddDays(10).Date);
        analysisTask.EstimatedHours.Should().Be(80);
        analysisTask.Priority.Should().Be(TaskPriority.High);

        var developmentTask = createdTasks[1];
        developmentTask.Name.Should().Be("Development Task");
        developmentTask.Description.Should().Be("Implement solution");
        developmentTask.StartDate.Date.Should().Be(projectStartDate.AddDays(10).Date);
        developmentTask.EndDate.Date.Should().Be(projectStartDate.AddDays(40).Date);
        developmentTask.EstimatedHours.Should().Be(240);
        developmentTask.Priority.Should().Be(TaskPriority.Normal);
    }

    #endregion

    #region Template Usage Tracking Tests

    [Fact]
    public async Task CreateProjectFromTemplateAsync_IncrementsUsageCount()
    {
        // Arrange
        var template = new ProjectTemplate
        {
            Name = "Usage Template",
            Category = "Test",
            UsageCount = 5
        };

        _context.ProjectTemplates.Add(template);
        await _context.SaveChangesAsync();

        // Act
        await _templateService.CreateProjectFromTemplateAsync(
            template.Id,
            "Test Project",
            "Test Manager",
            DateTime.Today);

        // Assert
        var updatedTemplate = await _context.ProjectTemplates.FindAsync(template.Id);
        updatedTemplate!.UsageCount.Should().Be(6);
    }

    #endregion

    #region Task Template Dependencies Tests

    [Fact]
    public async Task CreateProjectFromTemplateAsync_WithTaskDependencies_CreatesCorrectDependencies()
    {
        // Arrange
        var template = new ProjectTemplate
        {
            Name = "Template with Dependencies",
            Category = "Complex"
        };

        var taskTemplate1 = new TaskTemplate
        {
            Name = "Foundation Task",
            EstimatedDurationDays = 5,
            Order = 1,
            StartOffsetDays = 0,
            ProjectTemplate = template
        };

        var taskTemplate2 = new TaskTemplate
        {
            Name = "Dependent Task",
            EstimatedDurationDays = 3,
            Order = 2,
            StartOffsetDays = 5,
            ProjectTemplate = template
        };

        var dependency = new TaskTemplateDependency
        {
            DependentTaskTemplate = taskTemplate2,
            PrerequisiteTaskTemplate = taskTemplate1,
            DependencyType = DependencyType.FinishToStart,
            LagDays = 1
        };

        taskTemplate2.Dependencies.Add(dependency);
        template.TaskTemplates.Add(taskTemplate1);
        template.TaskTemplates.Add(taskTemplate2);

        _context.ProjectTemplates.Add(template);
        await _context.SaveChangesAsync();

        // Act
        var project = await _templateService.CreateProjectFromTemplateAsync(
            template.Id,
            "Project with Dependencies",
            "Manager",
            DateTime.Today);

        // Assert
        var tasks = await _context.Tasks
            .Include(t => t.Dependencies)
            .Where(t => t.ProjectId == project.Id)
            .OrderBy(t => t.Name)
            .ToListAsync();

        tasks.Should().HaveCount(2);

        var dependentTask = tasks.FirstOrDefault(t => t.Name == "Dependent Task");
        dependentTask.Should().NotBeNull();
        dependentTask!.Dependencies.Should().HaveCount(1);
        dependentTask.Dependencies.First().Name.Should().Be("Foundation Task");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetTemplateByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _templateService.GetTemplateByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateProjectFromTemplateAsync_WithInvalidTemplateId_ThrowsException()
    {
        // Act & Assert
        var act = async () => await _templateService.CreateProjectFromTemplateAsync(
            999,
            "Invalid Template Project",
            "Manager",
            DateTime.Today);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Template not found");
    }

    [Fact]
    public async Task UpdateTemplateAsync_WithNonExistentTemplate_ThrowsException()
    {
        // Arrange
        var nonExistentTemplate = new ProjectTemplate
        {
            Id = 999,
            Name = "Non-existent Template"
        };

        // Act & Assert
        var act = async () => await _templateService.UpdateTemplateAsync(nonExistentTemplate);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Template not found");
    }

    #endregion

    #region Built-in Templates Tests

    [Fact]
    public async Task GetBuiltInTemplatesAsync_ReturnsOnlyBuiltInTemplates()
    {
        // Arrange
        var builtInTemplate = new ProjectTemplate
        {
            Name = "Built-in Template",
            Category = "System",
            IsBuiltIn = true,
            IsActive = true
        };

        var customTemplate = new ProjectTemplate
        {
            Name = "Custom Template",
            Category = "User",
            IsBuiltIn = false,
            IsActive = true
        };

        _context.ProjectTemplates.AddRange(builtInTemplate, customTemplate);
        await _context.SaveChangesAsync();

        // Act
        var result = await _templateService.GetBuiltInTemplatesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Built-in Template");
        result.First().IsBuiltIn.Should().BeTrue();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}