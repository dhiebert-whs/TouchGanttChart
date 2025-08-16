using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using TouchGanttChart.Data;
using TouchGanttChart.Services.Implementations;
using TouchGanttChart.Services.Interfaces;
using TouchGanttChart.ViewModels;

namespace TouchGanttChart;

/// <summary>
/// Interaction logic for App.xaml with dependency injection configuration.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    /// <summary>
    /// Gets the current application instance.
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("Services not initialized");

    /// <summary>
    /// Application startup event handler.
    /// </summary>
    /// <param name="e">Startup event arguments.</param>
    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/touchgantt-.txt", 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7)
                .CreateLogger();

            Log.Information("Starting Touch Gantt Chart application");

            // Build the host
            _host = CreateHostBuilder(e.Args).Build();

            // Initialize database
            await InitializeDatabaseAsync();

            // Start the host
            await _host.StartAsync();

            // Create and show main window
            var mainWindow = Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();

            Log.Information("Application started successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            MessageBox.Show($"Application failed to start: {ex.Message}", "Startup Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }

        base.OnStartup(e);
    }

    /// <summary>
    /// Application exit event handler.
    /// </summary>
    /// <param name="e">Exit event arguments.</param>
    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            Log.Information("Shutting down application");

            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            Log.Information("Application shutdown complete");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during application shutdown");
        }
        finally
        {
            Log.CloseAndFlush();
        }

        base.OnExit(e);
    }

    /// <summary>
    /// Creates the host builder with dependency injection configuration.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>The configured host builder.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                // Register DbContext
                services.AddDbContext<AppDbContext>();

                // Register services
                services.AddSingleton<IDataService, DataService>();
                services.AddSingleton<IPdfExportService, PdfExportService>();
                services.AddTransient<ITouchGestureService, TouchGestureService>();

                // Register ViewModels
                services.AddSingleton<MainWindowViewModel>();

                // Register Views
                services.AddSingleton<MainWindow>();

                // Add logging
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog();
                });
            });

    /// <summary>
    /// Initializes the database and applies any pending migrations.
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Log.Information("Initializing database");

            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync();

            // Apply SQLite optimizations
            await dbContext.OptimizeDatabaseAsync();

            // Seed data if needed
            await dbContext.SeedDataAsync();

            Log.Information("Database initialization completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database initialization failed");
            throw;
        }
    }

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance.</returns>
    public T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a service of the specified type or null if not found.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance or null.</returns>
    public T? GetOptionalService<T>() where T : class
    {
        return Services.GetService<T>();
    }
}

