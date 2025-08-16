# Touch-Optimized WPF Gantt Chart Development Plan

Based on comprehensive research into modern WPF development practices for 2025, this implementation plan provides everything needed to start building a touch-optimized C# WPF Gantt chart application with VSCode and Claudecode integration.

## Project setup and environment configuration

The modern WPF development stack for 2025 centers around **CommunityToolkit.Mvvm** for MVVM implementation, **Microsoft.Extensions.DependencyInjection** for dependency injection, and **.NET 8** with SDK-style project files. VSCode requires specific extensions and configuration to provide effective WPF development capabilities, though it lacks the visual XAML designer found in Visual Studio.

### Prerequisites and tool installation

Install the essential development tools:

```bash
# Install .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# Install Visual Studio Code  
winget install Microsoft.VisualStudioCode

# Install Git for version control
winget install Git.Git
```

### VSCode extension setup

Install these essential extensions through VSCode marketplace:

1. **C# Dev Kit** (`ms-dotnettools.csdevkit`) - Core C# development with debugging and IntelliSense
2. **Uno Platform** (`unoplatform.vscode`) - Enhanced XAML support and Hot Reload
3. **NuGet Package Manager** (`jmrog.vscode-nuget-package-manager`) - Package management
4. **XML Tools** (`DotJoshJohnson.xml`) - XAML/XML editing support

## Project creation and structure

Create the project foundation with modern architecture patterns:

```bash
# Create project directory and initialize
mkdir TouchGanttChart
cd TouchGanttChart
dotnet new wpf --framework net8.0-windows --name TouchGanttChart
cd TouchGanttChart
```

### Modern project architecture

Implement the recommended folder structure for AI-assisted development:

```
TouchGanttChart/
├── .ai/                          # AI configuration
│   ├── CLAUDE.md                 # AI assistant rules
│   ├── context.md                # Project context
│   └── examples/                 # Code patterns
├── .vscode/                      # VSCode configuration
│   ├── launch.json
│   ├── tasks.json
│   └── settings.json
├── src/
│   ├── Models/                   # Domain models
│   │   ├── GanttTask.cs
│   │   ├── Project.cs
│   │   └── TimelineEvent.cs
│   ├── ViewModels/               # MVVM view models
│   │   ├── Base/
│   │   │   └── ViewModelBase.cs
│   │   ├── MainWindowViewModel.cs
│   │   ├── GanttChartViewModel.cs
│   │   └── TaskEditorViewModel.cs
│   ├── Views/                    # XAML views
│   │   ├── MainWindow.xaml
│   │   ├── Controls/
│   │   │   ├── TouchGanttChart.xaml
│   │   │   └── TaskEditor.xaml
│   │   └── Dialogs/
│   ├── Services/                 # Business services
│   │   ├── Interfaces/
│   │   │   ├── IDataService.cs
│   │   │   ├── IPdfExportService.cs
│   │   │   └── ITouchGestureService.cs
│   │   └── Implementations/
│   ├── Data/                     # Data access
│   │   ├── AppDbContext.cs
│   │   └── Repositories/
│   ├── Resources/                # UI resources
│   │   ├── Styles/
│   │   ├── Templates/
│   │   ├── Colors.xaml
│   │   └── TouchStyles.xaml
│   └── Behaviors/                # Touch behaviors
├── tests/                        # Test projects
│   ├── UnitTests/
│   └── IntegrationTests/
├── docs/                         # Documentation
└── examples/                     # Code examples
```

## Dependencies and package configuration

Configure the project with modern dependencies optimized for touch-enabled WPF applications:

### Core project file setup

Replace the generated `.csproj` with this modern configuration:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
    <ApplicationIcon>Resources\Images\app.ico</ApplicationIcon>
    <PlatformTarget>x64</PlatformTarget>
    <DebugType>portable</DebugType>
    <XamlDebuggingInformation Condition="'$(Configuration)' == 'Debug'">True</XamlDebuggingInformation>
  </PropertyGroup>

  <!-- Core Dependencies -->
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
  </ItemGroup>

  <!-- Database Dependencies -->
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.8" />
  </ItemGroup>

  <!-- PDF Export Dependencies -->
  <ItemGroup>
    <PackageReference Include="IronPdf" Version="2024.8.3" />
  </ItemGroup>

  <!-- Touch and UI Dependencies -->
  <ItemGroup>
    <PackageReference Include="MahApps.Metro" Version="2.4.10" />
  </ItemGroup>

  <!-- Testing Dependencies -->
  <ItemGroup>
    <PackageReference Include="xUnit" Version="2.4.2" />
    <PackageReference Include="xUnit.runner.visualstudio" Version="2.4.5" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  </ItemGroup>

  <ItemGroup>
    <Resource Include="Resources\Images\**\*" />
    <Page Include="Resources\Styles\**\*.xaml" />
    <Page Include="Resources\Templates\**\*.xaml" />
  </ItemGroup>

