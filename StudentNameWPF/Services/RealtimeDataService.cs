using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using FUMiniHotelSystem.Models;
using FUMiniHotelSystem.DataAccess;
using System.Text.Json;

namespace StudentNameWPF.Services
{
    public class RealtimeDataService
    {
        private readonly CustomerRepository _customerRepository;
        private readonly BookingRepository _bookingRepository;
        private readonly RoomRepository _roomRepository;
        private readonly RoomTypeRepository _roomTypeRepository;
        private readonly ChartExportService _chartExportService;
        
        private System.Timers.Timer? _updateTimer;
        private bool _isRunning = false;
        
        public event EventHandler<RealtimeDataEventArgs>? DataUpdated;
        
        public RealtimeDataService()
        {
            var connectionString = GetConnectionString();
            
            _customerRepository = new CustomerRepository(connectionString);
            _bookingRepository = new BookingRepository(connectionString);
            _roomRepository = new RoomRepository(connectionString);
            _roomTypeRepository = new RoomTypeRepository(connectionString);
            _chartExportService = new ChartExportService();
        }
        
        public void StartRealtimeUpdates(int intervalSeconds = 30)
        {
            if (_isRunning) return;
            
            _updateTimer = new System.Timers.Timer(intervalSeconds * 1000);
            _updateTimer.Elapsed += async (sender, e) => await UpdateDataAsync();
            _updateTimer.AutoReset = true;
            _updateTimer.Enabled = true;
            _isRunning = true;
            
            // Initial data load
            _ = Task.Run(async () => await UpdateDataAsync());
        }
        
        public void StopRealtimeUpdates()
        {
            if (_updateTimer != null)
            {
                _updateTimer.Enabled = false;
                _updateTimer.Dispose();
                _updateTimer = null;
            }
            _isRunning = false;
        }
        
        public async Task RefreshDataAsync()
        {
            await UpdateDataAsync();
        }
        
        public async Task ForceUpdateAsync()
        {
            System.Diagnostics.Debug.WriteLine("RealtimeDataService: Force update requested");
            await UpdateDataAsync();
        }
        
        private async Task UpdateDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("RealtimeDataService: Starting data update...");
                
                var customers = await _customerRepository.GetAllAsync();
                var bookings = await _bookingRepository.GetAllAsync();
                var rooms = await _roomRepository.GetAllAsync();
                var roomTypes = await _roomTypeRepository.GetAllAsync();
                
                System.Diagnostics.Debug.WriteLine($"RealtimeDataService: Loaded {customers.Count} customers, {bookings.Count} bookings, {rooms.Count} rooms");
                
                var totalRevenue = bookings.Sum(b => b.TotalAmount);
                var averageBookingValue = bookings.Any() ? bookings.Average(b => b.TotalAmount) : 0;
                
                System.Diagnostics.Debug.WriteLine($"RealtimeDataService: Data Summary:");
                System.Diagnostics.Debug.WriteLine($"  - Customers: {customers.Count}");
                System.Diagnostics.Debug.WriteLine($"  - Bookings: {bookings.Count}");
                System.Diagnostics.Debug.WriteLine($"  - Rooms: {rooms.Count}");
                System.Diagnostics.Debug.WriteLine($"  - Total Revenue: ${totalRevenue:0}");
                System.Diagnostics.Debug.WriteLine($"  - Average Booking Value: ${averageBookingValue:0}");
                
                var realtimeData = new RealtimeData
                {
                    Timestamp = DateTime.Now,
                    TotalCustomers = customers.Count,
                    TotalBookings = bookings.Count,
                    TotalRooms = rooms.Count,
                    TotalRevenue = totalRevenue,
                    AverageBookingValue = averageBookingValue,
                    OccupancyRate = CalculateOccupancyRate(bookings, rooms),
                    RecentBookings = bookings
                        .OrderByDescending(b => b.CheckInDate)
                        .Take(5)
                        .Select(b => new BookingSummary
                        {
                            BookingID = b.BookingID,
                            CustomerName = customers.FirstOrDefault(c => c.CustomerID == b.CustomerID)?.CustomerFullName ?? "Unknown",
                            RoomNumber = rooms.FirstOrDefault(r => r.RoomID == b.RoomID)?.RoomNumber ?? "Unknown",
                            CheckInDate = b.CheckInDate,
                            TotalAmount = b.TotalAmount
                        })
                        .ToList(),
                    MonthlyRevenue = GetMonthlyRevenue(bookings),
                    CustomerDistribution = GetCustomerDistribution(customers, bookings),
                    RoomTypePerformance = GetRoomTypePerformance(rooms, roomTypes, bookings)
                };
                
                // Export realtime data to JSON
                await ExportRealtimeDataAsync(realtimeData);
                
                // Notify subscribers
                DataUpdated?.Invoke(this, new RealtimeDataEventArgs(realtimeData));
                
                System.Diagnostics.Debug.WriteLine($"RealtimeDataService: Data update completed successfully at {realtimeData.Timestamp:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RealtimeDataService: Error updating data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"RealtimeDataService: Stack trace: {ex.StackTrace}");
            }
        }
        
