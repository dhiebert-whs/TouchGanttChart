using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using TouchGanttChart.Services.Interfaces;

namespace TouchGanttChart.Services.Implementations;

/// <summary>
/// Implementation of touch gesture handling for the Gantt chart interface.
/// Provides multi-touch support optimized for large displays and cleartouch boards.
/// </summary>
public class TouchGestureService : ITouchGestureService
{
    private readonly ILogger<TouchGestureService> _logger;
    private FrameworkElement? _targetElement;
    private DateTime _lastTouchTime;
    private Point _lastTouchPoint;
    private bool _isDragging;
    private Point _dragStartPoint;
    private readonly System.Timers.Timer _longPressTimer;

    /// <summary>
    /// Initializes a new instance of the TouchGestureService class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TouchGestureService(ILogger<TouchGestureService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Initialize properties with touch-optimized defaults
        MinZoom = 0.1;
        MaxZoom = 5.0;
        CurrentZoom = 1.0;
        PanSensitivity = 1.0;
        ZoomSensitivity = 1.0;
        LongPressDuration = 500; // milliseconds
        IsMomentumScrollingEnabled = true;
        IsMultiTouchEnabled = true;

        _longPressTimer = new System.Timers.Timer();
        _longPressTimer.Elapsed += OnLongPressTimerElapsed;
        _longPressTimer.AutoReset = false;

