using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TouchGanttChart.Models;

namespace TouchGanttChart.Controls;

/// <summary>
/// Custom Canvas control for displaying Gantt chart timeline with touch optimization.
/// Renders tasks as horizontal bars with proper scaling and touch interaction.
/// </summary>
public class GanttTimelineCanvas : Canvas
{
    private const double TASK_BAR_HEIGHT = 32;
    private const double TASK_ROW_HEIGHT = 44; // Touch-optimized row height
    private const double TIMELINE_HEADER_HEIGHT = 60;
    private const double CATEGORY_HEADER_HEIGHT = 36;
    private const double CATEGORY_SPACING = 8;
    private const double GRID_LINE_THICKNESS = 1;
    private const double MAJOR_GRID_LINE_THICKNESS = 2;

    private readonly List<TaskBarElement> _taskBars = new();
    private readonly List<Line> _gridLines = new();
    private readonly List<TextBlock> _timelineLabels = new();
    private readonly List<UIElement> _dependencyLines = new();
    private Point _lastPanPoint;
    private bool _isPanning;
    private DateTime _lastClickTime = DateTime.MinValue;
    private GanttTask? _lastClickedTask;
    private bool _isDraggingTask;
    private GanttTask? _draggingTask;
    private Point _dragStartPoint;
    private Border? _draggedTaskBar;

