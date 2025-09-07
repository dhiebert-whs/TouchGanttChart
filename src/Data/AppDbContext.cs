using Microsoft.EntityFrameworkCore;
using TouchGanttChart.Models;
using TaskStatus = TouchGanttChart.Models.TaskStatus;

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

            // Add FRC Robot Build Project with Hierarchical Tasks and Dependencies
            var frcProject = new Project
            {
                Name = "FRC Robot Build - Team 1234",
                Description = "FIRST Robotics Competition robot design and build for the 2025 season with swerve drive and 2-stage elevator",
                StartDate = new DateTime(2025, 10, 1),
                EndDate = new DateTime(2025, 11, 12),
                ProjectManager = "Chief Engineer",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                Budget = 15000m,
                ActualCost = 0m,
                Color = "#e74c3c"
            };

            Projects.Add(frcProject);
            await SaveChangesAsync();

            // Create FRC Tasks with Hierarchical Structure and Dependencies
            var frcTasks = new List<GanttTask>();

            // PRIMARY MECHANICAL TASKS
            var buildDrivetrain = new GanttTask
            {
                Name = "Build Drivetrain",
                Description = "Design and build robot drivetrain system",
                StartDate = new DateTime(2025, 10, 6),
                EndDate = new DateTime(2025, 10, 18),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "Mechanical Lead",
                Category = "Mechanical",
                EstimatedHours = 60
            };

            var buildIntake = new GanttTask
            {
                Name = "Build Intake",
                Description = "Design and build game piece intake mechanism",
                StartDate = new DateTime(2025, 10, 8),
                EndDate = new DateTime(2025, 10, 18),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "Mechanical Team",
                Category = "Mechanical",
                EstimatedHours = 50
            };

            var buildElevator = new GanttTask
            {
                Name = "Build 2-Stage Elevator",
                Description = "Design and build two-stage elevator system",
                StartDate = new DateTime(2025, 10, 11),
                EndDate = new DateTime(2025, 10, 25),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "Mechanical Team",
                Category = "Mechanical",
                EstimatedHours = 70
            };

            // Add primary tasks first and save to get IDs
            Tasks.AddRange(new[] { buildDrivetrain, buildIntake, buildElevator });
            await SaveChangesAsync();

            // DRIVETRAIN SUBTASKS (now with valid parent IDs)
            var assembleFrame = new GanttTask
            {
                Name = "Assemble Frame",
                Description = "Build main robot frame structure",
                StartDate = new DateTime(2025, 10, 6),
                EndDate = new DateTime(2025, 10, 10),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = buildDrivetrain.Id,
                Assignee = "Mechanical Team",
                Category = "Mechanical",
                EstimatedHours = 24
            };

            var assembleSwerve1 = new GanttTask
            {
                Name = "Assemble Swerve Module 1",
                Description = "Build first swerve drive module",
                StartDate = new DateTime(2025, 10, 6),
                EndDate = new DateTime(2025, 10, 9),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = buildDrivetrain.Id,
                Assignee = "Mechanical Team",
                Category = "Mechanical",
                EstimatedHours = 8
            };

            var assembleSwerve2 = new GanttTask
            {
                Name = "Assemble Swerve Module 2",
                Description = "Build second swerve drive module",
                StartDate = new DateTime(2025, 10, 6),
                EndDate = new DateTime(2025, 10, 9),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = buildDrivetrain.Id,
                Assignee = "Mechanical Team",
                Category = "Mechanical",
                EstimatedHours = 8
            };

            var assembleSwerve3 = new GanttTask
            {
                Name = "Assemble Swerve Module 3",
                Description = "Build third swerve drive module",
                StartDate = new DateTime(2025, 10, 9),
                EndDate = new DateTime(2025, 10, 12),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = buildDrivetrain.Id,
                Assignee = "Mechanical Team",
                Category = "Mechanical",
                EstimatedHours = 8
            };

            var assembleSwerve4 = new GanttTask
            {
                Name = "Assemble Swerve Module 4",
                Description = "Build fourth swerve drive module",
                StartDate = new DateTime(2025, 10, 9),
                EndDate = new DateTime(2025, 10, 12),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = buildDrivetrain.Id,
                Assignee = "Mechanical Team",
                Category = "Mechanical",
                EstimatedHours = 8
            };

            var mountSwerveModules = new GanttTask
            {
                Name = "Mount Swerve Modules on Frame",
                Description = "Install all four swerve modules onto the frame",
                StartDate = new DateTime(2025, 10, 13),
                EndDate = new DateTime(2025, 10, 18),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = buildDrivetrain.Id,
                Assignee = "Mechanical Team",
                Category = "Mechanical",
                EstimatedHours = 16
            };

            // Add drivetrain subtasks
            Tasks.AddRange(new[] { assembleFrame, assembleSwerve1, assembleSwerve2, assembleSwerve3, assembleSwerve4, mountSwerveModules });
            await SaveChangesAsync();

            // PRIMARY ELECTRICAL TASKS
            var mountRoboRio = new GanttTask
            {
                Name = "Mount RoboRio",
                Description = "Install and secure RoboRio control system",
                StartDate = new DateTime(2025, 10, 13),
                EndDate = new DateTime(2025, 10, 15),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "Electrical Lead",
                Category = "Electrical",
                EstimatedHours = 8
            };

            var mountSparkMaxes = new GanttTask
            {
                Name = "Mount Drivetrain SparkMaxes",
                Description = "Install motor controllers for drivetrain",
                StartDate = new DateTime(2025, 10, 20),
                EndDate = new DateTime(2025, 10, 23),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "Electrical Team",
                Category = "Electrical",
                EstimatedHours = 12
            };

            var connectSwerveModules = new GanttTask
            {
                Name = "Connect Swerve Modules to SparkMaxes",
                Description = "Wire swerve modules to drivetrain controllers",
                StartDate = new DateTime(2025, 10, 24),
                EndDate = new DateTime(2025, 10, 28),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "Electrical Team",
                Category = "Electrical",
                EstimatedHours = 16
            };

            // Add electrical tasks
            Tasks.AddRange(new[] { mountRoboRio, mountSparkMaxes, connectSwerveModules });
            await SaveChangesAsync();

            // PRIMARY PROGRAMMING TASKS
            var drivetrainProgramming = new GanttTask
            {
                Name = "Drivetrain Programming",
                Description = "Program swerve drive control system",
                StartDate = new DateTime(2025, 10, 18),
                EndDate = new DateTime(2025, 10, 26),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "Programming Lead",
                Category = "Software",
                EstimatedHours = 32
            };

            var autonomousProgramming = new GanttTask
            {
                Name = "Autonomous Programming",
                Description = "Develop autonomous routines and path planning",
                StartDate = new DateTime(2025, 10, 21),
                EndDate = new DateTime(2025, 10, 31),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.High,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "Programming Team",
                Category = "Software",
                EstimatedHours = 40
            };

            var visionProgramming = new GanttTask
            {
                Name = "Vision System Programming",
                Description = "Implement computer vision for target tracking",
                StartDate = new DateTime(2025, 10, 24),
                EndDate = new DateTime(2025, 11, 5),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.Normal,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "Programming Team",
                Category = "Software",
                EstimatedHours = 48
            };

            // Add programming tasks
            Tasks.AddRange(new[] { drivetrainProgramming, autonomousProgramming, visionProgramming });
            await SaveChangesAsync();

            // PUBLIC RELATIONS TASKS
            var socialMediaCampaign = new GanttTask
            {
                Name = "Social Media Campaign",
                Description = "Document build progress and engage community",
                StartDate = new DateTime(2025, 10, 3),
                EndDate = new DateTime(2025, 11, 7),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.Normal,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "PR Team",
                Category = "Marketing",
                EstimatedHours = 20
            };

            var sponsorOutreach = new GanttTask
            {
                Name = "Sponsor Outreach",
                Description = "Engage with sponsors and document robot progress",
                StartDate = new DateTime(2025, 10, 6),
                EndDate = new DateTime(2025, 11, 5),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.Normal,
                Progress = 0,
                ProjectId = frcProject.Id,
                Assignee = "PR Lead",
                Category = "Marketing",
                EstimatedHours = 15
            };

            // Add primary PR tasks first and save to get IDs
            Tasks.AddRange(new[] { socialMediaCampaign, sponsorOutreach });
            await SaveChangesAsync();

            // PR SUBTASKS (now with valid parent IDs)
            var createContentPlan = new GanttTask
            {
                Name = "Create Content Plan",
                Description = "Develop social media content strategy and calendar",
                StartDate = new DateTime(2025, 10, 3),
                EndDate = new DateTime(2025, 10, 5),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.Normal,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = socialMediaCampaign.Id,
                Assignee = "PR Team",
                Category = "Marketing",
                EstimatedHours = 6
            };

            var dailyUpdates = new GanttTask
            {
                Name = "Daily Build Updates",
                Description = "Post daily progress updates on social media",
                StartDate = new DateTime(2025, 10, 6),
                EndDate = new DateTime(2025, 11, 7),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.Normal,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = socialMediaCampaign.Id,
                Assignee = "PR Team",
                Category = "Marketing",
                EstimatedHours = 14
            };

            var sponsorMeetings = new GanttTask
            {
                Name = "Weekly Sponsor Meetings",
                Description = "Conduct weekly progress meetings with sponsors",
                StartDate = new DateTime(2025, 10, 6),
                EndDate = new DateTime(2025, 11, 5),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.Normal,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = sponsorOutreach.Id,
                Assignee = "PR Lead",
                Category = "Marketing",
                EstimatedHours = 9
            };

            var sponsorReports = new GanttTask
            {
                Name = "Sponsor Progress Reports",
                Description = "Create detailed progress reports for sponsors",
                StartDate = new DateTime(2025, 10, 13),
                EndDate = new DateTime(2025, 11, 5),
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.Normal,
                Progress = 0,
                ProjectId = frcProject.Id,
                ParentTaskId = sponsorOutreach.Id,
                Assignee = "PR Lead",
                Category = "Marketing",
                EstimatedHours = 6
            };

            // Add PR subtasks
            Tasks.AddRange(new[] { createContentPlan, dailyUpdates, sponsorMeetings, sponsorReports });
            await SaveChangesAsync();

            // Add realistic task dependencies for FRC project
            // These show cross-team dependencies that are common in robotics projects
            
            // Programming depends on electrical completion
            drivetrainProgramming.Dependencies.Add(mountSparkMaxes);
            drivetrainProgramming.Dependencies.Add(connectSwerveModules);
            autonomousProgramming.Dependencies.Add(drivetrainProgramming);
            visionProgramming.Dependencies.Add(drivetrainProgramming);
            
            // Swerve module mounting depends on individual module assembly
            mountSwerveModules.Dependencies.Add(assembleSwerve1);
            mountSwerveModules.Dependencies.Add(assembleSwerve2);
            mountSwerveModules.Dependencies.Add(assembleSwerve3);
            mountSwerveModules.Dependencies.Add(assembleSwerve4);
            
            // Electrical work depends on mechanical frame
            mountRoboRio.Dependencies.Add(assembleFrame);
            mountSparkMaxes.Dependencies.Add(assembleFrame);
            
            // Swerve electrical depends on mounting the modules
            connectSwerveModules.Dependencies.Add(mountSwerveModules);
            connectSwerveModules.Dependencies.Add(mountSparkMaxes);
            
            // PR activities depend on having something to show
            socialMediaCampaign.Dependencies.Add(buildDrivetrain);
            sponsorOutreach.Dependencies.Add(buildDrivetrain);

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
            },
            new ProjectTemplate
            {
                Name = "FRC Robot Build Season",
                Description = "FIRST Robotics Competition robot design and build project for 6-week build season",
                Category = "Robotics",
                EstimatedDurationDays = 42,
                EstimatedBudget = 15000m,
                IsBuiltIn = true,
                Icon = "🤖",
                TaskTemplates = new List<TaskTemplate>
                {
                    // Mechanical Group Tasks
                    new() { Name = "Build Drivetrain", Description = "Design and build robot drivetrain system", EstimatedDurationDays = 12, StartOffsetDays = 5, Order = 1, Priority = TaskPriority.High, DefaultAssigneeRole = "Mechanical Lead", EstimatedHours = 60 },
                    new() { Name = "Build Intake", Description = "Design and build game piece intake mechanism", EstimatedDurationDays = 10, StartOffsetDays = 7, Order = 2, Priority = TaskPriority.High, DefaultAssigneeRole = "Mechanical Team", EstimatedHours = 50 },
                    new() { Name = "Build 2-Stage Elevator", Description = "Design and build two-stage elevator system", EstimatedDurationDays = 14, StartOffsetDays = 10, Order = 3, Priority = TaskPriority.High, DefaultAssigneeRole = "Mechanical Team", EstimatedHours = 70 },
                    
                    // Electrical Group Tasks  
                    new() { Name = "Mount RoboRio", Description = "Install and secure RoboRio control system", EstimatedDurationDays = 2, StartOffsetDays = 12, Order = 4, Priority = TaskPriority.High, DefaultAssigneeRole = "Electrical Lead", EstimatedHours = 8 },
                    new() { Name = "Mount Drivetrain SparkMaxes", Description = "Install motor controllers for drivetrain", EstimatedDurationDays = 3, StartOffsetDays = 17, Order = 5, Priority = TaskPriority.High, DefaultAssigneeRole = "Electrical Team", EstimatedHours = 12 },
                    new() { Name = "Connect Swerve Modules", Description = "Wire swerve modules to drivetrain controllers", EstimatedDurationDays = 4, StartOffsetDays = 20, Order = 6, Priority = TaskPriority.High, DefaultAssigneeRole = "Electrical Team", EstimatedHours = 16 },
                    
                    // Programming Group Tasks
                    new() { Name = "Drivetrain Programming", Description = "Program swerve drive control system", EstimatedDurationDays = 8, StartOffsetDays = 15, Order = 7, Priority = TaskPriority.High, DefaultAssigneeRole = "Programming Lead", EstimatedHours = 32 },
                    new() { Name = "Autonomous Programming", Description = "Develop autonomous routines and path planning", EstimatedDurationDays = 10, StartOffsetDays = 18, Order = 8, Priority = TaskPriority.High, DefaultAssigneeRole = "Programming Team", EstimatedHours = 40 },
                    new() { Name = "Vision System Programming", Description = "Implement computer vision for target tracking", EstimatedDurationDays = 12, StartOffsetDays = 20, Order = 9, Priority = TaskPriority.Normal, DefaultAssigneeRole = "Programming Team", EstimatedHours = 48 },
                    
                    // Public Relations Tasks
                    new() { Name = "Social Media Campaign", Description = "Document build progress and engage community", EstimatedDurationDays = 35, StartOffsetDays = 2, Order = 10, Priority = TaskPriority.Normal, DefaultAssigneeRole = "PR Team", EstimatedHours = 20 },
                    new() { Name = "Sponsor Outreach", Description = "Engage with sponsors and document robot progress", EstimatedDurationDays = 30, StartOffsetDays = 5, Order = 11, Priority = TaskPriority.Normal, DefaultAssigneeRole = "PR Lead", EstimatedHours = 15 }
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