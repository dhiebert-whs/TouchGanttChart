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