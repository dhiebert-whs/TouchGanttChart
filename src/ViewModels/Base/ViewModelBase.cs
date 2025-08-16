using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace TouchGanttChart.ViewModels.Base;

/// <summary>
/// Base class for all view models providing common MVVM functionality.
/// Uses CommunityToolkit.Mvvm for property change notification and commands.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the ViewModelBase class.
    /// </summary>
    protected ViewModelBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ViewModelBase class with logging.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected ViewModelBase(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the view model is currently loading.
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Gets or sets the current status message.
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the view model has errors.
    /// </summary>
    [ObservableProperty]
    private bool _hasErrors;

    /// <summary>
    /// Gets or sets the current error message.
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the view model is in touch mode.
    /// </summary>
    [ObservableProperty]
    private bool _isTouchMode = true;

    /// <summary>
    /// Gets or sets a value indicating whether the view model is enabled.
    /// </summary>
    [ObservableProperty]
    private bool _isEnabled = true;

    /// <summary>
    /// Gets or sets the title for the view model.
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Called when the view model is initialized.
    /// Override in derived classes for custom initialization logic.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();
            
            _logger?.LogInformation("Initializing {ViewModelType}", GetType().Name);
            
            await OnInitializeAsync();
            
            _logger?.LogInformation("Successfully initialized {ViewModelType}", GetType().Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error initializing {ViewModelType}", GetType().Name);
            SetError($"Initialization failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Called when the view model is being closed or disposed.
    /// Override in derived classes for cleanup logic.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task CleanupAsync()
    {
        try
        {
            _logger?.LogInformation("Cleaning up {ViewModelType}", GetType().Name);
            
            await OnCleanupAsync();
            
            _logger?.LogInformation("Successfully cleaned up {ViewModelType}", GetType().Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error cleaning up {ViewModelType}", GetType().Name);
        }
    }

    /// <summary>
    /// Refreshes the view model data.
    /// Override in derived classes for custom refresh logic.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task RefreshAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();
            
            _logger?.LogInformation("Refreshing {ViewModelType}", GetType().Name);
            
            await OnRefreshAsync();
            
            _logger?.LogInformation("Successfully refreshed {ViewModelType}", GetType().Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error refreshing {ViewModelType}", GetType().Name);
            SetError($"Refresh failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Override in derived classes for custom initialization logic.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnInitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Override in derived classes for custom cleanup logic.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnCleanupAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Override in derived classes for custom refresh logic.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnRefreshAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets an error message and marks the view model as having errors.
    /// </summary>
    /// <param name="message">The error message.</param>
    protected void SetError(string message)
    {
        ErrorMessage = message;
        StatusMessage = message;
        HasErrors = true;
        
        _logger?.LogWarning("Error set in {ViewModelType}: {ErrorMessage}", GetType().Name, message);
    }

    /// <summary>
    /// Clears any error state and messages.
    /// </summary>
    protected void ClearError()
    {
        ErrorMessage = string.Empty;
        HasErrors = false;
        
        if (StatusMessage == ErrorMessage)
        {
            StatusMessage = string.Empty;
        }
    }

    /// <summary>
    /// Sets a status message for user feedback.
    /// </summary>
    /// <param name="message">The status message.</param>
    protected void SetStatus(string message)
    {
        StatusMessage = message;
        _logger?.LogInformation("Status set in {ViewModelType}: {StatusMessage}", GetType().Name, message);
    }

    /// <summary>
    /// Executes an async operation with proper error handling and loading state management.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="loadingMessage">Optional loading message.</param>
    /// <param name="successMessage">Optional success message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task ExecuteAsync(Func<Task> operation, string? loadingMessage = null, string? successMessage = null)
    {
        try
        {
            IsLoading = true;
            ClearError();
            
            if (!string.IsNullOrEmpty(loadingMessage))
            {
                SetStatus(loadingMessage);
            }
            
            await operation();
            
            if (!string.IsNullOrEmpty(successMessage))
            {
                SetStatus(successMessage);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing operation in {ViewModelType}", GetType().Name);
            SetError($"Operation failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Executes an async operation with a return value and proper error handling.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="loadingMessage">Optional loading message.</param>
    /// <param name="successMessage">Optional success message.</param>
    /// <returns>A task representing the asynchronous operation with result.</returns>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? loadingMessage = null, string? successMessage = null)
    {
        try
        {
            IsLoading = true;
            ClearError();
            
            if (!string.IsNullOrEmpty(loadingMessage))
            {
                SetStatus(loadingMessage);
            }
            
            var result = await operation();
            
            if (!string.IsNullOrEmpty(successMessage))
            {
                SetStatus(successMessage);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing operation in {ViewModelType}", GetType().Name);
            SetError($"Operation failed: {ex.Message}");
            return default;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Validates the view model state.
    /// Override in derived classes for custom validation logic.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public virtual bool Validate()
    {
        ClearError();
        return true;
    }

    /// <summary>
    /// Gets a value indicating whether the view model can be saved.
    /// Override in derived classes for custom save validation.
    /// </summary>
    public virtual bool CanSave => !HasErrors && !IsLoading && IsEnabled;

    /// <summary>
    /// Gets a value indicating whether the view model can be refreshed.
    /// Override in derived classes for custom refresh validation.
    /// </summary>
    public virtual bool CanRefresh => !IsLoading && IsEnabled;

    /// <summary>
    /// Gets a value indicating whether the view model is busy.
    /// </summary>
    public bool IsBusy => IsLoading;
}