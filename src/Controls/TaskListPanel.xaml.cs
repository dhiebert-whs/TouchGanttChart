using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TouchGanttChart.Models;

namespace TouchGanttChart.Controls;

/// <summary>
/// Interaction logic for TaskListPanel - Touch-optimized task list with filtering and selection
/// </summary>
public partial class TaskListPanel : UserControl
{
    public TaskListPanel()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Dependency property for the selected task
    /// </summary>
    public static readonly DependencyProperty SelectedTaskProperty =
        DependencyProperty.Register(nameof(SelectedTask), typeof(GanttTask), typeof(TaskListPanel),
            new PropertyMetadata(null, OnSelectedTaskChanged));

    /// <summary>
    /// Gets or sets the currently selected task
    /// </summary>
    public GanttTask? SelectedTask
    {
        get => (GanttTask?)GetValue(SelectedTaskProperty);
        set => SetValue(SelectedTaskProperty, value);
    }

    /// <summary>
    /// Event fired when a task is selected
    /// </summary>
    public event EventHandler<TaskSelectedEventArgs>? TaskSelected;

    /// <summary>
    /// Event fired when a task is double-clicked/tapped for editing
    /// </summary>
    public event EventHandler<TaskSelectedEventArgs>? TaskEditRequested;

    /// <summary>
    /// Handles mouse click on task items
    /// </summary>
    private void OnTaskItemClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is GanttTask task)
        {
            HandleTaskSelection(task, e.ClickCount);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles touch events on task items
    /// </summary>
    private void OnTaskItemTouch(object sender, TouchEventArgs e)
    {
        if (sender is Border border && border.Tag is GanttTask task)
        {
            HandleTaskSelection(task, 1);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles task selection and edit requests
    /// </summary>
    /// <param name="task">The selected task</param>
    /// <param name="clickCount">Number of clicks/taps</param>
    private void HandleTaskSelection(GanttTask task, int clickCount)
    {
        // Update selection
        SelectedTask = task;

        // Fire selection event
        TaskSelected?.Invoke(this, new TaskSelectedEventArgs(task));

        // Handle double-click/double-tap for editing
        if (clickCount >= 2)
        {
            TaskEditRequested?.Invoke(this, new TaskSelectedEventArgs(task));
        }
    }

    /// <summary>
    /// Handles changes to the SelectedTask property
    /// </summary>
    private static void OnSelectedTaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaskListPanel panel)
        {
            panel.OnSelectedTaskChanged(e.NewValue as GanttTask);
        }
    }

    /// <summary>
    /// Updates task selection UI when SelectedTask changes
    /// </summary>
    /// <param name="selectedTask">The newly selected task</param>
    private void OnSelectedTaskChanged(GanttTask? selectedTask)
    {
        // Update IsSelected property on all tasks in the data context
        if (DataContext is ViewModels.MainWindowViewModel viewModel)
        {
            foreach (var task in viewModel.Tasks)
            {
                task.IsSelected = task == selectedTask;
            }
        }
    }
}

/// <summary>
/// Event arguments for task selection events
/// </summary>
public class TaskSelectedEventArgs : EventArgs
{
    public GanttTask Task { get; }

    public TaskSelectedEventArgs(GanttTask task)
    {
        Task = task ?? throw new ArgumentNullException(nameof(task));
    }
}