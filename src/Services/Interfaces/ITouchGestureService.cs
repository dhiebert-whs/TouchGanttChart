using System.Windows;
using System.Windows.Input;

namespace TouchGanttChart.Services.Interfaces;

/// <summary>
/// Interface for handling touch gesture operations in the Gantt chart.
/// </summary>
public interface ITouchGestureService
{
    /// <summary>
    /// Initializes touch gesture handling for the specified element.
    /// </summary>
    /// <param name="element">The UI element to enable touch gestures on.</param>
    void InitializeTouchGestures(FrameworkElement element);

    /// <summary>
    /// Handles pan gesture for timeline navigation.
    /// </summary>
    /// <param name="deltaX">The horizontal pan delta.</param>
    /// <param name="deltaY">The vertical pan delta.</param>
    void HandlePanGesture(double deltaX, double deltaY);

    /// <summary>
    /// Handles zoom gesture for timeline scaling.
    /// </summary>
    /// <param name="scaleFactor">The zoom scale factor.</param>
    /// <param name="centerPoint">The center point of the zoom gesture.</param>
    void HandleZoomGesture(double scaleFactor, Point centerPoint);

    /// <summary>
    /// Handles tap gesture for task selection.
    /// </summary>
    /// <param name="tapPoint">The point where the tap occurred.</param>
    /// <param name="isDoubleTap">Whether this is a double tap gesture.</param>
    void HandleTapGesture(Point tapPoint, bool isDoubleTap = false);

    /// <summary>
    /// Handles long press gesture for context actions.
    /// </summary>
    /// <param name="pressPoint">The point where the long press occurred.</param>
    void HandleLongPressGesture(Point pressPoint);

    /// <summary>
    /// Handles drag gesture for task manipulation.
    /// </summary>
    /// <param name="startPoint">The starting point of the drag.</param>
    /// <param name="currentPoint">The current point during drag.</param>
    /// <param name="isComplete">Whether the drag gesture is complete.</param>
    void HandleDragGesture(Point startPoint, Point currentPoint, bool isComplete = false);

    /// <summary>
    /// Gets or sets the minimum zoom level.
    /// </summary>
    double MinZoom { get; set; }

    /// <summary>
    /// Gets or sets the maximum zoom level.
    /// </summary>
    double MaxZoom { get; set; }

    /// <summary>
    /// Gets or sets the current zoom level.
    /// </summary>
    double CurrentZoom { get; set; }

    /// <summary>
    /// Gets or sets the pan sensitivity factor.
    /// </summary>
    double PanSensitivity { get; set; }

    /// <summary>
    /// Gets or sets the zoom sensitivity factor.
    /// </summary>
    double ZoomSensitivity { get; set; }

    /// <summary>
    /// Gets or sets the long press duration threshold in milliseconds.
    /// </summary>
    int LongPressDuration { get; set; }

    /// <summary>
    /// Gets or sets whether momentum scrolling is enabled.
    /// </summary>
    bool IsMomentumScrollingEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether multi-touch gestures are enabled.
    /// </summary>
    bool IsMultiTouchEnabled { get; set; }

    /// <summary>
    /// Event raised when a gesture is detected.
    /// </summary>
    event EventHandler<TouchGestureEventArgs>? GestureDetected;

    /// <summary>
    /// Event raised when the zoom level changes.
    /// </summary>
    event EventHandler<ZoomChangedEventArgs>? ZoomChanged;

    /// <summary>
    /// Event raised when a pan operation occurs.
    /// </summary>
    event EventHandler<PanEventArgs>? Panned;

    /// <summary>
    /// Event raised when a task is selected via touch.
    /// </summary>
    event EventHandler<TaskSelectedEventArgs>? TaskSelected;
}

/// <summary>
/// Event arguments for touch gesture events.
/// </summary>
public class TouchGestureEventArgs : EventArgs
{
    public TouchGestureType GestureType { get; set; }
    public Point Position { get; set; }
    public double Delta { get; set; }
    public bool Handled { get; set; }
}

/// <summary>
/// Event arguments for zoom changed events.
/// </summary>
public class ZoomChangedEventArgs : EventArgs
{
    public double OldZoom { get; set; }
    public double NewZoom { get; set; }
    public Point CenterPoint { get; set; }
}

/// <summary>
/// Event arguments for pan events.
/// </summary>
public class PanEventArgs : EventArgs
{
    public double DeltaX { get; set; }
    public double DeltaY { get; set; }
    public bool IsComplete { get; set; }
}

/// <summary>
/// Event arguments for task selection events.
/// </summary>
public class TaskSelectedEventArgs : EventArgs
{
    public Point SelectionPoint { get; set; }
    public bool IsDoubleClick { get; set; }
    public object? SelectedTask { get; set; }
}

/// <summary>
/// Types of touch gestures supported.
/// </summary>
public enum TouchGestureType
{
    Tap,
    DoubleTap,
    LongPress,
    Pan,
    Zoom,
    Drag,
    Pinch,
    Rotate
}