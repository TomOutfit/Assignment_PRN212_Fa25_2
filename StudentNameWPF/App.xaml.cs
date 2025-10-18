using System.Configuration;
using System.Data;
using System.Windows;
using OfficeOpenXml;

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
            
            // Set EPPlus license for non-commercial use
            // Note: EPPlus 8+ requires license to be set before any ExcelPackage usage
            try
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618 // Type or member is obsolete
                System.Diagnostics.Debug.WriteLine("EPPlus license set to NonCommercial");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set EPPlus license: {ex.Message}");
            }
            
            base.OnStartup(e);
            
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

    protected override void OnExit(ExitEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Application exiting with code: {e.ApplicationExitCode}");
        base.OnExit(e);
    }
}