</Project>
```

### Package installation commands

Execute these commands to install the dependencies:

```bash
# Install core packages
dotnet add package CommunityToolkit.Mvvm --version 8.4.0
dotnet add package Microsoft.Extensions.Hosting --version 8.0.0
dotnet add package Microsoft.Extensions.DependencyInjection --version 8.0.0

# Install database packages
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 9.0.8
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.8

# Install PDF and UI packages
dotnet add package IronPdf --version 2024.8.3
dotnet add package MahApps.Metro --version 2.4.10

# Install testing packages
dotnet add package xUnit --version 2.4.2
dotnet add package xUnit.runner.visualstudio --version 2.4.5
```

## VSCode configuration files

Configure VSCode for optimal WPF development experience:

### Launch configuration (.vscode/launch.json)

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Debug WPF App",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net8.0-windows/TouchGanttChart.exe",
            "args": [],
            "cwd": "${workspaceFolder}",
            "console": "internalConsole",
            "stopAtEntry": false,
            "requireExactSource": false
        }
    ]
}
```

### Build tasks (.vscode/tasks.json)

```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/TouchGanttChart.csproj",
                "/property:GenerateFullPaths=true",
                "/consoleloggerparameters:NoSummary"
            ],
            "problemMatcher": "$msCompile",
            "group": {
                "kind": "build",
                "isDefault": true
            },
            "presentation": {
                "reveal": "silent"
            }
        },
        {
            "label": "publish",
            "command": "dotnet",
            "type": "process",
            "args": [
                "publish",
                "${workspaceFolder}/TouchGanttChart.csproj",
                "/property:GenerateFullPaths=true",
                "/consoleloggerparameters:NoSummary"
            ],
            "problemMatcher": "$msCompile"
        }
    ]
}
```

### Workspace settings (.vscode/settings.json)

```json
{
    "dotnet.defaultSolution": "TouchGanttChart.sln",
    "dotnet.completion.showCompletionItemsFromUnimportedNamespaces": true,
    "dotnet.inlayHints.enableInlayHintsForParameters": true,
    "dotnet.inlayHints.enableInlayHintsForTypes": true,
    "editor.formatOnSave": true,
    "editor.formatOnType": true,
    "editor.codeActionsOnSave": {
        "source.organizeImports": true,
        "source.fixAll": true
    },
    "files.exclude": {
        "**/bin": true,
        "**/obj": true,
        "**/.vs": true
    },
    "search.exclude": {
        "**/bin": true,
        "**/obj": true,
        "**/.vs": true
    },
    "xml.fileAssociations": [
        {
            "pattern": "**/*.xaml",
            "systemId": "xaml"
        }
    ],
    "files.associations": {
        "*.xaml": "xml"
    }
}
```

## Core application templates

Create the foundational application structure with modern patterns:

### Application entry point (Program.cs)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TouchGanttChart.ViewModels;
using TouchGanttChart.Views;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.Services.Implementations;
using TouchGanttChart.Data;

namespace TouchGanttChart;

public class Program
{
    [STAThread]
    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                // Register ViewModels
                services.AddSingleton<MainWindowViewModel>();
                services.AddTransient<GanttChartViewModel>();
                services.AddTransient<TaskEditorViewModel>();
                
                // Register Views
                services.AddSingleton<MainWindow>();
                
                // Register Services
                services.AddSingleton<IDataService, DataService>();
                services.AddSingleton<IPdfExportService, PdfExportService>();
                services.AddTransient<ITouchGestureService, TouchGestureService>();
                
