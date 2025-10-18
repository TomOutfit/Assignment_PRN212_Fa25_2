using FUMiniHotelSystem.Models;
using StudentNameWPF.ViewModels;
using System.Windows;

namespace StudentNameWPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(Customer currentUser)
        {
            InitializeComponent();
            
            var viewModel = new MainViewModel(currentUser);
            viewModel.LogoutCommand = new RelayCommand(Logout);
            DataContext = viewModel;
        }

        private void Logout()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void OpenRealtimeDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Get the current user from the DataContext
            var viewModel = DataContext as MainViewModel;
            if (viewModel?.CurrentUser == null)
            {
                MessageBox.Show("Không thể xác định thông tin người dùng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if user is admin
            if (!viewModel.CurrentUser.IsAdmin)
            {
                MessageBox.Show("Bạn không có quyền để làm điều này!\n\nChỉ Admin mới có thể truy cập Realtime Dashboard.", 
                    "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // User is admin, open the dashboard
            var dashboardWindow = new RealtimeDashboardWindow();
            dashboardWindow.Show();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaximizeButton.Content = "🗖"; // Maximize icon
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaximizeButton.Content = "🗗"; // Restore icon
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
