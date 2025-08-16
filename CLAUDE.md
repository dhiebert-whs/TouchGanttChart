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

## Production Development Plan

### Phase 1: Core Foundation (Weeks 1-3)
**Objective**: Establish technical foundation with .NET 9.0 and basic architecture

#### Week 1: Infrastructure Setup
- Database Layer: Entity Framework Core data models (GanttTask, Project)
- MVVM Foundation: ViewModelBase with CommunityToolkit.Mvvm 
- Dependency Injection: Microsoft.Extensions.DependencyInjection container
- Logging: Serilog with file and console sinks
- Testing Framework: xUnit test projects structure

#### Week 2: Core Services
- Data Services: Repository pattern with CRUD operations
- PDF Export Service: IronPDF integration for timeline reports
- Touch Gesture Service: Manipulation event handling foundation
- Database Initialization: EF migrations and seed data

#### Week 3: Basic UI Framework
- MainWindow Layout: Touch-optimized three-panel design
- Touch Styles: 44px minimum touch targets with 8px margins
- MahApps.Metro Integration: Modern UI with DPI scaling
- Basic Navigation: Project/task tree view with selection

### Phase 2: Core Gantt Functionality (Weeks 4-7)
**Objective**: Essential Gantt chart features for immediate productivity

#### Week 4: Task Management
- Task CRUD Operations: Create, edit, delete with touch-friendly dialogs
- Task Properties: Start/end dates, progress, status, priority
- Validation: Client-side validation with error handling
- Data Binding: ViewModels connected to UI

#### Week 5: Timeline Visualization
- Canvas Rendering: Custom Gantt chart canvas with timeline scale
- Task Bars: Horizontal rectangles with proper sizing
- Time Scale Views: Daily, weekly, monthly displays
- Today Indicator: Visual marker for current date

#### Week 6: Basic Dependencies
- Finish-to-Start Links: Simple dependency relationships
- Visual Connectors: Arrow lines between dependent tasks
- Dependency Validation: Prevent circular dependencies
- Timeline Recalculation: Auto-update dependent dates

#### Week 7: Touch Interactions
- Pan & Zoom: Multi-touch timeline navigation with momentum
- Task Selection: Touch-based selection with visual feedback
- Date Adjustment: Drag-and-drop task bar resizing/moving
- Gesture Conflict Resolution: 150ms tap vs pan timing

### Phase 3: Professional Features (Weeks 8-11)
**Objective**: Advanced functionality for professional project management

#### Week 8: Enhanced Task Management
- Subtask Hierarchy: Parent-child task relationships
- Team Assignment: Resource allocation to tasks
- Status Tracking: Progress indicators with visual cues
- Task Templates: Common task patterns for reuse

#### Week 9: Advanced Timeline Features
- Milestone Markers: Key project checkpoints
- Critical Path Display: Highlight critical task sequences
- Timeline Filtering: Filter by status, assignee, priority
- Bulk Operations: Multi-select task operations

#### Week 10: Export & Reporting
- PDF Export Enhancement: Professional timeline reports
- Data Export: CSV/Excel export for external tools
- Print Layout: Optimized printing with page breaks
- Report Templates: Standardized report formats

#### Week 11: Collaboration Features
- Project Sharing: Save/load project files
- Recent Projects: Quick access to frequent projects
- Backup & Recovery: Auto-save and recovery mechanisms
- Change Tracking: Audit trail for task modifications

### Phase 4: Large Display Optimization (Weeks 12-15)
**Objective**: Optimize for 80+ inch cleartouch boards

#### Week 12: DPI & Scaling
- Large Display Support: 150-200% UI scaling for 80+ inch displays
- Font Optimization: Minimum 16px fonts (24-32px effective)
- Touch Target Enhancement: Compensate for PCT parallax errors
- Distance-Based Design: 2-4 feet optimal viewing distance

#### Week 13: Advanced Touch Gestures
- Multi-Touch Gestures: Sophisticated touch patterns
- Gesture Customization: User-configurable touch behaviors
- Hardware Integration: Cleartouch board optimizations
- Performance Tuning: Smooth gesture handling under load

#### Week 14: Performance & Virtualization
- Large Dataset Support: Handle 10,000+ tasks efficiently
- Canvas Virtualization: Render only visible timeline sections
- Memory Optimization: Prevent memory leaks
- Database Performance: SQLite WAL mode optimization

#### Week 15: Accessibility & Polish
- Screen Reader Support: Windows Narrator compatibility
- Keyboard Navigation: Full keyboard accessibility
- High Contrast: Support for accessibility themes
- Touch Feedback: Haptic/visual feedback for interactions

### Phase 5: Production Deployment (Weeks 16-18)
**Objective**: Package and deploy production-ready application

#### Week 16: Quality Assurance
- Comprehensive Testing: >80% unit test coverage
- Integration Testing: End-to-end workflow validation
- Touch Hardware Testing: Physical device validation
- Performance Profiling: Load testing with large datasets

#### Week 17: Documentation & Training
- API Documentation: XML documentation for all public APIs
- User Guide: Touch-specific usage instructions
- Deployment Guide: Installation and configuration
- Troubleshooting: Common issues and solutions

#### Week 18: Deployment & Release
- Installer Creation: Professional installation package
- Distribution Setup: Deployment to target devices
- Monitoring: Application health and usage tracking
- Support Infrastructure: Issue tracking and resolution

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