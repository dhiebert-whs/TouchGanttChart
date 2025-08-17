using Microsoft.EntityFrameworkCore;
using TouchGanttChart.Models;

namespace TouchGanttChart.Data;

/// <summary>
/// Entity Framework database context for the Touch Gantt Chart application.
/// Configured for SQLite with touch-optimized data access patterns.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the Projects DbSet.
    /// </summary>
    public DbSet<Project> Projects { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Tasks DbSet.
    /// </summary>
    public DbSet<GanttTask> Tasks { get; set; } = null!;

    /// <summary>
    /// Gets or sets the project templates in the application.
    /// </summary>
    public DbSet<ProjectTemplate> ProjectTemplates { get; set; } = null!;

    /// <summary>
    /// Gets or sets the task templates in the application.
    /// </summary>
    public DbSet<TaskTemplate> TaskTemplates { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the AppDbContext class.
    /// </summary>
    public AppDbContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the AppDbContext class with options.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the database connection and options.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=ganttchart.db", options =>
            {
                options.CommandTimeout(30);
            });
            
            // Performance optimizations
            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.EnableServiceProviderCaching();
            optionsBuilder.EnableDetailedErrors(false);
            
            // SQLite-specific optimizations
            optionsBuilder.UseSqlite(sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(30);
            });
        }
    }

    /// <summary>
    /// Configures the model relationships and constraints.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureProject(modelBuilder);
        ConfigureGanttTask(modelBuilder);
        ConfigureTaskDependencies(modelBuilder);
        ConfigureProjectTemplate(modelBuilder);
        ConfigureTaskTemplate(modelBuilder);
        ConfigureTaskTemplateDependencies(modelBuilder);
        
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Configures the Project entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    private static void ConfigureProject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            
            // Required properties
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.ProjectManager).HasMaxLength(100);
            entity.Property(p => p.Color).HasMaxLength(7).HasDefaultValue("#3498db");
            
            // Decimal precision for budget
            entity.Property(p => p.Budget).HasPrecision(18, 2);
            entity.Property(p => p.ActualCost).HasPrecision(18, 2);
            
            // Indexes for performance
            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.StartDate);
            entity.HasIndex(p => p.EndDate);
            entity.HasIndex(p => p.IsArchived);
            entity.HasIndex(p => p.CreatedDate);
            
            // Relationships
            entity.HasMany(p => p.Tasks)
                  .WithOne(t => t.Project)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the GanttTask entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    private static void ConfigureGanttTask(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GanttTask>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedOnAdd();
            
            // Required properties
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(1000);
            entity.Property(t => t.Assignee).HasMaxLength(100);
            entity.Property(t => t.Category).HasMaxLength(50).HasDefaultValue("General");
            
            // Decimal precision for hours
            entity.Property(t => t.EstimatedHours).HasPrecision(10, 2);
            entity.Property(t => t.ActualHours).HasPrecision(10, 2);
            
            // Progress constraint
            entity.Property(t => t.Progress).HasDefaultValue(0);
            
            // Indexes for performance
            entity.HasIndex(t => t.Name);
            entity.HasIndex(t => t.StartDate);
            entity.HasIndex(t => t.EndDate);
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.Priority);
            entity.HasIndex(t => t.Progress);
            entity.HasIndex(t => t.Assignee);
            entity.HasIndex(t => t.Category);
            entity.HasIndex(t => t.ParentTaskId);
            entity.HasIndex(t => t.ProjectId);
            entity.HasIndex(t => t.CreatedDate);
            
            // Self-referencing relationship for parent-child tasks
            entity.HasOne(t => t.ParentTask)
                  .WithMany(t => t.SubTasks)
                  .HasForeignKey(t => t.ParentTaskId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Exclude calculated properties from database
            entity.Ignore(t => t.Duration);
            entity.Ignore(t => t.IsOverdue);
            entity.Ignore(t => t.IsMilestone);
            entity.Ignore(t => t.HasSubTasks);
            entity.Ignore(t => t.ProgressDisplay);
            entity.Ignore(t => t.DurationDisplay);
            entity.Ignore(t => t.IsSelected);
            entity.Ignore(t => t.IsExpanded);
        });
    }

    /// <summary>
    /// Configures task dependency relationships.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    private static void ConfigureTaskDependencies(ModelBuilder modelBuilder)
    {
        // Many-to-many relationship for task dependencies
        modelBuilder.Entity<GanttTask>()
            .HasMany(t => t.DependentTasks)
            .WithMany(t => t.Dependencies)
            .UsingEntity(
                "TaskDependencies",
                l => l.HasOne(typeof(GanttTask)).WithMany().HasForeignKey("DependentTaskId").HasPrincipalKey(nameof(GanttTask.Id)),
                r => r.HasOne(typeof(GanttTask)).WithMany().HasForeignKey("PrerequisiteTaskId").HasPrincipalKey(nameof(GanttTask.Id)),
                j => j.HasKey("DependentTaskId", "PrerequisiteTaskId"));
    }

    /// <summary>
    /// Applies SQLite performance optimizations.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OptimizeDatabaseAsync()
    {
        await Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;");
        await Database.ExecuteSqlRawAsync("PRAGMA synchronous = NORMAL;");
        await Database.ExecuteSqlRawAsync("PRAGMA temp_store = MEMORY;");
        await Database.ExecuteSqlRawAsync("PRAGMA mmap_size = 268435456;");
        await Database.ExecuteSqlRawAsync("PRAGMA cache_size = 10000;");
        await Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
    }

    /// <summary>
    /// Seeds the database with sample data for development.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SeedDataAsync()
    {
        if (!await Projects.AnyAsync())
        {
            var sampleProject = new Project
            {
                Name = "Touch Gantt Chart Development",
                Description = "Development of a touch-optimized Gantt chart application using WPF and .NET 9.0",
                ProjectManager = "Development Team",
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today.AddDays(120),
                Status = Models.TaskStatus.InProgress,
                Priority = Models.TaskPriority.High,
                Budget = 50000m,
                Color = "#2c3e50"
            };

            Projects.Add(sampleProject);
            await SaveChangesAsync();

            // Add sample tasks
            var tasks = new List<GanttTask>
            {
                new()
                {
                    Name = "Project Planning",
                    Description = "Initial project planning and requirements gathering",
                    StartDate = DateTime.Today.AddDays(-30),
                    EndDate = DateTime.Today.AddDays(-25),
                    Status = Models.TaskStatus.Completed,
                    Priority = Models.TaskPriority.High,
                    Progress = 100,
                    ProjectId = sampleProject.Id,
                    Assignee = "Project Manager",
                    EstimatedHours = 40,
                    ActualHours = 42
                },
                new()
                {
                    Name = "Database Design",
                    Description = "Design Entity Framework models and database schema",
                    StartDate = DateTime.Today.AddDays(-20),
                    EndDate = DateTime.Today.AddDays(-15),
                    Status = Models.TaskStatus.Completed,
                    Priority = Models.TaskPriority.High,
                    Progress = 100,
                    ProjectId = sampleProject.Id,
                    Assignee = "Backend Developer",
                    EstimatedHours = 32,
                    ActualHours = 35
                },
                new()
                {
                    Name = "UI Framework Setup",
                    Description = "Implement MVVM foundation and touch-optimized UI framework",
                    StartDate = DateTime.Today.AddDays(-10),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = Models.TaskStatus.InProgress,
                    Priority = Models.TaskPriority.High,
                    Progress = 60,
                    ProjectId = sampleProject.Id,
                    Assignee = "Frontend Developer",
                    EstimatedHours = 80,
                    ActualHours = 45
                },
                new()
                {
                    Name = "Gantt Chart Visualization",
                    Description = "Implement custom Gantt chart canvas with timeline rendering",
                    StartDate = DateTime.Today.AddDays(5),
                    EndDate = DateTime.Today.AddDays(25),
                    Status = Models.TaskStatus.NotStarted,
                    Priority = Models.TaskPriority.High,
                    Progress = 0,
                    ProjectId = sampleProject.Id,
                    Assignee = "Frontend Developer",
                    EstimatedHours = 100
                },
                new()
                {
                    Name = "Touch Gesture Implementation",
                    Description = "Add multi-touch support for pan, zoom, and task manipulation",
                    StartDate = DateTime.Today.AddDays(20),
                    EndDate = DateTime.Today.AddDays(40),
                    Status = Models.TaskStatus.NotStarted,
                    Priority = Models.TaskPriority.High,
                    Progress = 0,
                    ProjectId = sampleProject.Id,
                    Assignee = "Frontend Developer",
                    EstimatedHours = 60
                },
                new()
                {
                    Name = "Testing & Optimization",
                    Description = "Comprehensive testing and performance optimization",
                    StartDate = DateTime.Today.AddDays(80),
                    EndDate = DateTime.Today.AddDays(110),
                    Status = Models.TaskStatus.NotStarted,
                    Priority = Models.TaskPriority.Normal,
                    Progress = 0,
                    ProjectId = sampleProject.Id,
                    Assignee = "QA Team",
                    EstimatedHours = 120
                }
            };

            Tasks.AddRange(tasks);
            await SaveChangesAsync();
        }

        // Seed project templates
        if (!await ProjectTemplates.AnyAsync())
        {
            await SeedProjectTemplatesAsync();
        }
    }

    /// <summary>
    /// Seeds sample project templates for development.
    /// </summary>
    private async Task SeedProjectTemplatesAsync()
    {
        var templates = new[]
        {
            new ProjectTemplate
            {
                Name = "Software Development Project",
                Description = "Standard software development project with planning, development, testing, and deployment phases",
                Category = "Software Development",
                EstimatedDurationDays = 90,
                EstimatedBudget = 75000m,
                IsBuiltIn = true,
                Icon = "💻",
                TaskTemplates = new List<TaskTemplate>
                {
                    new() { Name = "Project Planning", Description = "Initial project planning and requirements gathering", EstimatedDurationDays = 5, StartOffsetDays = 0, Order = 1, Priority = TaskPriority.High, DefaultAssigneeRole = "Project Manager", EstimatedHours = 40 },
                    new() { Name = "System Design", Description = "Design system architecture and technical specifications", EstimatedDurationDays = 10, StartOffsetDays = 5, Order = 2, Priority = TaskPriority.High, DefaultAssigneeRole = "Architect", EstimatedHours = 80 },
                    new() { Name = "Development Phase 1", Description = "Core functionality development", EstimatedDurationDays = 30, StartOffsetDays = 15, Order = 3, Priority = TaskPriority.High, DefaultAssigneeRole = "Developer", EstimatedHours = 240 },
                    new() { Name = "Testing Phase 1", Description = "Unit and integration testing", EstimatedDurationDays = 10, StartOffsetDays = 45, Order = 4, Priority = TaskPriority.High, DefaultAssigneeRole = "QA Engineer", EstimatedHours = 80 },
                    new() { Name = "Development Phase 2", Description = "Additional features and refinements", EstimatedDurationDays = 20, StartOffsetDays = 55, Order = 5, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Developer", EstimatedHours = 160 },
                    new() { Name = "Final Testing", Description = "System testing and user acceptance testing", EstimatedDurationDays = 10, StartOffsetDays = 75, Order = 6, Priority = TaskPriority.High, DefaultAssigneeRole = "QA Engineer", EstimatedHours = 80 },
                    new() { Name = "Deployment", Description = "Production deployment and go-live", EstimatedDurationDays = 5, StartOffsetDays = 85, Order = 7, Priority = TaskPriority.Critical, DefaultAssigneeRole = "DevOps Engineer", EstimatedHours = 40, IsMilestone = true }
                }
            },
            new ProjectTemplate
            {
                Name = "Marketing Campaign",
                Description = "Complete marketing campaign from strategy to execution and analysis",
                Category = "Marketing",
                EstimatedDurationDays = 60,
                EstimatedBudget = 25000m,
                IsBuiltIn = true,
                Icon = "📈",
                TaskTemplates = new List<TaskTemplate>
                {
                    new() { Name = "Market Research", Description = "Conduct market analysis and competitor research", EstimatedDurationDays = 7, StartOffsetDays = 0, Order = 1, Priority = TaskPriority.High, DefaultAssigneeRole = "Marketing Analyst", EstimatedHours = 56 },
                    new() { Name = "Strategy Development", Description = "Develop campaign strategy and messaging", EstimatedDurationDays = 5, StartOffsetDays = 7, Order = 2, Priority = TaskPriority.High, DefaultAssigneeRole = "Marketing Manager", EstimatedHours = 40 },
                    new() { Name = "Creative Development", Description = "Create campaign assets and materials", EstimatedDurationDays = 14, StartOffsetDays = 12, Order = 3, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Creative Director", EstimatedHours = 112 },
                    new() { Name = "Channel Setup", Description = "Set up advertising channels and platforms", EstimatedDurationDays = 3, StartOffsetDays = 26, Order = 4, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Digital Marketer", EstimatedHours = 24 },
                    new() { Name = "Campaign Launch", Description = "Launch marketing campaign across all channels", EstimatedDurationDays = 1, StartOffsetDays = 29, Order = 5, Priority = TaskPriority.Critical, DefaultAssigneeRole = "Marketing Manager", EstimatedHours = 8, IsMilestone = true },
                    new() { Name = "Campaign Monitoring", Description = "Monitor campaign performance and optimization", EstimatedDurationDays = 21, StartOffsetDays = 30, Order = 6, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Digital Marketer", EstimatedHours = 84 },
                    new() { Name = "Analysis & Reporting", Description = "Analyze results and create performance report", EstimatedDurationDays = 7, StartOffsetDays = 51, Order = 7, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Marketing Analyst", EstimatedHours = 35 }
                }
            },
            new ProjectTemplate
            {
                Name = "Event Planning",
                Description = "Comprehensive event planning template for conferences, meetings, and corporate events",
                Category = "Event Management",
                EstimatedDurationDays = 45,
                EstimatedBudget = 30000m,
                IsBuiltIn = true,
                Icon = "🎉",
                TaskTemplates = new List<TaskTemplate>
                {
                    new() { Name = "Event Concept", Description = "Define event concept, goals, and requirements", EstimatedDurationDays = 3, StartOffsetDays = 0, Order = 1, Priority = TaskPriority.High, DefaultAssigneeRole = "Event Manager", EstimatedHours = 24 },
                    new() { Name = "Venue Selection", Description = "Research and book appropriate venue", EstimatedDurationDays = 7, StartOffsetDays = 3, Order = 2, Priority = TaskPriority.High, DefaultAssigneeRole = "Event Coordinator", EstimatedHours = 35 },
                    new() { Name = "Vendor Management", Description = "Select and coordinate with vendors (catering, AV, etc.)", EstimatedDurationDays = 10, StartOffsetDays = 10, Order = 3, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Event Coordinator", EstimatedHours = 50 },
                    new() { Name = "Marketing & Promotion", Description = "Create promotional materials and marketing campaign", EstimatedDurationDays = 14, StartOffsetDays = 20, Order = 4, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Marketing Specialist", EstimatedHours = 70 },
                    new() { Name = "Registration Setup", Description = "Set up registration system and attendee management", EstimatedDurationDays = 3, StartOffsetDays = 25, Order = 5, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Event Coordinator", EstimatedHours = 20 },
                    new() { Name = "Final Preparations", Description = "Finalize all event details and contingency planning", EstimatedDurationDays = 5, StartOffsetDays = 35, Order = 6, Priority = TaskPriority.High, DefaultAssigneeRole = "Event Manager", EstimatedHours = 40 },
                    new() { Name = "Event Execution", Description = "Execute the event and manage on-site operations", EstimatedDurationDays = 1, StartOffsetDays = 40, Order = 7, Priority = TaskPriority.Critical, DefaultAssigneeRole = "Event Manager", EstimatedHours = 12, IsMilestone = true },
                    new() { Name = "Post-Event Follow-up", Description = "Post-event analysis, feedback collection, and reporting", EstimatedDurationDays = 3, StartOffsetDays = 41, Order = 8, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Event Manager", EstimatedHours = 20 }
                }
            }
        };

        ProjectTemplates.AddRange(templates);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Configures the ProjectTemplate entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    private static void ConfigureProjectTemplate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectTemplate>(entity =>
        {
            entity.HasKey(pt => pt.Id);
            entity.Property(pt => pt.Id).ValueGeneratedOnAdd();
            
            // Required properties
            entity.Property(pt => pt.Name).IsRequired().HasMaxLength(200);
            entity.Property(pt => pt.Description).HasMaxLength(1000);
            entity.Property(pt => pt.Category).IsRequired().HasMaxLength(100);
            entity.Property(pt => pt.Icon).HasMaxLength(50).HasDefaultValue("📋");
            
            // Decimal precision
            entity.Property(pt => pt.EstimatedBudget).HasPrecision(18, 2);
            
            // Indexes for performance
            entity.HasIndex(pt => pt.Name);
            entity.HasIndex(pt => pt.Category);
            entity.HasIndex(pt => pt.IsActive);
            entity.HasIndex(pt => pt.IsBuiltIn);
            entity.HasIndex(pt => pt.CreatedDate);
            
            // Relationships
            entity.HasMany(pt => pt.TaskTemplates)
                  .WithOne(tt => tt.ProjectTemplate)
                  .HasForeignKey(tt => tt.ProjectTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the TaskTemplate entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    private static void ConfigureTaskTemplate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskTemplate>(entity =>
        {
            entity.HasKey(tt => tt.Id);
            entity.Property(tt => tt.Id).ValueGeneratedOnAdd();
            
            // Required properties
            entity.Property(tt => tt.Name).IsRequired().HasMaxLength(200);
            entity.Property(tt => tt.Description).HasMaxLength(1000);
            entity.Property(tt => tt.DefaultAssigneeRole).HasMaxLength(100);
            
            // Decimal precision
            entity.Property(tt => tt.EstimatedHours).HasPrecision(8, 2);
            
            // Enum conversions
            entity.Property(tt => tt.Priority).HasConversion<string>();
            
            // Indexes for performance
            entity.HasIndex(tt => tt.ProjectTemplateId);
            entity.HasIndex(tt => tt.ParentTaskTemplateId);
            entity.HasIndex(tt => tt.Order);
            entity.HasIndex(tt => tt.IsMilestone);
            entity.HasIndex(tt => tt.IsCriticalPath);
            
            // Self-referencing relationship for parent-child hierarchy
            entity.HasOne(tt => tt.ParentTaskTemplate)
                  .WithMany(tt => tt.ChildTaskTemplates)
                  .HasForeignKey(tt => tt.ParentTaskTemplateId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configures the TaskTemplateDependency entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    private static void ConfigureTaskTemplateDependencies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskTemplateDependency>(entity =>
        {
            // Composite key
            entity.HasKey(ttd => new { ttd.DependentTaskTemplateId, ttd.PrerequisiteTaskTemplateId });
            
            // Enum conversion
            entity.Property(ttd => ttd.DependencyType).HasConversion<string>();
            
            // Configure relationships
            entity.HasOne(ttd => ttd.DependentTaskTemplate)
                  .WithMany(tt => tt.Dependencies)
                  .HasForeignKey(ttd => ttd.DependentTaskTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ttd => ttd.PrerequisiteTaskTemplate)
                  .WithMany(tt => tt.Dependents)
                  .HasForeignKey(ttd => ttd.PrerequisiteTaskTemplateId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes for performance
            entity.HasIndex(ttd => ttd.DependentTaskTemplateId);
            entity.HasIndex(ttd => ttd.PrerequisiteTaskTemplateId);
        });
    }
}