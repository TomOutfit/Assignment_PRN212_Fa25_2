using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Windows;
using System.Threading.Tasks;
using StudentNameWPF.Services;
using StudentNameWPF.ViewModels;

namespace StudentNameWPF.ViewModels
{
    public class RealtimeDashboardViewModel : BaseViewModel, IDisposable
    {
        private readonly RealtimeDataService _realtimeDataService;
        private readonly DispatcherTimer _uiUpdateTimer;
        
        private DateTime _lastUpdate;
        private int _totalCustomers;
        private int _totalBookings;
        private int _totalRooms;
        private decimal _totalRevenue;
        private decimal _averageBookingValue;
        private double _occupancyRate;
        private bool _isRealtimeEnabled;
        private string _statusMessage = "Initializing...";
        
        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set => SetProperty(ref _lastUpdate, value);
        }
        
        public int TotalCustomers
        {
            get => _totalCustomers;
            set => SetProperty(ref _totalCustomers, value);
        }
        
        public int TotalBookings
        {
            get => _totalBookings;
            set => SetProperty(ref _totalBookings, value);
        }
        
        public int TotalRooms
        {
            get => _totalRooms;
            set => SetProperty(ref _totalRooms, value);
        }
        
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }
        
        public decimal AverageBookingValue
        {
            get => _averageBookingValue;
            set => SetProperty(ref _averageBookingValue, value);
        }
        
        public double OccupancyRate
        {
            get => _occupancyRate;
            set => SetProperty(ref _occupancyRate, value);
        }
        
        public bool IsRealtimeEnabled
        {
            get => _isRealtimeEnabled;
            set => SetProperty(ref _isRealtimeEnabled, value);
        }
        
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        
        public ObservableCollection<BookingSummary> RecentBookings { get; } = new();
        public ObservableCollection<MonthlyRevenueData> MonthlyRevenue { get; } = new();
        public ObservableCollection<CustomerDistributionData> CustomerDistribution { get; } = new();
        public ObservableCollection<RoomTypePerformanceData> RoomTypePerformance { get; } = new();
        
        public RelayCommand ToggleRealtimeCommand { get; }
        public RelayCommand RefreshDataCommand { get; }
        public RelayCommand ExportDataCommand { get; }
        
        public event EventHandler<StudentNameWPF.Services.RealtimeDataEventArgs>? DataUpdated;
        
        public RealtimeDashboardViewModel()
        {
            _realtimeDataService = new RealtimeDataService();
            _realtimeDataService.DataUpdated += OnDataUpdated;
            
            _uiUpdateTimer = new DispatcherTimer();
            _uiUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            _uiUpdateTimer.Tick += OnUITimerTick;
            _uiUpdateTimer.Start();
            
            ToggleRealtimeCommand = new RelayCommand(ToggleRealtime);
            RefreshDataCommand = new RelayCommand(RefreshData);
            ExportDataCommand = new RelayCommand(ExportData);
            
            // Initial data load
            _ = Task.Run(async () => await LoadInitialDataAsync());
        }
        
        private async Task LoadInitialDataAsync()
        {
            try
            {
                StatusMessage = "Loading initial data...";
                
                // Load initial data immediately
                await _realtimeDataService.RefreshDataAsync();
                
                StatusMessage = "Data loaded successfully";
                IsRealtimeEnabled = true;
                
                // Start realtime updates with shorter interval for better responsiveness
                _realtimeDataService.StartRealtimeUpdates(10); // Update every 10 seconds
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading initial data: {ex.Message}");
                MessageBox.Show($"Failed to load dashboard data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void OnDataUpdated(object? sender, RealtimeDataEventArgs e)
        {
            // Update on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var data = e.Data;
                    
                    LastUpdate = data.Timestamp;
                    TotalCustomers = data.TotalCustomers;
                    TotalBookings = data.TotalBookings;
                    TotalRooms = data.TotalRooms;
                    TotalRevenue = data.TotalRevenue;
                    AverageBookingValue = data.AverageBookingValue;
                    OccupancyRate = data.OccupancyRate;
                    
                    // Update collections with better performance
                    RecentBookings.Clear();
                    foreach (var booking in data.RecentBookings)
                    {
                        RecentBookings.Add(booking);
                    }
                    
                    MonthlyRevenue.Clear();
                    foreach (var revenue in data.MonthlyRevenue)
                    {
                        MonthlyRevenue.Add(revenue);
                    }
                    
                    CustomerDistribution.Clear();
                    foreach (var customer in data.CustomerDistribution)
                    {
                        CustomerDistribution.Add(customer);
                    }
                    
                    RoomTypePerformance.Clear();
                    foreach (var roomType in data.RoomTypePerformance)
                    {
                        RoomTypePerformance.Add(roomType);
                    }
                    
                    StatusMessage = $"✅ Data updated: {data.Timestamp:HH:mm:ss}";
                    System.Diagnostics.Debug.WriteLine($"Dashboard updated with {data.TotalBookings} bookings, {data.TotalCustomers} customers");
                    
                    // Trigger DataUpdated event for charts
                    DataUpdated?.Invoke(this, new StudentNameWPF.Services.RealtimeDataEventArgs(data));
                }
                catch (Exception ex)
                {
                    StatusMessage = $"❌ Update failed: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"Error updating dashboard: {ex.Message}");
                }
            });
        }
        
        private void OnUITimerTick(object? sender, EventArgs e)
        {
            // Update status message with elapsed time
            if (IsRealtimeEnabled && LastUpdate != DateTime.MinValue)
            {
                var elapsed = DateTime.Now - LastUpdate;
                StatusMessage = $"Last updated: {LastUpdate:HH:mm:ss} ({elapsed.TotalSeconds:F0}s ago)";
            }
        }
        
        private void ToggleRealtime()
        {
            if (IsRealtimeEnabled)
            {
                _realtimeDataService.StopRealtimeUpdates();
                IsRealtimeEnabled = false;
                StatusMessage = "Realtime updates stopped";
            }
            else
            {
                _realtimeDataService.StartRealtimeUpdates(30);
                IsRealtimeEnabled = true;
                StatusMessage = "Realtime updates started";
            }
        }
        
        private async void RefreshData()
        {
            try
            {
                StatusMessage = "🔄 Refreshing data...";
                await _realtimeDataService.RefreshDataAsync();
                StatusMessage = "✅ Data refreshed successfully";
                System.Diagnostics.Debug.WriteLine("Manual data refresh completed");
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Refresh failed: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error refreshing data: {ex.Message}");
                MessageBox.Show($"Failed to refresh data: {ex.Message}", "Refresh Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private async void ExportData()
        {
            try
            {
                StatusMessage = "📤 Exporting data...";
                
                // Trigger manual data refresh and export
                await _realtimeDataService.RefreshDataAsync();
                
                StatusMessage = "✅ Data exported successfully";
                MessageBox.Show("📊 Dashboard data has been exported to JSON files in the Reports folder.\n\nFiles include:\n• Realtime data\n• Dashboard charts\n• Customer analysis", 
                    "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Export failed: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error exporting data: {ex.Message}");
                MessageBox.Show($"Failed to export data: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public void Dispose()
        {
            _realtimeDataService?.StopRealtimeUpdates();
            _uiUpdateTimer?.Stop();
            if (_realtimeDataService != null)
            {
                _realtimeDataService.DataUpdated -= OnDataUpdated;
            }
        }
    }
}