        private double CalculateOccupancyRate(List<Booking> bookings, List<RoomInformation> rooms)
        {
            if (!rooms.Any()) return 0;
            
            var currentDate = DateTime.Now;
            var occupiedRooms = bookings.Count(b => 
                b.CheckInDate <= currentDate && 
                b.CheckOutDate >= currentDate && 
                b.BookingStatus == 1);
            
            var occupancyRate = (double)occupiedRooms / rooms.Count * 100;
            
            System.Diagnostics.Debug.WriteLine($"CalculateOccupancyRate: Current Date: {currentDate:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"CalculateOccupancyRate: Total Rooms: {rooms.Count}");
            System.Diagnostics.Debug.WriteLine($"CalculateOccupancyRate: Total Bookings: {bookings.Count}");
            System.Diagnostics.Debug.WriteLine($"CalculateOccupancyRate: Occupied Rooms: {occupiedRooms}");
            System.Diagnostics.Debug.WriteLine($"CalculateOccupancyRate: Occupancy Rate: {occupancyRate:F1}%");
            
            // Debug: Show active bookings
            var activeBookings = bookings.Where(b => 
                b.CheckInDate <= currentDate && 
                b.CheckOutDate >= currentDate && 
                b.BookingStatus == 1).ToList();
                
            foreach (var booking in activeBookings)
            {
                System.Diagnostics.Debug.WriteLine($"  Active Booking {booking.BookingID}: Room {booking.RoomID}, {booking.CheckInDate:yyyy-MM-dd} to {booking.CheckOutDate:yyyy-MM-dd}");
            }
            
            return occupancyRate;
        }
        
        private List<MonthlyRevenueData> GetMonthlyRevenue(List<Booking> bookings)
        {
            return bookings
                .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyRevenueData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Revenue = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .ToList();
        }
        
        private List<CustomerDistributionData> GetCustomerDistribution(List<Customer> customers, List<Booking> bookings)
        {
            return bookings
                .GroupBy(b => b.CustomerID)
                .Select(g => new CustomerDistributionData
                {
                    CustomerID = g.Key,
                    CustomerName = customers.FirstOrDefault(c => c.CustomerID == g.Key)?.CustomerFullName ?? "Unknown",
                    BookingCount = g.Count(),
                    TotalSpent = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(c => c.BookingCount)
                .Take(10)
                .ToList();
        }
        
        private List<RoomTypePerformanceData> GetRoomTypePerformance(List<RoomInformation> rooms, List<RoomType> roomTypes, List<Booking> bookings)
        {
            return roomTypes.Select(rt => new RoomTypePerformanceData
            {
                RoomTypeID = rt.RoomTypeID,
                RoomTypeName = rt.RoomTypeName,
                RoomCount = rooms.Count(r => r.RoomTypeID == rt.RoomTypeID),
                BookingCount = bookings.Count(b => rooms.Any(r => r.RoomID == b.RoomID && r.RoomTypeID == rt.RoomTypeID)),
                Revenue = bookings.Where(b => rooms.Any(r => r.RoomID == b.RoomID && r.RoomTypeID == rt.RoomTypeID))
                                .Sum(b => b.TotalAmount)
            }).ToList();
        }
        
        private async Task ExportRealtimeDataAsync(RealtimeData data)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("RealtimeDataService: Starting data export...");
                
                // Ensure Reports directory exists
                Directory.CreateDirectory("Reports");
                
                // Export basic realtime data
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                var filePath = $"Reports/RealtimeData_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                await File.WriteAllTextAsync(filePath, json);
                
                System.Diagnostics.Debug.WriteLine($"RealtimeDataService: Basic data exported to {filePath}");
                
                // Export dashboard data with charts
                var dashboardPath = await _chartExportService.ExportRealtimeDashboardAsync(data, "Dashboard");
                System.Diagnostics.Debug.WriteLine($"RealtimeDataService: Dashboard data exported to {dashboardPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RealtimeDataService: Error exporting data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"RealtimeDataService: Export stack trace: {ex.StackTrace}");
            }
        }

        private string GetConnectionString()
        {
            try
            {
                // Read from appsettings.json
                var json = File.ReadAllText("appsettings.json");
                var config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                
                if (config != null && config.ContainsKey("ConnectionString"))
                {
                    return config["ConnectionString"]?.ToString() ?? "Server=(localdb)\\mssqllocaldb;Database=FUMiniHotelManagement;Trusted_Connection=true;TrustServerCertificate=true;";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading connection string: {ex.Message}");
            }
            
            // Default connection string
            return "Server=(localdb)\\mssqllocaldb;Database=FUMiniHotelManagement;Trusted_Connection=true;TrustServerCertificate=true;";
        }
    }
    
    public class RealtimeDataEventArgs : EventArgs
    {
        public RealtimeData Data { get; }
        
        public RealtimeDataEventArgs(RealtimeData data)
        {
            Data = data;
        }
    }
    
    public class RealtimeData
    {
        public DateTime Timestamp { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBookings { get; set; }
        public int TotalRooms { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageBookingValue { get; set; }
        public double OccupancyRate { get; set; }
        public List<BookingSummary> RecentBookings { get; set; } = new();
        public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new();
        public List<CustomerDistributionData> CustomerDistribution { get; set; } = new();
        public List<RoomTypePerformanceData> RoomTypePerformance { get; set; } = new();
    }
    
    public class BookingSummary
    {
        public int BookingID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    public class MonthlyRevenueData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }
    
    public class CustomerDistributionData
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
    
    public class RoomTypePerformanceData
    {
        public int RoomTypeID { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public int RoomCount { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
