using FUMiniHotelSystem.Models;
using StudentNameWPF.ViewModels;
using System.Windows;

namespace StudentNameWPF.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoginWindow constructor called");
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("LoginWindow InitializeComponent completed");
                
                var viewModel = new LoginViewModel();
                System.Diagnostics.Debug.WriteLine("LoginViewModel created successfully");
                viewModel.LoginSuccessful += OnLoginSuccessful;
                DataContext = viewModel;
                System.Diagnostics.Debug.WriteLine("LoginWindow setup completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoginWindow constructor failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"LoginWindow initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void OnLoginSuccessful(object? sender, Customer customer)
        {
            var mainWindow = new MainWindow(customer);
            mainWindow.Show();
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"PasswordBox_PasswordChanged called - Password length: {PasswordBox.Password?.Length ?? 0}");
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
                System.Diagnostics.Debug.WriteLine($"Password set in ViewModel - Length: {viewModel.Password?.Length ?? 0}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