    static GanttTimelineCanvas()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GanttTimelineCanvas), 
            new FrameworkPropertyMetadata(typeof(GanttTimelineCanvas)));
    }

    public GanttTimelineCanvas()
    {
        Background = Brushes.White;
        ClipToBounds = true;
        
        // Enable touch and manipulation
        IsManipulationEnabled = true;
        Focusable = true;
        
        // Subscribe to manipulation events for touch interaction
        ManipulationStarted += OnManipulationStarted;
        ManipulationDelta += OnManipulationDelta;
        ManipulationCompleted += OnManipulationCompleted;
        
        // Mouse events for desktop compatibility
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        // MouseWheel removed - zoom only via buttons
        
        SizeChanged += OnSizeChanged;
    }

    #region Dependency Properties

    public static readonly DependencyProperty TasksProperty =
        DependencyProperty.Register(nameof(Tasks), typeof(ObservableCollection<GanttTask>), 
            typeof(GanttTimelineCanvas),
            new PropertyMetadata(null, OnTasksChanged));

    public static readonly DependencyProperty TimelineStartProperty =
        DependencyProperty.Register(nameof(TimelineStart), typeof(DateTime), 
            typeof(GanttTimelineCanvas),
            new PropertyMetadata(DateTime.Today.AddMonths(-1), OnTimelineChanged));

    public static readonly DependencyProperty TimelineEndProperty =
        DependencyProperty.Register(nameof(TimelineEnd), typeof(DateTime), 
            typeof(GanttTimelineCanvas),
            new PropertyMetadata(DateTime.Today.AddMonths(2), OnTimelineChanged));

    public static readonly DependencyProperty ZoomLevelProperty =
        DependencyProperty.Register(nameof(ZoomLevel), typeof(double), 
            typeof(GanttTimelineCanvas),
            new PropertyMetadata(1.0, OnZoomLevelChanged));

    public static readonly DependencyProperty SelectedTaskProperty =
        DependencyProperty.Register(nameof(SelectedTask), typeof(GanttTask), 
            typeof(GanttTimelineCanvas),
            new PropertyMetadata(null, OnSelectedTaskChanged));

    public static readonly DependencyProperty ViewModeProperty =
        DependencyProperty.Register(nameof(ViewMode), typeof(TimelineViewMode), 
            typeof(GanttTimelineCanvas),
            new PropertyMetadata(TimelineViewMode.Weekly, OnViewModeChanged));

    public static readonly DependencyProperty HighlightedTaskChainProperty =
        DependencyProperty.Register(nameof(HighlightedTaskChain), typeof(ObservableCollection<GanttTask>), 
            typeof(GanttTimelineCanvas),
            new PropertyMetadata(null, OnHighlightedTaskChainChanged));

    public ObservableCollection<GanttTask>? Tasks
    {
        get => (ObservableCollection<GanttTask>?)GetValue(TasksProperty);
        set => SetValue(TasksProperty, value);
    }

    public DateTime TimelineStart
    {
        get => (DateTime)GetValue(TimelineStartProperty);
        set => SetValue(TimelineStartProperty, value);
    }

    public DateTime TimelineEnd
    {
        get => (DateTime)GetValue(TimelineEndProperty);
        set => SetValue(TimelineEndProperty, value);
    }

    public double ZoomLevel
    {
        get => (double)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    public GanttTask? SelectedTask
    {
        get => (GanttTask?)GetValue(SelectedTaskProperty);
        set => SetValue(SelectedTaskProperty, value);
    }

    public TimelineViewMode ViewMode
    {
        get => (TimelineViewMode)GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }

    public ObservableCollection<GanttTask>? HighlightedTaskChain
    {
        get => (ObservableCollection<GanttTask>?)GetValue(HighlightedTaskChainProperty);
        set => SetValue(HighlightedTaskChainProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Event fired when a task is double-clicked
    /// </summary>
    public event EventHandler<GanttTask>? TaskDoubleClicked;

    /// <summary>
    /// Event fired when a task's dates are changed through dragging
    /// </summary>
    public event EventHandler<TaskDatesChangedEventArgs>? TaskDatesChanged;

    #endregion

    #region Property Change Handlers

    private static void OnTasksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GanttTimelineCanvas canvas)
        {
            if (e.OldValue is ObservableCollection<GanttTask> oldTasks)
            {
                oldTasks.CollectionChanged -= canvas.OnTasksCollectionChanged;
            }

            if (e.NewValue is ObservableCollection<GanttTask> newTasks)
            {
                newTasks.CollectionChanged += canvas.OnTasksCollectionChanged;
            }

            canvas.RedrawTimeline();
        }
    }

    private static void OnTimelineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GanttTimelineCanvas canvas)
        {
            canvas.RedrawTimeline();
        }
    }

    private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GanttTimelineCanvas canvas)
        {
            canvas.RedrawTimeline();
        }
    }

    private static void OnSelectedTaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GanttTimelineCanvas canvas)
        {
            canvas.UpdateTaskSelection();
        }
    }

    private static void OnViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GanttTimelineCanvas canvas)
        {
            canvas.ApplyViewModeSettings();
            canvas.RedrawTimeline();
        }
    }

    private static void OnHighlightedTaskChainChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GanttTimelineCanvas canvas)
        {
            canvas.RedrawTimeline();
        }
    }

    private void OnTasksCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (Dispatcher.CheckAccess())
        {
            RedrawTimeline();
        }
        else
        {
            Dispatcher.BeginInvoke(RedrawTimeline);
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        RedrawTimeline();
    }

    #endregion

    #region Timeline Rendering

    public void RedrawTimeline()
    {
        if (Tasks == null || ActualWidth <= 0 || ActualHeight <= 0)
            return;

        ClearCanvas();
        DrawTimelineGrid();
        DrawTimelineHeader();
        DrawTaskBars();
        DrawDependencyLines();
        UpdateCanvasSize();
    }

    private void ClearCanvas()
    {
        Children.Clear();
        _taskBars.Clear();
        _gridLines.Clear();
        _timelineLabels.Clear();
        _dependencyLines.Clear();
    }

    private void DrawTimelineGrid()
    {
        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        if (totalDays <= 0) return;

        var pixelsPerDay = (ActualWidth * ZoomLevel) / totalDays;
        var unitDays = TimelineViewConfig.GetTimeUnitDays(ViewMode);
        var currentDate = TimelineStart;

        // Draw vertical grid lines based on view mode
        while (currentDate <= TimelineEnd)
        {
            var x = (currentDate - TimelineStart).TotalDays * pixelsPerDay;
            
            var (isMajor, isMinor) = GetGridLineType(currentDate);
            
            var thickness = isMajor ? MAJOR_GRID_LINE_THICKNESS : 
                           isMinor ? GRID_LINE_THICKNESS * 1.5 : GRID_LINE_THICKNESS;
            
            var brush = isMajor ? Brushes.DarkGray :
                       isMinor ? Brushes.Gray : Brushes.LightGray;

            var line = new Line
            {
                X1 = x,
                Y1 = TIMELINE_HEADER_HEIGHT,
                X2 = x,
                Y2 = ActualHeight,
                Stroke = brush,
                StrokeThickness = thickness
            };

            Children.Add(line);
            _gridLines.Add(line);

            currentDate = GetNextGridDate(currentDate);
        }

        // Draw horizontal grid lines aligned with category structure
        if (Tasks != null && Tasks.Count > 0)
        {
            var tasksByCategory = Tasks.GroupBy(t => t.Category).ToList();
            double currentY = TIMELINE_HEADER_HEIGHT;
            
            foreach (var categoryGroup in tasksByCategory.OrderBy(g => g.Key))
            {
                // Category header line
                var categoryLine = new Line
                {
                    X1 = 0,
                    Y1 = currentY,
                    X2 = ActualWidth * ZoomLevel,
                    Y2 = currentY,
                    Stroke = Brushes.Gray,
                    StrokeThickness = MAJOR_GRID_LINE_THICKNESS
                };
                Children.Add(categoryLine);
                _gridLines.Add(categoryLine);
                
                currentY += CATEGORY_HEADER_HEIGHT + CATEGORY_SPACING;
                
                // Task rows within category
                foreach (var task in categoryGroup.OrderBy(t => t.StartDate))
                {
                    var taskLine = new Line
                    {
                        X1 = 0,
                        Y1 = currentY,
                        X2 = ActualWidth * ZoomLevel,
                        Y2 = currentY,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = GRID_LINE_THICKNESS
                    };
                    Children.Add(taskLine);
                    _gridLines.Add(taskLine);
                    
                    currentY += TASK_ROW_HEIGHT;
                }
                
                currentY += CATEGORY_SPACING;
            }
        }
    }

    private void DrawTimelineHeader()
    {
        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        if (totalDays <= 0) return;

        var pixelsPerDay = (ActualWidth * ZoomLevel) / totalDays;
        
        // Header background
        var headerBackground = new Rectangle
        {
            Width = ActualWidth * ZoomLevel,
            Height = TIMELINE_HEADER_HEIGHT,
            Fill = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            Stroke = Brushes.DarkGray,
            StrokeThickness = 1
        };
        Canvas.SetTop(headerBackground, 0);
        Children.Add(headerBackground);

        // Draw view-mode specific headers
        switch (ViewMode)
        {
            case TimelineViewMode.Daily:
                DrawDailyHeader(pixelsPerDay);
                break;
            case TimelineViewMode.Weekly:
                DrawWeeklyHeader(pixelsPerDay);
                break;
            case TimelineViewMode.Monthly:
                DrawMonthlyHeader(pixelsPerDay);
                break;
            case TimelineViewMode.Quarterly:
                DrawQuarterlyHeader(pixelsPerDay);
                break;
            case TimelineViewMode.Yearly:
                DrawYearlyHeader(pixelsPerDay);
                break;
        }
    }

    private void DrawTaskBars()
    {
        if (Tasks == null) return;

        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        if (totalDays <= 0) return;

        var pixelsPerDay = (ActualWidth * ZoomLevel) / totalDays;

        // Group tasks by category
        var tasksByCategory = Tasks.GroupBy(t => t.Category).ToList();
        
        double currentY = TIMELINE_HEADER_HEIGHT;
        
        foreach (var categoryGroup in tasksByCategory.OrderBy(g => g.Key))
        {
            // Draw category header
            DrawCategoryHeader(categoryGroup.Key, currentY, pixelsPerDay);
            currentY += CATEGORY_HEADER_HEIGHT + CATEGORY_SPACING;
            
            // Draw tasks in this category
            foreach (var task in categoryGroup.OrderBy(t => t.StartDate))
            {
                var taskY = currentY + (TASK_ROW_HEIGHT - TASK_BAR_HEIGHT) / 2;
                var taskElement = CreateTaskBarElement(task, pixelsPerDay, taskY);
                _taskBars.Add(taskElement);
                Children.Add(taskElement.Container);
                
                currentY += TASK_ROW_HEIGHT;
            }
            
            // Add spacing between categories
            currentY += CATEGORY_SPACING;
        }
    }

    private void DrawCategoryHeader(string categoryName, double y, double pixelsPerDay)
    {
        // Category background
        var categoryBackground = new Rectangle
        {
            Width = ActualWidth * ZoomLevel,
            Height = CATEGORY_HEADER_HEIGHT,
            Fill = new SolidColorBrush(Color.FromRgb(240, 244, 248)),
            Stroke = new SolidColorBrush(Color.FromRgb(206, 212, 218)),
            StrokeThickness = 1
        };
        Canvas.SetTop(categoryBackground, y);
        Children.Add(categoryBackground);

        // Category label
        var categoryLabel = new TextBlock
        {
            Text = categoryName,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 0, 0)
        };
        Canvas.SetLeft(categoryLabel, 16);
        Canvas.SetTop(categoryLabel, y + (CATEGORY_HEADER_HEIGHT - 20) / 2);
        Children.Add(categoryLabel);
    }

    private TaskBarElement CreateTaskBarElement(GanttTask task, double pixelsPerDay, double y)
    {
        var startX = (task.StartDate - TimelineStart).TotalDays * pixelsPerDay;
        var duration = (task.EndDate - task.StartDate).TotalDays;
        var width = Math.Max(20, duration * pixelsPerDay); // Minimum width for touch

        var container = new Border
        {
            Width = width,
            Height = TASK_BAR_HEIGHT,
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            Tag = task
        };

        // Set colors based on task status and dependencies
        var (fillBrush, borderBrush) = GetTaskColors(task);
        container.Background = fillBrush;
        container.BorderBrush = borderBrush;

        // Add visual indicators for task relationships
        ApplyTaskRelationshipStyling(container, task);

        // Create the main content grid
        var contentGrid = new Grid();

        // Progress indicator
        if (task.Progress > 0)
        {
            var progressWidth = (width - 2) * (task.Progress / 100.0);
            var progressBar = new Rectangle
            {
                Width = progressWidth,
                Height = TASK_BAR_HEIGHT - 2,
                Fill = new SolidColorBrush(Color.FromArgb(100, 0, 123, 255)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            contentGrid.Children.Add(progressBar);
        }

        // Task label
        var labelContainer = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0)
        };

        var taskLabel = new TextBlock
        {
            Text = task.Name,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        var progressLabel = new TextBlock
        {
            Text = $" ({task.Progress}%)",
            FontSize = 10,
            Foreground = Brushes.LightGray,
            VerticalAlignment = VerticalAlignment.Center
        };

        labelContainer.Children.Add(taskLabel);
        if (task.Progress > 0)
        {
            labelContainer.Children.Add(progressLabel);
        }

        contentGrid.Children.Add(labelContainer);
        container.Child = contentGrid;

        Canvas.SetLeft(container, startX);
        Canvas.SetTop(container, y);

        // Add touch events
        container.MouseLeftButtonDown += (s, e) => OnTaskBarMouseDown(task, container, e);
        container.MouseMove += (s, e) => OnTaskBarMouseMove(task, container, e);
        container.MouseLeftButtonUp += (s, e) => OnTaskBarMouseUp(task, container, e);
        container.TouchDown += (s, e) => OnTaskBarTouched(task, e);

        return new TaskBarElement(task, container);
    }

    private void DrawDependencyLines()
    {
        if (Tasks == null) return;

        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        if (totalDays <= 0) return;

        var pixelsPerDay = (ActualWidth * ZoomLevel) / totalDays;

        // Draw dependency lines for each task
        for (int i = 0; i < Tasks.Count; i++)
        {
            var task = Tasks[i];
            if (task.Dependencies == null || task.Dependencies.Count == 0) continue;

            foreach (var dependency in task.Dependencies)
            {
                var dependencyIndex = Tasks.IndexOf(dependency);
                if (dependencyIndex == -1) continue;

                DrawDependencyLine(task, dependency, i, dependencyIndex, pixelsPerDay);
            }
        }
    }

    private void DrawDependencyLine(GanttTask task, GanttTask dependency, int taskIndex, int dependencyIndex, double pixelsPerDay)
    {
        // Calculate positions
        var taskY = TIMELINE_HEADER_HEIGHT + (taskIndex * TASK_ROW_HEIGHT) + (TASK_ROW_HEIGHT / 2);
        var dependencyY = TIMELINE_HEADER_HEIGHT + (dependencyIndex * TASK_ROW_HEIGHT) + (TASK_ROW_HEIGHT / 2);

        var dependencyEndX = (dependency.EndDate - TimelineStart).TotalDays * pixelsPerDay;
        var taskStartX = (task.StartDate - TimelineStart).TotalDays * pixelsPerDay;

        // Create dependency line path (L-shaped arrow)
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = new Point(dependencyEndX, dependencyY) };

        // Horizontal line from dependency end
        var horizontalLength = Math.Max(20, (taskStartX - dependencyEndX) / 2);
        pathFigure.Segments.Add(new LineSegment(new Point(dependencyEndX + horizontalLength, dependencyY), true));

        // Vertical line toward task
        pathFigure.Segments.Add(new LineSegment(new Point(dependencyEndX + horizontalLength, taskY), true));

        // Final horizontal line to task start
        pathFigure.Segments.Add(new LineSegment(new Point(taskStartX, taskY), true));

        pathGeometry.Figures.Add(pathFigure);

        // Style dependency lines differently based on task relationships
        var strokeColor = Color.FromRgb(52, 144, 220); // Default blue
        var strokeThickness = 2.0;
        var dashArray = new DoubleCollection { 5, 3 };

        // Parent-child relationships use solid lines
        if (task.ParentTask != null && task.ParentTask == dependency)
        {
            strokeColor = Color.FromRgb(108, 117, 125); // Gray for hierarchy
            dashArray = null; // Solid line
            strokeThickness = 3.0;
        }
        // Critical path uses thicker red lines
        else if (dependency.Priority == TaskPriority.Critical || task.Priority == TaskPriority.Critical)
        {
            strokeColor = Color.FromRgb(220, 53, 69); // Red for critical
            strokeThickness = 3.0;
        }

        var dependencyPath = new Path
        {
            Data = pathGeometry,
            Stroke = new SolidColorBrush(strokeColor),
            StrokeThickness = strokeThickness,
            StrokeDashArray = dashArray
        };

        Children.Add(dependencyPath);
        _dependencyLines.Add(dependencyPath);

        // Add arrowhead at the end
        DrawArrowHead(taskStartX, taskY);
    }

    private void DrawArrowHead(double x, double y)
    {
        var arrowGeometry = new PathGeometry();
        var arrowFigure = new PathFigure { StartPoint = new Point(x - 8, y - 4) };
        arrowFigure.Segments.Add(new LineSegment(new Point(x, y), true));
        arrowFigure.Segments.Add(new LineSegment(new Point(x - 8, y + 4), true));
        arrowGeometry.Figures.Add(arrowFigure);

        var arrowPath = new Path
        {
            Data = arrowGeometry,
            Stroke = new SolidColorBrush(Color.FromRgb(52, 144, 220)),
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromRgb(52, 144, 220))
        };

        Children.Add(arrowPath);
        _dependencyLines.Add(arrowPath);
    }

    #endregion

    #region Touch and Mouse Interaction

    private void OnManipulationStarted(object? sender, ManipulationStartedEventArgs e)
    {
        _isPanning = true;
        _lastPanPoint = e.ManipulationOrigin;
        CaptureMouse();
    }

    private void OnManipulationDelta(object? sender, ManipulationDeltaEventArgs e)
    {
        if (!_isPanning) return;

        var deltaX = e.DeltaManipulation.Translation.X;
        var deltaY = e.DeltaManipulation.Translation.Y;

        // Pan the timeline
        var scrollViewer = FindScrollViewer();
        if (scrollViewer != null)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - deltaX);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - deltaY);
        }

        // Handle zoom
        if (Math.Abs(e.DeltaManipulation.Scale.X - 1.0) > 0.1)
        {
            var newZoom = ZoomLevel * e.DeltaManipulation.Scale.X;
            ZoomLevel = Math.Max(0.1, Math.Min(5.0, newZoom));
        }
    }

    private void OnManipulationCompleted(object? sender, ManipulationCompletedEventArgs e)
    {
        _isPanning = false;
        ReleaseMouseCapture();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isPanning = true;
        _lastPanPoint = e.GetPosition(this);
        CaptureMouse();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning || e.LeftButton != MouseButtonState.Pressed) return;

        var currentPoint = e.GetPosition(this);
        var deltaX = currentPoint.X - _lastPanPoint.X;
        var deltaY = currentPoint.Y - _lastPanPoint.Y;

        var scrollViewer = FindScrollViewer();
        if (scrollViewer != null)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - deltaX);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - deltaY);
        }

        _lastPanPoint = currentPoint;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isPanning = false;
        ReleaseMouseCapture();
    }

    // MouseWheel zoom removed - zoom only via buttons in toolbar

    private void OnTaskBarMouseDown(GanttTask task, Border container, MouseButtonEventArgs e)
    {
        var now = DateTime.Now;
        var timeSinceLastClick = now - _lastClickTime;
        
        // Check for double-click (within 500ms and same task)
        if (timeSinceLastClick.TotalMilliseconds < 500 && _lastClickedTask == task)
        {
            TaskDoubleClicked?.Invoke(this, task);
            e.Handled = true;
            return;
        }
        
        // Start drag operation
        _isDraggingTask = true;
        _draggingTask = task;
        _draggedTaskBar = container;
        _dragStartPoint = e.GetPosition(this);
        container.CaptureMouse();
        
        SelectedTask = task;
        _lastClickTime = now;
        _lastClickedTask = task;
        e.Handled = true;
    }

    private void OnTaskBarMouseMove(GanttTask task, Border container, MouseEventArgs e)
    {
        if (!_isDraggingTask || _draggingTask != task || !container.IsMouseCaptured)
            return;

        var currentPoint = e.GetPosition(this);
        var deltaX = currentPoint.X - _dragStartPoint.X;
        
        // Calculate new position
        var currentLeft = Canvas.GetLeft(container);
        var newLeft = Math.Max(0, currentLeft + deltaX);
        
        // Update visual position
        Canvas.SetLeft(container, newLeft);
        _dragStartPoint = currentPoint;
        
        e.Handled = true;
    }

    private void OnTaskBarMouseUp(GanttTask task, Border container, MouseEventArgs e)
    {
        if (!_isDraggingTask || _draggingTask != task)
            return;

        _isDraggingTask = false;
        container.ReleaseMouseCapture();
        
        // Calculate new dates based on position
        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        if (totalDays <= 0)
        {
            _draggingTask = null;
            _draggedTaskBar = null;
            return;
        }

        var pixelsPerDay = (ActualWidth * ZoomLevel) / totalDays;
        var currentLeft = Canvas.GetLeft(container);
        var dayOffset = currentLeft / pixelsPerDay;
        
        var duration = task.EndDate - task.StartDate;
        var newStartDate = TimelineStart.AddDays(dayOffset);
        var newEndDate = newStartDate.Add(duration);
        
        // Fire the event to update the task
        TaskDatesChanged?.Invoke(this, new TaskDatesChangedEventArgs(task, newStartDate, newEndDate));
        
        _draggingTask = null;
        _draggedTaskBar = null;
        e.Handled = true;
    }

    private void OnTaskBarTouched(GanttTask task, TouchEventArgs e)
    {
        SelectedTask = task;
        e.Handled = true;
    }

    #endregion

    #region Helper Methods

    private (SolidColorBrush fill, SolidColorBrush border) GetTaskColors(GanttTask task)
    {
        return task.Status switch
        {
            TouchGanttChart.Models.TaskStatus.NotStarted => (
                new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                new SolidColorBrush(Color.FromRgb(73, 80, 87))
            ),
            TouchGanttChart.Models.TaskStatus.InProgress => (
                new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                new SolidColorBrush(Color.FromRgb(0, 86, 179))
            ),
            TouchGanttChart.Models.TaskStatus.Completed => (
                new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                new SolidColorBrush(Color.FromRgb(33, 136, 56))
            ),
            TouchGanttChart.Models.TaskStatus.OnHold => (
                new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                new SolidColorBrush(Color.FromRgb(227, 172, 9))
            ),
            TouchGanttChart.Models.TaskStatus.Cancelled => (
                new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                new SolidColorBrush(Color.FromRgb(176, 42, 55))
            ),
            _ => (
                new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                new SolidColorBrush(Color.FromRgb(73, 80, 87))
            )
        };
    }

    private void ApplyTaskRelationshipStyling(Border container, GanttTask task)
    {
        // Apply different styling based on task relationships
        if (task.ParentTask != null)
        {
            // This is a subtask - use thicker border with different color
            container.BorderThickness = new Thickness(2);
            container.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Yellow for subtasks
        }
        else if (task.SubTasks.Count > 0)
        {
            // This is a parent task - use thicker border
            container.BorderThickness = new Thickness(3);
        }
        else if (task.Dependencies != null && task.Dependencies.Count > 0)
        {
            // This task has dependencies - use thinner border with blue color
            container.BorderThickness = new Thickness(1);
            container.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 123, 255)); // Blue for dependent tasks
        }

        // Add left border indicator for priority
        if (task.Priority == TaskPriority.Critical)
        {
            var priorityIndicator = new Border
            {
                Width = 4,
                Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)), // Red for critical
                HorizontalAlignment = HorizontalAlignment.Left
            };
            
            if (container.Child is Grid grid)
            {
                grid.Children.Insert(0, priorityIndicator);
            }
        }
        else if (task.Priority == TaskPriority.High)
        {
            var priorityIndicator = new Border
            {
                Width = 4,
                Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)), // Yellow for high
                HorizontalAlignment = HorizontalAlignment.Left
            };
            
            if (container.Child is Grid grid)
            {
                grid.Children.Insert(0, priorityIndicator);
            }
        }

        // Add completion indicator for early/late completion
        if (task.CompletionDate.HasValue)
        {
            var completionIndicator = new Ellipse
            {
                Width = 12,
                Height = 12,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, -6, -6, 0)
            };

            if (task.IsCompletedEarly)
            {
                completionIndicator.Fill = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // Green
            }
            else if (task.CompletionVarianceDays < 0)
            {
                completionIndicator.Fill = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Red
            }
            else
            {
                completionIndicator.Fill = new SolidColorBrush(Color.FromRgb(0, 123, 255)); // Blue
            }

            if (container.Child is Grid grid)
            {
                grid.Children.Add(completionIndicator);
            }
        }
    }

    private void UpdateTaskSelection()
    {
        foreach (var taskBar in _taskBars)
        {
            var isSelected = taskBar.Task == SelectedTask;
            var border = taskBar.Container;
            
            if (isSelected)
            {
                border.BorderThickness = new Thickness(3);
                border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Blue,
                    BlurRadius = 5,
                    Opacity = 0.5,
                    ShadowDepth = 0
                };
            }
            else
            {
                border.BorderThickness = new Thickness(1);
                border.Effect = null;
            }
        }
    }

    private void UpdateCanvasSize()
    {
        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        var taskCount = Tasks?.Count ?? 0;
        
        // Calculate required width based on timeline and zoom - use proper scaling
        var pixelsPerDay = ZoomLevel * 40; // 40 pixels per day at 1.0 zoom
        var requiredWidth = Math.Max(800, totalDays * pixelsPerDay);
        Width = requiredWidth;
        MinWidth = requiredWidth;
        
        // Calculate height based on categories and tasks
        if (Tasks != null && Tasks.Count > 0)
        {
            var categoryCount = Tasks.GroupBy(t => t.Category).Count();
            var totalHeight = TIMELINE_HEADER_HEIGHT + 
                             (categoryCount * (CATEGORY_HEADER_HEIGHT + CATEGORY_SPACING * 2)) + 
                             (taskCount * TASK_ROW_HEIGHT) + 20;
            Height = Math.Max(400, totalHeight);
            MinHeight = Math.Max(400, totalHeight);
        }
        else
        {
            Height = Math.Max(400, TIMELINE_HEADER_HEIGHT + 100);
            MinHeight = Math.Max(400, TIMELINE_HEADER_HEIGHT + 100);
        }
        
        // Force layout update to ensure ScrollViewer recognizes new size
        InvalidateMeasure();
        InvalidateArrange();
    }

    private ScrollViewer? FindScrollViewer()
    {
        var parent = Parent;
        while (parent != null && !(parent is ScrollViewer))
        {
            parent = LogicalTreeHelper.GetParent(parent);
        }
        return parent as ScrollViewer;
    }

    private void ApplyViewModeSettings()
    {
        var (minZoom, maxZoom) = TimelineViewConfig.GetZoomRange(ViewMode);
        ZoomLevel = Math.Max(minZoom, Math.Min(maxZoom, ZoomLevel));
    }

    private (bool isMajor, bool isMinor) GetGridLineType(DateTime date)
    {
        return ViewMode switch
        {
            TimelineViewMode.Daily => (date.Day == 1, date.DayOfWeek == DayOfWeek.Monday),
            TimelineViewMode.Weekly => (date.Day == 1, date.DayOfWeek == DayOfWeek.Monday),
            TimelineViewMode.Monthly => (date.Month == 1, date.Day == 1),
            TimelineViewMode.Quarterly => (date.Month == 1, date.Month % 3 == 1),
            TimelineViewMode.Yearly => (date.Year % 5 == 0, date.Month == 1),
            _ => (date.Day == 1, date.DayOfWeek == DayOfWeek.Monday)
        };
    }

    private DateTime GetNextGridDate(DateTime current)
    {
        return ViewMode switch
        {
            TimelineViewMode.Daily => current.AddDays(1),
            TimelineViewMode.Weekly => current.AddDays(1),
            TimelineViewMode.Monthly => current.AddDays(7),
            TimelineViewMode.Quarterly => current.AddMonths(1),
            TimelineViewMode.Yearly => current.AddMonths(3),
            _ => current.AddDays(1)
        };
    }

    private void DrawDailyHeader(double pixelsPerDay)
    {
        // Month header
        var currentMonth = new DateTime(TimelineStart.Year, TimelineStart.Month, 1);
        while (currentMonth <= TimelineEnd)
        {
            var nextMonth = currentMonth.AddMonths(1);
            var monthEnd = nextMonth > TimelineEnd ? TimelineEnd : nextMonth;
            
            var startX = Math.Max(0, (currentMonth - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (monthEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 60)
            {
                CreateHeaderLabel(currentMonth.ToString("MMM yyyy"), startX, width, 8, 14, FontWeights.SemiBold, Brushes.DarkBlue);
            }
            currentMonth = nextMonth;
        }

        // Day labels
        var currentDate = TimelineStart.Date;
        while (currentDate <= TimelineEnd)
        {
            var x = (currentDate - TimelineStart).TotalDays * pixelsPerDay;
            var dayWidth = pixelsPerDay;

            if (dayWidth > 25)
            {
                CreateHeaderLabel(currentDate.ToString("dd"), x, dayWidth, 32, 11, FontWeights.Normal, Brushes.Gray);
            }
            currentDate = currentDate.AddDays(1);
        }
    }

    private void DrawWeeklyHeader(double pixelsPerDay)
    {
        // Month header
        var currentMonth = new DateTime(TimelineStart.Year, TimelineStart.Month, 1);
        while (currentMonth <= TimelineEnd)
        {
            var nextMonth = currentMonth.AddMonths(1);
            var monthEnd = nextMonth > TimelineEnd ? TimelineEnd : nextMonth;
            
            var startX = Math.Max(0, (currentMonth - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (monthEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 60)
            {
                CreateHeaderLabel(currentMonth.ToString("MMM yyyy"), startX, width, 8, 14, FontWeights.SemiBold, Brushes.DarkBlue);
            }
            currentMonth = nextMonth;
        }

        // Week labels
        var currentWeek = TimelineStart.Date;
        while (currentWeek.DayOfWeek != DayOfWeek.Monday && currentWeek > TimelineStart.AddDays(-7))
            currentWeek = currentWeek.AddDays(-1);

        while (currentWeek <= TimelineEnd)
        {
            var weekEnd = currentWeek.AddDays(7);
            var actualWeekEnd = weekEnd > TimelineEnd ? TimelineEnd : weekEnd;
            
            var startX = Math.Max(0, (currentWeek - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (actualWeekEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 40)
            {
                var weekLabel = $"Week {GetWeekOfYear(currentWeek)} ({currentWeek:MMM dd})";
                CreateHeaderLabel(weekLabel, startX, width, 32, 10, FontWeights.Normal, Brushes.Gray);
            }
            currentWeek = currentWeek.AddDays(7);
        }
    }

    private void DrawMonthlyHeader(double pixelsPerDay)
    {
        // Year header
        var currentYear = TimelineStart.Year;
        while (currentYear <= TimelineEnd.Year)
        {
            var yearStart = new DateTime(currentYear, 1, 1);
            var yearEnd = new DateTime(currentYear, 12, 31);
            
            var startX = Math.Max(0, (yearStart - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (yearEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 80)
            {
                CreateHeaderLabel(currentYear.ToString(), startX, width, 8, 16, FontWeights.Bold, Brushes.DarkBlue);
            }
            currentYear++;
        }

        // Month labels
        var currentMonth = new DateTime(TimelineStart.Year, TimelineStart.Month, 1);
        while (currentMonth <= TimelineEnd)
        {
            var nextMonth = currentMonth.AddMonths(1);
            var monthEnd = nextMonth > TimelineEnd ? TimelineEnd : nextMonth;
            
            var startX = Math.Max(0, (currentMonth - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (monthEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 30)
            {
                CreateHeaderLabel(currentMonth.ToString("MMM"), startX, width, 32, 12, FontWeights.Normal, Brushes.Gray);
            }
            currentMonth = nextMonth;
        }
    }

    private void DrawQuarterlyHeader(double pixelsPerDay)
    {
        // Year header
        var currentYear = TimelineStart.Year;
        while (currentYear <= TimelineEnd.Year)
        {
            var yearStart = new DateTime(currentYear, 1, 1);
            var yearEnd = new DateTime(currentYear, 12, 31);
            
            var startX = Math.Max(0, (yearStart - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (yearEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 100)
            {
                CreateHeaderLabel(currentYear.ToString(), startX, width, 8, 16, FontWeights.Bold, Brushes.DarkBlue);
            }
            currentYear++;
        }

        // Quarter labels
        var currentQuarter = new DateTime(TimelineStart.Year, ((TimelineStart.Month - 1) / 3) * 3 + 1, 1);
        while (currentQuarter <= TimelineEnd)
        {
            var nextQuarter = currentQuarter.AddMonths(3);
            var quarterEnd = nextQuarter > TimelineEnd ? TimelineEnd : nextQuarter;
            
            var startX = Math.Max(0, (currentQuarter - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (quarterEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 50)
            {
                var quarter = (currentQuarter.Month - 1) / 3 + 1;
                CreateHeaderLabel($"Q{quarter}", startX, width, 32, 12, FontWeights.Normal, Brushes.Gray);
            }
            currentQuarter = nextQuarter;
        }
    }

    private void DrawYearlyHeader(double pixelsPerDay)
    {
        // Decade header (optional for very long projects)
        var currentDecade = (TimelineStart.Year / 10) * 10;
        while (currentDecade <= TimelineEnd.Year)
        {
            var decadeStart = new DateTime(currentDecade, 1, 1);
            var decadeEnd = new DateTime(currentDecade + 9, 12, 31);
            
            var startX = Math.Max(0, (decadeStart - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (decadeEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 120)
            {
                CreateHeaderLabel($"{currentDecade}s", startX, width, 8, 16, FontWeights.Bold, Brushes.DarkBlue);
            }
            currentDecade += 10;
        }

        // Year labels
        var currentYear = TimelineStart.Year;
        while (currentYear <= TimelineEnd.Year)
        {
            var yearStart = new DateTime(currentYear, 1, 1);
            var yearEnd = new DateTime(currentYear, 12, 31);
            
            var startX = Math.Max(0, (yearStart - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (yearEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 40)
            {
                CreateHeaderLabel(currentYear.ToString(), startX, width, 32, 12, FontWeights.Normal, Brushes.Gray);
            }
            currentYear++;
        }
    }

    private void CreateHeaderLabel(string text, double startX, double width, double topOffset, double fontSize, FontWeight fontWeight, Brush foreground)
    {
        var label = new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            FontWeight = fontWeight,
            Foreground = foreground,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Canvas.SetLeft(label, startX + (width - text.Length * fontSize * 0.6) / 2);
        Canvas.SetTop(label, topOffset);
        Children.Add(label);
        _timelineLabels.Add(label);
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var jan1 = new DateTime(date.Year, 1, 1);
        var daysOffset = (int)jan1.DayOfWeek;
        var firstWeek = jan1.AddDays(-daysOffset);
        var weekNum = (int)Math.Ceiling((date - firstWeek).TotalDays / 7.0);
        return weekNum;
    }

    #endregion

    private record TaskBarElement(GanttTask Task, Border Container);
}