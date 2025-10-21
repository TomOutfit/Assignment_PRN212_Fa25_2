using FUMiniHotelSystem.BusinessLogic;
using FUMiniHotelSystem.DataAccess;
using FUMiniHotelSystem.Models;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace StudentNameWPF.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand ClearCommand { get; }

        public event EventHandler<Customer>? LoginSuccessful;

        public LoginViewModel()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoginViewModel constructor called");
                
                // Initialize services
                System.Diagnostics.Debug.WriteLine("Initializing configuration...");
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                System.Diagnostics.Debug.WriteLine("Configuration loaded successfully");

                var connectionString = configuration["ConnectionString"] ?? "Server=(localdb)\\mssqllocaldb;Database=FUMiniHotelManagement;Trusted_Connection=true;TrustServerCertificate=true;";
                
                var appSettings = new AppSettings
                {
                    AdminEmail = configuration["AdminEmail"] ?? "admin@FUMiniHotelSystem.com",
                    AdminPassword = configuration["AdminPassword"] ?? "@@abc123@@",
                    ConnectionString = connectionString
                };
                System.Diagnostics.Debug.WriteLine($"AppSettings created - AdminEmail: {appSettings.AdminEmail}");

                System.Diagnostics.Debug.WriteLine("Creating CustomerRepository...");
                var customerRepository = new CustomerRepository(connectionString);
                System.Diagnostics.Debug.WriteLine("CustomerRepository created successfully");
                
                System.Diagnostics.Debug.WriteLine("Creating AuthenticationService...");
                _authService = new AuthenticationService(customerRepository, appSettings);
                System.Diagnostics.Debug.WriteLine("AuthenticationService created successfully");

                LoginCommand = new RelayCommand(async () => await LoginAsync(), () => !IsLoading);
                ClearCommand = new RelayCommand(ClearFields);
                System.Diagnostics.Debug.WriteLine("LoginViewModel constructor completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoginViewModel constructor failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task LoginAsync()
        {
            // Clear previous error message
            ErrorMessage = string.Empty;

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Login attempt - Email: '{Email}', Password length: {Password?.Length ?? 0}");

            // Validate input
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Please enter both email and password.";
                }
                else if (string.IsNullOrWhiteSpace(Email))
                {
                    ErrorMessage = "Please enter your email address.";
                }
                else if (string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Please enter your password.";
                }
                System.Diagnostics.Debug.WriteLine($"Validation failed: {ErrorMessage}");
                return;
            }

            // Validate email format
            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address.";
                return;
            }

            IsLoading = true;

            try
            {
                var customer = await _authService.AuthenticateAsync(Email, Password);
                if (customer != null)
                {
                    // Welcome message
                    MessageBox.Show($"Welcome to our Hotel System !\n\nUser: {customer.CustomerFullName}\nEmail: {customer.EmailAddress}\nIsAdmin: {customer.IsAdmin}", 
                        "Welcome", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoginSuccessful?.Invoke(this, customer);
                }
                else
                {
                    ErrorMessage = "Invalid email or password. Please check your credentials and try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public void ClearFields()
        {
            Email = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}