                // Register DbContext
                services.AddDbContext<AppDbContext>();
            })
            .Build();

        var app = new App();
        app.InitializeComponent();
        
        // Get and show main window
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        app.MainWindow = mainWindow;
        mainWindow.Show();
        
        app.Run();
    }
}
```

### Modern App.xaml configuration

```xml
<Application x:Class="TouchGanttChart.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- MahApps.Metro resources -->
                <ResourceDictionary Source="pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml" />
                
                <!-- Application resources -->
                <ResourceDictionary Source="/Resources/Colors.xaml"/>
                <ResourceDictionary Source="/Resources/TouchStyles.xaml"/>
                <ResourceDictionary Source="/Resources/Styles/ButtonStyles.xaml"/>
                <ResourceDictionary Source="/Resources/Templates/DataTemplates.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### ViewModelBase implementation

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace TouchGanttChart.ViewModels.Base;

/// <summary>
/// Base class for all view models providing common MVVM functionality.
/// AI Context: Uses CommunityToolkit.Mvvm for property change notification and commands.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasErrors;

    /// <summary>
    /// Called when the view model is initialized.
    /// Override in derived classes for custom initialization logic.
    /// </summary>
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the view model is being closed.
    /// Override in derived classes for cleanup logic.
    /// </summary>
    public virtual Task CleanupAsync()
    {
        return Task.CompletedTask;
    }

    protected void SetError(string message)
    {
        StatusMessage = message;
        HasErrors = true;
    }

    protected void ClearError()
    {
        StatusMessage = string.Empty;
        HasErrors = false;
    }
}
```

## Domain models and data context

Establish the data foundation for the Gantt chart application:

### Core domain models

**GanttTask model (src/Models/GanttTask.cs):**

```csharp
using System.ComponentModel.DataAnnotations;

namespace TouchGanttChart.Models;

/// <summary>
/// Represents a task in the Gantt chart.
/// AI Context: Core domain entity with validation attributes for touch input.
/// </summary>
public class GanttTask
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Task name is required")]
    [StringLength(200, ErrorMessage = "Task name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; } = DateTime.Today;
    
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);
    
    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
    public int Progress { get; set; }
    
    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
    
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    
    // Navigation properties
    public int? ParentTaskId { get; set; }
    public GanttTask? ParentTask { get; set; }
    public List<GanttTask> SubTasks { get; set; } = new();
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    // Touch-friendly properties
    public bool IsSelected { get; set; }
    public bool IsExpanded { get; set; } = true;
    
    // Calculated properties
    public TimeSpan Duration => EndDate - StartDate;
    public bool IsOverdue => EndDate < DateTime.Today && Status != TaskStatus.Completed;
}

public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed,
    OnHold,
    Cancelled
}

public enum TaskPriority
{
    Low,
    Normal,
    High,
    Critical
}
```

**Project model (src/Models/Project.cs):**

```csharp
using System.ComponentModel.DataAnnotations;

namespace TouchGanttChart.Models;

/// <summary>
/// Represents a project containing multiple Gantt tasks.
/// AI Context: Top-level container for organizing tasks with metadata.
/// </summary>
public class Project
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastModifiedDate { get; set; } = DateTime.Now;
    
    // Navigation properties
    public List<GanttTask> Tasks { get; set; } = new();
    
    // Calculated properties  
    public int TaskCount => Tasks.Count;
    public int CompletedTaskCount => Tasks.Count(t => t.Status == TaskStatus.Completed);
    public double ProgressPercentage => TaskCount > 0 ? (double)CompletedTaskCount / TaskCount * 100 : 0;
}
```

### Database context setup

**Entity Framework context (src/Data/AppDbContext.cs):**

```csharp
using Microsoft.EntityFrameworkCore;
using TouchGanttChart.Models;

namespace TouchGanttChart.Data;

