# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TouchGanttChart is a WPF application built with .NET 9.0 that provides touch-enabled Gantt chart functionality. The application uses modern C# patterns with nullable reference types and implicit usings enabled.

## Build and Development Commands

```bash
# Build the solution
dotnet build

# Run the application
dotnet run

# Build for release
dotnet build --configuration Release

# Clean build artifacts
dotnet clean

# Run tests
dotnet test

# Restore packages
dotnet restore
```

## Architecture

The codebase follows a clean architecture pattern with clear separation of concerns:

- **src/Models/**: Domain models and entities
- **src/ViewModels/**: MVVM view models using CommunityToolkit.Mvvm
- **src/Views/**: WPF views, controls, and dialogs
- **src/Services/**: Business logic with interface/implementation separation
- **src/Data/**: Data access layer using Entity Framework Core with SQLite
- **src/Resources/**: XAML styles, templates, and application resources

## Key Dependencies

- **CommunityToolkit.Mvvm**: MVVM framework for view models and commanding
- **Microsoft.Extensions.Hosting**: Dependency injection and hosting
- **Entity Framework Core**: Data access with SQLite provider
- **Serilog**: Structured logging to file and console
- **MahApps.Metro**: Modern UI framework for WPF
- **IronPdf**: PDF export functionality
- **xUnit**: Testing framework

## Database

The application uses Entity Framework Core with SQLite as the database provider. Migration commands:

```bash
# Add new migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Drop database
dotnet ef database drop
```

## Testing

Tests are organized in the `tests/` directory with separate projects for unit and integration tests. Use xUnit as the testing framework.

## Current Development Status (Updated: 2025-09-07)

### ✅ **COMPLETED** - Core Foundation & Advanced Features
**Full-Featured Gantt Chart Application - PRODUCTION READY**

#### Infrastructure Setup ✅ COMPLETE
- ✅ Database Layer: Entity Framework Core with GanttTask, Project, ProjectTemplate models
- ✅ MVVM Foundation: ViewModelBase with CommunityToolkit.Mvvm implemented
- ✅ Dependency Injection: Microsoft.Extensions.DependencyInjection container configured
- ✅ Logging: Serilog with file and console sinks working
- ✅ Testing Framework: xUnit test projects structure in place
- ✅ Database Schema: All migrations applied, CompletionDate column added successfully

#### Core Services ✅ COMPLETE  
- ✅ Data Services: Full repository pattern with CRUD operations implemented
- ✅ PDF Export Service: IronPDF integration with user file selection dialog
- ✅ Touch Gesture Service: Advanced manipulation event handling with drag-and-drop
- ✅ Database Initialization: EF migrations working, seed data populated automatically
- ✅ Dependency Service: Task completion date tracking and dependency shifting

#### Advanced UI Framework ✅ COMPLETE
- ✅ MainWindow Layout: Touch-optimized three-panel design implemented
- ✅ Touch Styles: 44px minimum touch targets with 8px margins applied throughout
- ✅ Modern UI: Clean, professional styling with proper touch optimization
- ✅ Navigation: Project/task tree view with selection and double-click support
- ✅ Visual Differentiation: Color-coded task relationships and priority indicators

#### Project Management ✅ COMPLETE
- ✅ **New Project**: Project template selection dialog fully functional
- ✅ **Open Project**: Fixed ProjectSelectionDialog with proper project browsing
- ✅ **Close Project**: Proper project closure with command state management
- ✅ **Save Project**: Project persistence and updates working
- ✅ **Project Templates**: 4 built-in templates with comprehensive task structures

#### Advanced Task Management ✅ COMPLETE
- ✅ **Task CRUD Operations**: Create, edit, delete fully implemented with double-click support
- ✅ **Enhanced Task Editor**: All dropdowns functional (Status, Priority, Category, Assignee)
- ✅ **Team Role Assignments**: Dropdown with Mechanical, Electrical, Programming, PR, Leadership
- ✅ **Completion Date Tracking**: Task completion dates with early/late indicators
- ✅ **Dependency Shifting**: Automatic adjustment of dependent tasks when predecessors complete early
- ✅ **Visual Task Relationships**: Color-coded borders, priority bars, completion status circles

#### Professional Timeline Visualization ✅ COMPLETE
- ✅ **Advanced Canvas Framework**: GanttTimelineCanvas with full interaction support
- ✅ **Touch & Mouse Support**: Pan, zoom, select, and drag operations fully implemented
- ✅ **Task Bar Rendering**: Visual task bars with relationship-based styling
- ✅ **Enhanced Timeline Headers**: Weekly views with Monday dates, day/date headers
- ✅ **Drag & Drop**: Task bars draggable for real-time date modification
- ✅ **Interactive Features**: Double-click to edit, visual feedback during operations

#### Export & Reporting ✅ COMPLETE
- ✅ **PDF Export**: Complete project export with user-selectable file location
- ✅ **File Dialog Integration**: Professional save dialog with suggested filenames
- ✅ **Export Formatting**: Professional PDF layouts with task details and timeline

### ✅ **ALL CORE REQUIREMENTS MET**

The application now includes all essential Gantt chart functionality:

1. ✅ **Interactive Task Management**: Full CRUD with drag-and-drop date modification
2. ✅ **Professional UI**: Touch-optimized interface with visual task differentiation  
3. ✅ **Team Collaboration**: Role-based assignments and project templates
4. ✅ **Advanced Scheduling**: Completion tracking with dependency management
5. ✅ **Export Capabilities**: PDF generation with user file selection
6. ✅ **Database Integration**: Robust SQLite backend with Entity Framework

### 🚀 **READY FOR PRODUCTION USE**

### 📋 **FUTURE PHASES** - Advanced Features

#### Professional Features (Phase 3)
- Enhanced Task Management (subtasks, team assignment, status tracking)
- Advanced Timeline (milestones, critical path, filtering, bulk operations)  
- Export & Reporting (enhanced PDF, data export, print layouts)
- Collaboration (project sharing, recent projects, backup/recovery)

#### Large Display Optimization (Future Phase)
- Large Display Support: 150-200% UI scaling for 80+ inch displays
- Advanced Touch Gestures: Multi-touch patterns and customization
- Performance & Virtualization: Handle 10,000+ tasks efficiently
- Accessibility & Polish: Screen reader support, keyboard navigation

#### Production Deployment (Final Phase)  
- Quality Assurance: Comprehensive testing and validation
- Documentation & Training: User guides and deployment documentation
- Installer & Distribution: Professional deployment package

## Touch Design Requirements

- **Minimum Touch Targets**: 44px minimum with 8px margins
- **Large Display Scaling**: 150-200% UI scaling for 80+ inch displays
- **Gesture Support**: Pan, zoom, select with conflict resolution
- **Performance Target**: <100ms gesture response time
- **Dataset Capacity**: Support 10,000+ tasks efficiently

## Development Standards

- **File Organization**: Feature-based folder structure following clean architecture
- **Naming**: Microsoft C# conventions with descriptive names
- **Dependencies**: Constructor injection pattern throughout
- **Async Operations**: All I/O operations use async/await pattern
- **Error Handling**: Comprehensive logging with Serilog
- **Testing**: >80% unit test coverage target with touch gesture simulation