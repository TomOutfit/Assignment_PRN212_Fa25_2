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
            var dashboardWindow = new RealtimeDashboardWindow();
            dashboardWindow.Show();
        }
    }
}
