using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace StudentNameWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Application starting up...");
            base.OnStartup(e);
            
            // Initialize services
            System.Diagnostics.Debug.WriteLine("Initializing services...");
            Services.ServiceContainer.Initialize();
            System.Diagnostics.Debug.WriteLine("Services initialized successfully");
            
            // Initialize database
            System.Diagnostics.Debug.WriteLine("Initializing database...");
            InitializeDatabase();
            System.Diagnostics.Debug.WriteLine("Database initialized successfully");
            
            // Create and show the LoginWindow
            System.Diagnostics.Debug.WriteLine("Creating LoginWindow...");
            var loginWindow = new Views.LoginWindow();
            System.Diagnostics.Debug.WriteLine("LoginWindow created, showing window...");
            loginWindow.Show();
            System.Diagnostics.Debug.WriteLine("LoginWindow shown successfully");
            
            System.Diagnostics.Debug.WriteLine("Application startup completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Application startup failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            MessageBox.Show($"Application startup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void InitializeDatabase()
    {
        try
        {
            using var context = Services.ServiceContainer.GetService<FUMiniHotelSystem.DataAccess.HotelDbContext>();
            
            // Ensure database is created
            context.Database.EnsureCreated();
            System.Diagnostics.Debug.WriteLine("Database ensured created successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Application exiting with code: {e.ApplicationExitCode}");
        Services.ServiceContainer.Dispose();
        base.OnExit(e);
    }
}