        _logger.LogInformation("TouchGestureService initialized with touch-optimized settings");
    }

    #region Properties

    /// <inheritdoc/>
    public double MinZoom { get; set; }

    /// <inheritdoc/>
    public double MaxZoom { get; set; }

    /// <inheritdoc/>
    public double CurrentZoom { get; set; }

    /// <inheritdoc/>
    public double PanSensitivity { get; set; }

    /// <inheritdoc/>
    public double ZoomSensitivity { get; set; }

    /// <inheritdoc/>
    public int LongPressDuration { get; set; }

    /// <inheritdoc/>
    public bool IsMomentumScrollingEnabled { get; set; }

    /// <inheritdoc/>
    public bool IsMultiTouchEnabled { get; set; }

    #endregion

    #region Events

    /// <inheritdoc/>
    public event EventHandler<TouchGestureEventArgs>? GestureDetected;

    /// <inheritdoc/>
    public event EventHandler<ZoomChangedEventArgs>? ZoomChanged;

    /// <inheritdoc/>
    public event EventHandler<PanEventArgs>? Panned;

    /// <inheritdoc/>
    public event EventHandler<TaskSelectedEventArgs>? TaskSelected;

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public void InitializeTouchGestures(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        _targetElement = element;
        
        // Enable manipulation events for touch gestures
        element.IsManipulationEnabled = true;
        element.ManipulationDelta += OnManipulationDelta;
        element.ManipulationStarted += OnManipulationStarted;
        element.ManipulationCompleted += OnManipulationCompleted;
        
        // Enable touch events for additional gesture detection
        element.TouchDown += OnTouchDown;
        element.TouchUp += OnTouchUp;
        element.TouchMove += OnTouchMove;
        
        // Enable mouse events for non-touch devices
        element.MouseDown += OnMouseDown;
        element.MouseUp += OnMouseUp;
        element.MouseMove += OnMouseMove;
        element.MouseWheel += OnMouseWheel;

        _logger.LogInformation("Touch gestures initialized for element: {ElementType}", element.GetType().Name);
    }

    /// <inheritdoc/>
    public void HandlePanGesture(double deltaX, double deltaY)
    {
        try
        {
            var adjustedDeltaX = deltaX * PanSensitivity;
            var adjustedDeltaY = deltaY * PanSensitivity;

            _logger.LogDebug("Handling pan gesture: deltaX={DeltaX}, deltaY={DeltaY}", adjustedDeltaX, adjustedDeltaY);

            var panArgs = new PanEventArgs
            {
                DeltaX = adjustedDeltaX,
                DeltaY = adjustedDeltaY,
                IsComplete = false
            };

            Panned?.Invoke(this, panArgs);

            var gestureArgs = new TouchGestureEventArgs
            {
                GestureType = TouchGestureType.Pan,
                Position = _lastTouchPoint,
                Delta = Math.Sqrt(adjustedDeltaX * adjustedDeltaX + adjustedDeltaY * adjustedDeltaY)
            };

            GestureDetected?.Invoke(this, gestureArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling pan gesture");
        }
    }

    /// <inheritdoc/>
    public void HandleZoomGesture(double scaleFactor, Point centerPoint)
    {
        try
        {
            var adjustedScale = 1.0 + ((scaleFactor - 1.0) * ZoomSensitivity);
            var oldZoom = CurrentZoom;
            var newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, CurrentZoom * adjustedScale));

            if (Math.Abs(newZoom - CurrentZoom) > 0.001)
            {
                CurrentZoom = newZoom;

                _logger.LogDebug("Handling zoom gesture: scale={ScaleFactor}, oldZoom={OldZoom}, newZoom={NewZoom}", 
                    scaleFactor, oldZoom, newZoom);

                var zoomArgs = new ZoomChangedEventArgs
                {
                    OldZoom = oldZoom,
                    NewZoom = newZoom,
                    CenterPoint = centerPoint
                };

                ZoomChanged?.Invoke(this, zoomArgs);

                var gestureArgs = new TouchGestureEventArgs
                {
                    GestureType = TouchGestureType.Zoom,
                    Position = centerPoint,
                    Delta = newZoom - oldZoom
                };

                GestureDetected?.Invoke(this, gestureArgs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling zoom gesture");
        }
    }

    /// <inheritdoc/>
    public void HandleTapGesture(Point tapPoint, bool isDoubleTap = false)
    {
        try
        {
            _logger.LogDebug("Handling tap gesture at ({X}, {Y}), isDoubleTap={IsDoubleTap}", 
                tapPoint.X, tapPoint.Y, isDoubleTap);

            var gestureType = isDoubleTap ? TouchGestureType.DoubleTap : TouchGestureType.Tap;

            var taskArgs = new TaskSelectedEventArgs
            {
                SelectionPoint = tapPoint,
                IsDoubleClick = isDoubleTap,
                SelectedTask = null // Will be determined by the UI layer
            };

            TaskSelected?.Invoke(this, taskArgs);

            var gestureArgs = new TouchGestureEventArgs
            {
                GestureType = gestureType,
                Position = tapPoint,
                Delta = 0
            };

            GestureDetected?.Invoke(this, gestureArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling tap gesture");
        }
    }

    /// <inheritdoc/>
    public void HandleLongPressGesture(Point pressPoint)
    {
        try
        {
            _logger.LogDebug("Handling long press gesture at ({X}, {Y})", pressPoint.X, pressPoint.Y);

            var gestureArgs = new TouchGestureEventArgs
            {
                GestureType = TouchGestureType.LongPress,
                Position = pressPoint,
                Delta = 0
            };

            GestureDetected?.Invoke(this, gestureArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling long press gesture");
        }
    }

    /// <inheritdoc/>
    public void HandleDragGesture(Point startPoint, Point currentPoint, bool isComplete = false)
    {
        try
        {
            var deltaX = currentPoint.X - startPoint.X;
            var deltaY = currentPoint.Y - startPoint.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            _logger.LogDebug("Handling drag gesture: start=({StartX}, {StartY}), current=({CurrentX}, {CurrentY}), complete={IsComplete}", 
                startPoint.X, startPoint.Y, currentPoint.X, currentPoint.Y, isComplete);

            var gestureArgs = new TouchGestureEventArgs
            {
                GestureType = TouchGestureType.Drag,
                Position = currentPoint,
                Delta = distance
            };

            GestureDetected?.Invoke(this, gestureArgs);

            if (isComplete)
            {
                _isDragging = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling drag gesture");
        }
    }

    #endregion

    #region Event Handlers

    private void OnManipulationStarted(object? sender, ManipulationStartedEventArgs e)
    {
        _logger.LogDebug("Manipulation started at ({X}, {Y})", e.ManipulationOrigin.X, e.ManipulationOrigin.Y);
        _lastTouchPoint = e.ManipulationOrigin;
        _lastTouchTime = DateTime.Now;
    }

    private void OnManipulationDelta(object? sender, ManipulationDeltaEventArgs e)
    {
        try
        {
            var deltaTranslation = e.DeltaManipulation.Translation;
            var deltaScale = e.DeltaManipulation.Scale;

            // Handle panning
            if (Math.Abs(deltaTranslation.X) > 1 || Math.Abs(deltaTranslation.Y) > 1)
            {
                HandlePanGesture(deltaTranslation.X, deltaTranslation.Y);
            }

            // Handle zooming (pinch gestures)
            if (IsMultiTouchEnabled && Math.Abs(deltaScale.X - 1.0) > 0.01)
            {
                var scaleFactor = (deltaScale.X + deltaScale.Y) / 2.0;
                HandleZoomGesture(scaleFactor, e.ManipulationOrigin);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in manipulation delta handler");
        }
    }

    private void OnManipulationCompleted(object? sender, ManipulationCompletedEventArgs e)
    {
        try
        {
            _logger.LogDebug("Manipulation completed");

            // If momentum scrolling is enabled, handle inertia
            if (IsMomentumScrollingEnabled && e.FinalVelocities.LinearVelocity.Length > 50)
            {
                _logger.LogDebug("Applying momentum scrolling with velocity: {Velocity}", 
                    e.FinalVelocities.LinearVelocity.Length);
                
                // TODO: Implement momentum scrolling animation
            }

            // Complete any ongoing pan gesture
            var panArgs = new PanEventArgs
            {
                DeltaX = 0,
                DeltaY = 0,
                IsComplete = true
            };

            Panned?.Invoke(this, panArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in manipulation completed handler");
        }
    }

    private void OnTouchDown(object? sender, TouchEventArgs e)
    {
        try
        {
            var touchPoint = e.GetTouchPoint(_targetElement);
            _lastTouchPoint = touchPoint.Position;
            _lastTouchTime = DateTime.Now;

            // Start long press timer
            _longPressTimer.Interval = LongPressDuration;
            _longPressTimer.Start();

            _logger.LogDebug("Touch down at ({X}, {Y})", touchPoint.Position.X, touchPoint.Position.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in touch down handler");
        }
    }

    private void OnTouchUp(object? sender, TouchEventArgs e)
    {
        try
        {
            _longPressTimer.Stop();

            var touchPoint = e.GetTouchPoint(_targetElement);
            var currentTime = DateTime.Now;
            var timeDiff = (currentTime - _lastTouchTime).TotalMilliseconds;

            // Determine if this is a tap or double tap
            if (timeDiff < 150) // Quick tap threshold
            {
                var distance = CalculateDistance(_lastTouchPoint, touchPoint.Position);
                if (distance < 20) // Movement tolerance for tap
                {
                    // Check for double tap (within 300ms of previous tap)
                    // For simplicity, treating all quick taps as single taps for now
                    HandleTapGesture(touchPoint.Position, false);
                }
            }

            if (_isDragging)
            {
                HandleDragGesture(_dragStartPoint, touchPoint.Position, true);
            }

            _logger.LogDebug("Touch up at ({X}, {Y})", touchPoint.Position.X, touchPoint.Position.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in touch up handler");
        }
    }

    private void OnTouchMove(object? sender, TouchEventArgs e)
    {
        try
        {
            var touchPoint = e.GetTouchPoint(_targetElement);
            var distance = CalculateDistance(_lastTouchPoint, touchPoint.Position);

            // If we've moved beyond the drag threshold, start dragging
            if (!_isDragging && distance > 10)
            {
                _isDragging = true;
                _dragStartPoint = _lastTouchPoint;
                _longPressTimer.Stop(); // Cancel long press if dragging starts
            }

            if (_isDragging)
            {
                HandleDragGesture(_dragStartPoint, touchPoint.Position, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in touch move handler");
        }
    }

    private void OnMouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null) return; // Ignore if this is actually a touch event

        try
        {
            var position = e.GetPosition(_targetElement);
            _lastTouchPoint = position;
            _lastTouchTime = DateTime.Now;

            // Start long press timer for right-click simulation
            if (e.ChangedButton == MouseButton.Left)
            {
                _longPressTimer.Interval = LongPressDuration;
                _longPressTimer.Start();
            }

            _logger.LogDebug("Mouse down at ({X}, {Y})", position.X, position.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mouse down handler");
        }
    }

    private void OnMouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null) return; // Ignore if this is actually a touch event

        try
        {
            _longPressTimer.Stop();

            var position = e.GetPosition(_targetElement);
            var currentTime = DateTime.Now;
            var timeDiff = (currentTime - _lastTouchTime).TotalMilliseconds;

            if (e.ChangedButton == MouseButton.Left && timeDiff < 300)
            {
                var distance = CalculateDistance(_lastTouchPoint, position);
                if (distance < 5)
                {
                    HandleTapGesture(position, false);
                }
            }

            if (_isDragging)
            {
                HandleDragGesture(_dragStartPoint, position, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mouse up handler");
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (e.StylusDevice != null) return; // Ignore if this is actually a touch event

        try
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(_targetElement);
                var distance = CalculateDistance(_lastTouchPoint, position);

                if (!_isDragging && distance > 5)
                {
                    _isDragging = true;
                    _dragStartPoint = _lastTouchPoint;
                    _longPressTimer.Stop();
                }

                if (_isDragging)
                {
                    HandleDragGesture(_dragStartPoint, position, false);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mouse move handler");
        }
    }

    private void OnMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        try
        {
            var position = e.GetPosition(_targetElement);
            var scaleFactor = e.Delta > 0 ? 1.1 : 0.9;
            
            HandleZoomGesture(scaleFactor, position);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mouse wheel handler");
        }
    }

    private void OnLongPressTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            // Invoke on UI thread
            Application.Current?.Dispatcher.Invoke(() =>
            {
                HandleLongPressGesture(_lastTouchPoint);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in long press timer handler");
        }
    }

    #endregion

    #region Helper Methods

    private static double CalculateDistance(Point point1, Point point2)
    {
        var deltaX = point2.X - point1.X;
        var deltaY = point2.Y - point1.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    #endregion

    #region IDisposable

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _longPressTimer?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}