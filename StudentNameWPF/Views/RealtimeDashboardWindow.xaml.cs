using StudentNameWPF.ViewModels;
using System.Windows;

namespace StudentNameWPF.Views
{
    /// <summary>
    /// Interaction logic for RealtimeDashboardWindow.xaml
    /// </summary>
    public partial class RealtimeDashboardWindow : Window
    {
        private RealtimeDashboardViewModel _viewModel;
        
        public RealtimeDashboardWindow()
        {
            InitializeComponent();
            
            _viewModel = new RealtimeDashboardViewModel();
            DataContext = _viewModel;
            
            // Subscribe to data updates to update charts
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            
            // Subscribe to realtime data service events
            _viewModel.DataUpdated += OnRealtimeDataUpdated;
            
            // Force initial chart update after a short delay
            Dispatcher.BeginInvoke(new Action(() => {
                UpdateChartsWithCurrentData();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        
        private void OnRealtimeDataUpdated(object? sender, StudentNameWPF.Services.RealtimeDataEventArgs e)
        {
            // Update charts immediately when realtime data changes
            UpdateChartsWithData(e.Data);
        }
        
        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RealtimeDashboardViewModel.MonthlyRevenue))
            {
                UpdateMonthlyRevenueChart();
            }
            else if (e.PropertyName == nameof(RealtimeDashboardViewModel.CustomerDistribution))
            {
                UpdateCustomerDistributionChart();
            }
        }
        
        private void UpdateChartsWithData(StudentNameWPF.Services.RealtimeData data)
        {
            // Update Monthly Revenue Chart
            var monthlyData = data.MonthlyRevenue.Select(m => new StudentNameWPF.Views.Charts.MonthlyRevenueData 
            { 
                MonthName = m.MonthName, 
                Revenue = m.Revenue 
            }).ToList();
            MonthlyRevenueChart.UpdateData(monthlyData);
            
            // Update Customer Distribution Chart
            var customerData = data.CustomerDistribution.Select(c => new StudentNameWPF.Views.Charts.CustomerDistributionData 
            { 
                CustomerID = c.CustomerID,
                CustomerName = c.CustomerName,
                BookingCount = c.BookingCount,
                TotalSpent = c.TotalSpent
            }).ToList();
            CustomerDistributionChart.UpdateData(customerData);
        }
        
        private void UpdateMonthlyRevenueChart()
        {
            var data = _viewModel.MonthlyRevenue.Select(m => new StudentNameWPF.Views.Charts.MonthlyRevenueData 
            { 
                MonthName = m.MonthName, 
                Revenue = m.Revenue 
            }).ToList();
            MonthlyRevenueChart.UpdateData(data);
        }
        
        private void UpdateCustomerDistributionChart()
        {
            var data = _viewModel.CustomerDistribution.Select(c => new StudentNameWPF.Views.Charts.CustomerDistributionData 
            { 
                CustomerID = c.CustomerID,
                CustomerName = c.CustomerName,
                BookingCount = c.BookingCount,
                TotalSpent = c.TotalSpent
            }).ToList();
            CustomerDistributionChart.UpdateData(data);
        }
        
        private void UpdateChartsWithCurrentData()
        {
            System.Diagnostics.Debug.WriteLine("RealtimeDashboardWindow: Updating charts with current data");
            
            // Update Monthly Revenue Chart
            UpdateMonthlyRevenueChart();
            
            // Update Customer Distribution Chart  
            UpdateCustomerDistributionChart();
            
            // Force chart redraw after a short delay to ensure canvas is ready
            Dispatcher.BeginInvoke(new Action(() => {
                System.Diagnostics.Debug.WriteLine("RealtimeDashboardWindow: Forcing chart redraw");
                MonthlyRevenueChart.ForceRedraw();
                CustomerDistributionChart.ForceRedraw();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _viewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}
