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
    private const double GRID_LINE_THICKNESS = 1;
    private const double MAJOR_GRID_LINE_THICKNESS = 2;

    private readonly List<TaskBarElement> _taskBars = new();
    private readonly List<Line> _gridLines = new();
    private readonly List<TextBlock> _timelineLabels = new();
    private Point _lastPanPoint;
    private bool _isPanning;

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
        MouseWheel += OnMouseWheel;
        
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

    private void OnTasksCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RedrawTimeline();
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
        UpdateCanvasSize();
    }

    private void ClearCanvas()
    {
        Children.Clear();
        _taskBars.Clear();
        _gridLines.Clear();
        _timelineLabels.Clear();
    }

    private void DrawTimelineGrid()
    {
        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        if (totalDays <= 0) return;

        var pixelsPerDay = (ActualWidth * ZoomLevel) / totalDays;
        var currentDate = TimelineStart;

        // Draw vertical grid lines
        while (currentDate <= TimelineEnd)
        {
            var x = (currentDate - TimelineStart).TotalDays * pixelsPerDay;
            
            var isWeekStart = currentDate.DayOfWeek == DayOfWeek.Monday;
            var isMonthStart = currentDate.Day == 1;
            
            var thickness = isMonthStart ? MAJOR_GRID_LINE_THICKNESS : 
                           isWeekStart ? GRID_LINE_THICKNESS * 1.5 : GRID_LINE_THICKNESS;
            
            var brush = isMonthStart ? Brushes.DarkGray :
                       isWeekStart ? Brushes.Gray : Brushes.LightGray;

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

            currentDate = currentDate.AddDays(1);
        }

        // Draw horizontal grid lines for task rows
        var taskCount = Tasks?.Count ?? 0;
        for (int i = 0; i <= taskCount; i++)
        {
            var y = TIMELINE_HEADER_HEIGHT + (i * TASK_ROW_HEIGHT);
            
            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = ActualWidth * ZoomLevel,
                Y2 = y,
                Stroke = Brushes.LightGray,
                StrokeThickness = GRID_LINE_THICKNESS
            };

            Children.Add(line);
            _gridLines.Add(line);
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

        // Month labels
        var currentMonth = new DateTime(TimelineStart.Year, TimelineStart.Month, 1);
        while (currentMonth <= TimelineEnd)
        {
            var nextMonth = currentMonth.AddMonths(1);
            var monthEnd = nextMonth > TimelineEnd ? TimelineEnd : nextMonth;
            
            var startX = Math.Max(0, (currentMonth - TimelineStart).TotalDays * pixelsPerDay);
            var endX = (monthEnd - TimelineStart).TotalDays * pixelsPerDay;
            var width = endX - startX;

            if (width > 60) // Only show if there's enough space
            {
                var monthLabel = new TextBlock
                {
                    Text = currentMonth.ToString("MMM yyyy"),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.DarkBlue,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Canvas.SetLeft(monthLabel, startX + (width - 80) / 2);
                Canvas.SetTop(monthLabel, 8);
                Children.Add(monthLabel);
                _timelineLabels.Add(monthLabel);
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

            if (width > 40) // Only show if there's enough space
            {
                var weekLabel = new TextBlock
                {
                    Text = $"Week {GetWeekOfYear(currentWeek)}",
                    FontSize = 10,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Canvas.SetLeft(weekLabel, startX + (width - 50) / 2);
                Canvas.SetTop(weekLabel, 32);
                Children.Add(weekLabel);
                _timelineLabels.Add(weekLabel);
            }

            currentWeek = currentWeek.AddDays(7);
        }
    }

    private void DrawTaskBars()
    {
        if (Tasks == null) return;

        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        if (totalDays <= 0) return;

        var pixelsPerDay = (ActualWidth * ZoomLevel) / totalDays;

        for (int i = 0; i < Tasks.Count; i++)
        {
            var task = Tasks[i];
            var y = TIMELINE_HEADER_HEIGHT + (i * TASK_ROW_HEIGHT) + (TASK_ROW_HEIGHT - TASK_BAR_HEIGHT) / 2;

            var taskElement = CreateTaskBarElement(task, pixelsPerDay, y);
            _taskBars.Add(taskElement);
            Children.Add(taskElement.Container);
        }
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

        // Set colors based on task status
        var (fillBrush, borderBrush) = GetTaskColors(task);
        container.Background = fillBrush;
        container.BorderBrush = borderBrush;

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
            container.Child = progressBar;
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

        if (container.Child == null)
        {
            container.Child = labelContainer;
        }
        else
        {
            var grid = new Grid();
            grid.Children.Add((UIElement)container.Child);
            grid.Children.Add(labelContainer);
            container.Child = grid;
        }

        Canvas.SetLeft(container, startX);
        Canvas.SetTop(container, y);

        // Add touch events
        container.MouseLeftButtonDown += (s, e) => OnTaskBarClicked(task, e);
        container.TouchDown += (s, e) => OnTaskBarTouched(task, e);

        return new TaskBarElement(task, container);
    }

    #endregion

    #region Touch and Mouse Interaction

    private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
        _isPanning = true;
        _lastPanPoint = e.ManipulationOrigin;
        CaptureMouse();
    }

    private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
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

    private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
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

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
        var newZoom = ZoomLevel * zoomFactor;
        ZoomLevel = Math.Max(0.1, Math.Min(5.0, newZoom));
    }

    private void OnTaskBarClicked(GanttTask task, MouseButtonEventArgs e)
    {
        SelectedTask = task;
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
        
        Width = Math.Max(ActualWidth, totalDays * ZoomLevel * 2);
        Height = Math.Max(ActualHeight, TIMELINE_HEADER_HEIGHT + (taskCount * TASK_ROW_HEIGHT) + 20);
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