/// <summary>
/// Entity Framework database context for the Gantt chart application.
/// AI Context: Configured for SQLite with touch-optimized data access patterns.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<GanttTask> Tasks { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=ganttchart.db");
        optionsBuilder.EnableSensitiveDataLogging(false);
        optionsBuilder.EnableServiceProviderCaching();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Project entity
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.HasIndex(p => p.Name);
            
            // Configure one-to-many relationship
            entity.HasMany(p => p.Tasks)
                  .WithOne(t => t.Project)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure GanttTask entity
        modelBuilder.Entity<GanttTask>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(1000);
            entity.HasIndex(t => t.StartDate);
            entity.HasIndex(t => t.Status);
            
            // Configure self-referencing relationship for subtasks
            entity.HasOne(t => t.ParentTask)
                  .WithMany(t => t.SubTasks)
                  .HasForeignKey(t => t.ParentTaskId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
```

## Touch-optimized UI templates

Create touch-friendly user interface components:

### Touch styles resource dictionary

**Touch-optimized styles (src/Resources/TouchStyles.xaml):**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Touch-friendly button style -->
    <Style x:Key="TouchButtonStyle" TargetType="Button">
        <Setter Property="MinHeight" Value="44"/>
        <Setter Property="MinWidth" Value="44"/>
        <Setter Property="Margin" Value="8"/>
        <Setter Property="Padding" Value="16,12"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}" 
                            CornerRadius="8"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}">
                        <Grid>
                            <!-- Transparent touch area for larger hit target -->
                            <Rectangle Fill="Transparent" 
                                     Margin="-12"/>
                            <ContentPresenter HorizontalAlignment="Center" 
                                            VerticalAlignment="Center"/>
                        </Grid>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter Property="Background" Value="{StaticResource PrimaryLightBrush}"/>
                        </Trigger>
                        <Trigger Property="IsPressed" Value="True">
                            <Setter Property="Background" Value="{StaticResource PrimaryDarkBrush}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- Touch-friendly text input style -->
    <Style x:Key="TouchTextBoxStyle" TargetType="TextBox">
        <Setter Property="MinHeight" Value="44"/>
        <Setter Property="Padding" Value="12,8"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Margin" Value="4"/>
        <Setter Property="BorderThickness" Value="2"/>
        <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
        <Setter Property="Background" Value="White"/>
        <Style.Triggers>
            <Trigger Property="IsFocused" Value="True">
                <Setter Property="BorderBrush" Value="{StaticResource PrimaryBrush}"/>
            </Trigger>
        </Style.Triggers>
    </Style>

    <!-- Touch-friendly list item style -->
    <Style x:Key="TouchListBoxItemStyle" TargetType="ListBoxItem">
        <Setter Property="MinHeight" Value="60"/>
        <Setter Property="Padding" Value="16,12"/>
        <Setter Property="Margin" Value="0,2"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ListBoxItem">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="4">
                        <ContentPresenter/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsSelected" Value="True">
                            <Setter Property="Background" Value="{StaticResource SelectionBrush}"/>
                        </Trigger>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter Property="Background" Value="{StaticResource HoverBrush}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

</ResourceDictionary>
```

### MainWindow with touch optimization

**Main application window (src/Views/MainWindow.xaml):**

```xml
<controls:MetroWindow x:Class="TouchGanttChart.Views.MainWindow"
                      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                      xmlns:controls="http://metro.mahapps.com/winfx/xaml/controls"
                      xmlns:vm="clr-namespace:TouchGanttChart.ViewModels"
                      Title="Touch Gantt Chart" 
                      Height="800" 
                      Width="1200"
                      MinHeight="600"
                      MinWidth="800"
                      IsManipulationEnabled="True">

    <controls:MetroWindow.DataContext>
        <vm:MainWindowViewModel/>
    </controls:MetroWindow.DataContext>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Touch-optimized toolbar -->
        <StackPanel Grid.Row="0" 
                    Orientation="Horizontal" 
                    Background="{StaticResource ToolbarBrush}"
                    Margin="0,0,0,8">
            
            <Button Content="New Project" 
                    Command="{Binding NewProjectCommand}"
                    Style="{StaticResource TouchButtonStyle}"/>
            
            <Button Content="Open Project" 
                    Command="{Binding OpenProjectCommand}"
                    Style="{StaticResource TouchButtonStyle}"/>
            
            <Button Content="Save" 
                    Command="{Binding SaveCommand}"
                    Style="{StaticResource TouchButtonStyle}"/>
            
            <Separator Margin="8,0"/>
            
            <Button Content="Add Task" 
                    Command="{Binding AddTaskCommand}"
                    Style="{StaticResource TouchButtonStyle}"/>
            
            <Button Content="Export PDF" 
                    Command="{Binding ExportPdfCommand}"
                    Style="{StaticResource TouchButtonStyle}"/>
            
        </StackPanel>

        <!-- Main content area -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="300" MinWidth="200"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <!-- Project/Task tree view -->
            <Border Grid.Column="0" 
                    Background="White"
                    BorderBrush="{StaticResource BorderBrush}"
                    BorderThickness="0,0,1,0">
                
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="*"/>
                    </Grid.RowDefinitions>

                    <TextBlock Text="Projects &amp; Tasks" 
                               FontSize="16" 
                               FontWeight="SemiBold"
                               Margin="16,12"/>

                    <TreeView Grid.Row="1" 
                              ItemsSource="{Binding Projects}"
                              SelectedItem="{Binding SelectedItem}"
                              Margin="8">
                        <!-- TreeView item template would go here -->
                    </TreeView>
                </Grid>
            </Border>

            <!-- Splitter -->
            <GridSplitter Grid.Column="1" 
                          Width="8" 
                          HorizontalAlignment="Stretch"
                          Background="{StaticResource BorderBrush}"/>

            <!-- Gantt chart area -->
            <Border Grid.Column="2" 
                    Background="White">
                
                <ScrollViewer x:Name="GanttScrollViewer"
                              HorizontalScrollBarVisibility="Auto"
                              VerticalScrollBarVisibility="Auto"
                              IsManipulationEnabled="True">
                    
                    <!-- Custom Gantt chart control will be implemented here -->
                    <Canvas x:Name="GanttCanvas" 
                            Background="White"
                            IsManipulationEnabled="True">
                        
                        <!-- Gantt chart content -->
                        
                    </Canvas>
                    
                </ScrollViewer>
            </Border>
        </Grid>

        <!-- Status bar -->
        <StatusBar Grid.Row="2" 
                   Height="28">
            <StatusBarItem>
                <TextBlock Text="{Binding StatusMessage}"/>
            </StatusBarItem>
            <StatusBarItem HorizontalAlignment="Right">
                <ProgressBar Width="120" 
                             Height="16"
                             IsIndeterminate="{Binding IsLoading}"
                             Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"/>
            </StatusBarItem>
        </StatusBar>

    </Grid>

</controls:MetroWindow>
```

## AI assistant configuration

Configure the project for optimal AI-assisted development:

### AI configuration files

**Claude assistant configuration (.ai/CLAUDE.md):**

```markdown
# Touch Gantt Chart WPF Project - AI Assistant Configuration

## Project Context
- **Framework**: WPF (.NET 8.0) with touch optimization
- **Architecture**: MVVM with dependency injection
- **UI Framework**: MahApps.Metro with custom touch styles
- **Database**: SQLite with Entity Framework Core
- **Testing**: xUnit with comprehensive coverage target

## Core Technologies
- CommunityToolkit.Mvvm for MVVM implementation
- Microsoft.Extensions.DependencyInjection for IoC
- Touch-optimized UI controls with gesture support
- IronPDF for report generation

## Development Standards
- **File Organization**: Feature-based folder structure
- **Naming**: Microsoft C# conventions with descriptive names  
- **Dependencies**: Constructor injection pattern
- **Touch UI**: Minimum 44px touch targets, 8px margins
- **Async**: All I/O operations use async/await pattern
- **Error Handling**: Comprehensive logging with Serilog

## Code Pattern Examples
- ViewModels inherit from ViewModelBase with CommunityToolkit.Mvvm
- Services implement interface contracts with DI registration
- Touch gestures handled through manipulation events
- XAML follows touch-friendly design principles

## Quality Requirements
- XML documentation for all public APIs
- Unit test coverage >80%
- Touch gesture validation on actual hardware
- Performance optimization for large datasets
- Accessibility compliance for screen readers
```

### Project context documentation

**Development context (.ai/context.md):**

```markdown
# Development Context for AI Assistants

## Current Phase
Building core Gantt chart functionality with touch-optimized interface

## Active Components
- **Models**: GanttTask, Project entities with validation
- **ViewModels**: MainWindowViewModel with command patterns
- **Data Access**: Entity Framework with SQLite backend
- **Touch UI**: Custom controls with gesture recognition

## Implementation Priorities
1. Touch-responsive Gantt chart canvas
2. Drag-and-drop task scheduling
3. Multi-touch zoom and pan functionality  
4. PDF export with formatted timeline
5. Real-time collaboration features

## Technical Constraints
- Touch targets must be minimum 44px for finger navigation
- Performance optimized for datasets up to 10,000 tasks
- Offline-first design with sync capabilities
- Cross-device compatibility (tablets, touch laptops)

## Quality Gates
- All touch interactions tested on physical devices
- Accessibility validation with Windows Narrator
- Performance profiling under load conditions
- Memory leak detection in long-running sessions

## Integration Points
- PDF export service using IronPDF
- Touch gesture processing with native WPF APIs
- Database migrations through EF Core tools
- Unit testing with touch simulation frameworks
```

## Development workflow configuration

Establish an efficient development and testing workflow:

### Build and test scripts

Create these PowerShell scripts for common development tasks:

**build.ps1:**

```powershell
# Build script for Touch Gantt Chart
Write-Host "Building Touch Gantt Chart application..." -ForegroundColor Green

# Clean previous builds
dotnet clean
if ($LASTEXITCODE -ne 0) { 
    Write-Error "Clean failed"
    exit 1
}

# Restore packages
dotnet restore
if ($LASTEXITCODE -ne 0) { 
    Write-Error "Restore failed"
    exit 1
}

# Build application
dotnet build --configuration Debug
if ($LASTEXITCODE -ne 0) { 
    Write-Error "Build failed"
    exit 1
}

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test --configuration Debug --logger "console;verbosity=normal"
if ($LASTEXITCODE -ne 0) { 
    Write-Error "Tests failed"
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green
```

**setup-dev.ps1:**

```powershell
# Development environment setup script
Write-Host "Setting up development environment..." -ForegroundColor Green

# Create necessary directories
$directories = @(
    "src/Models",
    "src/ViewModels/Base", 
    "src/Views/Controls",
    "src/Views/Dialogs",
    "src/Services/Interfaces",
    "src/Services/Implementations", 
    "src/Data",
    "src/Resources/Styles",
    "src/Resources/Templates", 
    "tests/UnitTests",
    "tests/IntegrationTests",
    "docs",
    "examples",
    ".ai/examples"
)

foreach ($dir in $directories) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    Write-Host "Created directory: $dir" -ForegroundColor Gray
}

# Initialize database  
Write-Host "Initializing database..." -ForegroundColor Yellow
dotnet ef database update
if ($LASTEXITCODE -ne 0) { 
    Write-Warning "Database update failed - run manually later"
}

Write-Host "Development environment setup complete!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Run 'dotnet build' to verify setup"
Write-Host "2. Press F5 in VSCode to start debugging"  
Write-Host "3. Review .ai/CLAUDE.md for AI assistant guidelines"
```

## Implementation roadmap

Execute this step-by-step implementation plan:

### Phase 1: Foundation setup (Day 1)

1. **Create project structure**: Run setup scripts and verify VSCode configuration
2. **Install dependencies**: Execute package installation commands
3. **Initialize database**: Create initial EF migrations and seed data
4. **Basic UI framework**: Implement MainWindow with touch-optimized layout

### Phase 2: Core functionality (Days 2-5)

1. **Domain models**: Complete GanttTask and Project entities with validation
2. **Data services**: Implement repository pattern with CRUD operations  
3. **MVVM ViewModels**: Build MainWindowViewModel with command bindings
4. **Touch UI controls**: Create custom Gantt chart canvas with manipulation events

### Phase 3: Advanced features (Days 6-10)

1. **Touch gestures**: Implement pan, zoom, and selection with multi-touch support
2. **Task editing**: Build touch-friendly task editor dialog with validation
3. **PDF export**: Integrate IronPDF for formatted timeline reports
4. **Testing framework**: Comprehensive unit and integration test coverage

### Phase 4: Polish and optimization (Days 11-14)

1. **Performance tuning**: Optimize rendering for large datasets with virtualization
2. **Accessibility**: Ensure screen reader compatibility and keyboard navigation
3. **Documentation**: Complete API documentation and user guides
4. **Deployment**: Package application with installer and distribution

This comprehensive implementation plan provides everything needed to build a modern, touch-optimized WPF Gantt chart application with VSCode and AI-assisted development workflows. The architecture supports scalability, maintainability, and excellent user experience on touch-enabled devices.