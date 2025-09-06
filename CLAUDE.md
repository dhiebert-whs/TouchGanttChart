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

## Current Development Status (Updated: 2025-09-06)

### ✅ **COMPLETED** - Core Foundation & Project Management
**Infrastructure, Architecture, and Core Project Operations - FULLY IMPLEMENTED**

#### Infrastructure Setup ✅ COMPLETE
- ✅ Database Layer: Entity Framework Core with GanttTask, Project, ProjectTemplate models
- ✅ MVVM Foundation: ViewModelBase with CommunityToolkit.Mvvm implemented
- ✅ Dependency Injection: Microsoft.Extensions.DependencyInjection container configured
- ✅ Logging: Serilog with file and console sinks working
- ✅ Testing Framework: xUnit test projects structure in place
- ✅ Database Schema: All migrations applied, Category column added successfully

#### Core Services ✅ COMPLETE  
- ✅ Data Services: Full repository pattern with CRUD operations implemented
- ✅ PDF Export Service: IronPDF integration service created
- ✅ Touch Gesture Service: Manipulation event handling foundation implemented
- ✅ Database Initialization: EF migrations working, seed data populated automatically

#### Basic UI Framework ✅ COMPLETE
- ✅ MainWindow Layout: Touch-optimized three-panel design implemented
- ✅ Touch Styles: 44px minimum touch targets with 8px margins applied throughout
- ✅ Modern UI: Clean, professional styling with proper touch optimization
- ✅ Basic Navigation: Project/task tree view with selection implemented

#### Project Management ✅ COMPLETE
- ✅ **New Project**: Project template selection dialog fully functional
- ✅ **Open Project**: ProjectSelectionDialog with touch-optimized project browsing
- ✅ **Close Project**: Proper project closure with command state management
- ✅ **Save Project**: Project persistence and updates working
- ✅ **Project Templates**: 3 built-in templates with task templates and dependencies

#### Task Management ✅ COMPLETE
- ✅ **Task CRUD Operations**: Create, edit, delete fully implemented and working
- ✅ **Task Properties**: Complete data model with all required fields
- ✅ **Status Dropdown**: Populated with TaskStatus enum values (NotStarted, InProgress, Completed, OnHold, Cancelled)
- ✅ **Priority Dropdown**: Populated with TaskPriority enum values (Low, Normal, High, Critical)
- ✅ **Category Management**: 10 predefined categories (General, Mechanical, Electrical, Software, Documentation, Testing, Design, Research, Planning, Marketing)
- ✅ **Duration Auto-calculation**: Automatic calculation from start/end dates with display formatting
- ✅ **Create Task Button**: Fully functional with proper project assignment

### 🚧 **REMAINING TASKS** - Advanced Features

#### Task Dependencies & Hierarchy 🚧 TODO
- ❌ **Task Dependencies UI**: Dependency selection missing from create/edit task dialogs
- ❌ **Subtask Hierarchy**: Need to verify parent-child task relationships in UI
- ❌ **Custom Options Management**: Advanced status/priority customization

#### Timeline Visualization 🟡 PARTIAL  
- ✅ **Canvas Framework**: GanttTimelineCanvas custom control created
- ✅ **Touch Support**: Manipulation events wired up for pan/zoom
- ❌ **Task Bar Rendering**: Visual task bars need to be drawn on timeline
- ❌ **Timeline Headers**: Need day/date/weekday headers (currently only basic headers)
- ❌ **Today Indicator**: Current date marker not implemented
- ❌ **Drag & Drop**: Task bars not draggable for date adjustment

#### Custom Management Features 🚧 TODO
- ❌ **Custom Category Management**: UI for adding/removing custom categories
- ❌ **Advanced Dependencies**: Complex dependency types and validation

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