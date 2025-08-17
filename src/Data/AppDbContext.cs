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
    }
}