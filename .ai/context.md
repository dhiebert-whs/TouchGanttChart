context.md
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