using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FUMiniHotelSystem.Models;

namespace StudentNameWPF.Services
{
    public class ReportExportService
    {
        public async Task<string> ExportToCSVAsync<T>(List<T> data, string fileName, string[]? headers = null)
        {
            var csv = new StringBuilder();
            
            // Add headers if provided
            if (headers != null)
            {
                csv.AppendLine(string.Join(",", headers));
            }
            
            // Add data rows
            foreach (var item in data)
            {
                var properties = typeof(T).GetProperties();
                var values = properties.Select(p => 
                {
                    var value = p.GetValue(item);
                    if (value is DateTime dateTime)
                        return dateTime.ToString("yyyy-MM-dd");
                    return value?.ToString() ?? string.Empty;
                });
                csv.AppendLine(string.Join(",", values));
            }
            
            var filePath = Path.Combine("Reports", $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, csv.ToString());
            
            return filePath;
        }
        
        public async Task<string> ExportToHTMLAsync(List<Booking> bookings, List<Customer> customers, List<RoomInformation> rooms, string reportType, string? customFilePath = null)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<title>FUMiniHotel System - Comprehensive Business Report</title>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; line-height: 1.6; }");
            html.AppendLine(".container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }");
            html.AppendLine("h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 15px; margin-bottom: 30px; text-align: center; }");
            html.AppendLine("h2 { color: #34495e; margin-top: 40px; margin-bottom: 20px; border-left: 4px solid #3498db; padding-left: 15px; }");
            html.AppendLine("h3 { color: #2c3e50; margin-top: 25px; margin-bottom: 15px; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; font-size: 14px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            html.AppendLine("th { background: linear-gradient(135deg, #3498db, #2980b9); color: white; font-weight: bold; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f8f9fa; }");
            html.AppendLine("tr:hover { background-color: #e8f4f8; }");
            html.AppendLine(".summary { background: linear-gradient(135deg, #ecf0f1, #bdc3c7); padding: 25px; border-radius: 10px; margin: 25px 0; border-left: 5px solid #3498db; }");
            html.AppendLine(".metric-card { background: white; padding: 20px; margin: 10px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); display: inline-block; min-width: 200px; text-align: center; }");
            html.AppendLine(".metric-value { font-size: 24px; font-weight: bold; color: #2c3e50; }");
            html.AppendLine(".metric-label { color: #7f8c8d; font-size: 14px; margin-top: 5px; }");
            html.AppendLine(".chart-placeholder { background: linear-gradient(135deg, #f8f9fa, #e9ecef); border: 2px dashed #dee2e6; padding: 40px; text-align: center; margin: 20px 0; border-radius: 8px; }");
            html.AppendLine(".insight { background: #e8f5e8; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #27ae60; }");
            html.AppendLine(".warning { background: #fef9e7; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #f39c12; }");
            html.AppendLine(".footer { text-align: center; margin-top: 40px; padding-top: 20px; border-top: 2px solid #ecf0f1; color: #7f8c8d; }");
            html.AppendLine("@media print { body { background: white; } .container { box-shadow: none; } }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            
            html.AppendLine("<div class='container'>");
            html.AppendLine($"<h1>🏨 FUMiniHotel Management System</h1>");
            html.AppendLine($"<h2>📊 Comprehensive Business Intelligence Report</h2>");
            html.AppendLine($"<p><strong>Report Type:</strong> {reportType}</p>");
            html.AppendLine($"<p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine($"<p><strong>Report Period:</strong> {bookings.Min(b => b.CheckInDate):yyyy-MM-dd} to {bookings.Max(b => b.CheckInDate):yyyy-MM-dd}</p>");
            
            // Executive Summary with enhanced metrics
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>📈 Executive Summary</h2>");
            
            var totalRevenue = bookings.Sum(b => b.TotalAmount);
            var avgBookingValue = bookings.Average(b => b.TotalAmount);
            var occupancyRate = CalculateOccupancyRate(bookings, rooms);
            var topCustomer = bookings.GroupBy(b => b.CustomerID)
                .Select(g => new { CustomerID = g.Key, Revenue = g.Sum(b => b.TotalAmount) })
                .OrderByDescending(c => c.Revenue).FirstOrDefault();
            
            html.AppendLine("<div style='display: flex; flex-wrap: wrap; justify-content: space-around;'>");
            html.AppendLine($"<div class='metric-card'><div class='metric-value'>${totalRevenue:0}</div><div class='metric-label'>Total Revenue</div></div>");
            html.AppendLine($"<div class='metric-card'><div class='metric-value'>{bookings.Count:N0}</div><div class='metric-label'>Total Bookings</div></div>");
            html.AppendLine($"<div class='metric-card'><div class='metric-value'>${avgBookingValue:0}</div><div class='metric-label'>Avg Booking Value</div></div>");
            html.AppendLine($"<div class='metric-card'><div class='metric-value'>{customers.Count:N0}</div><div class='metric-label'>Total Customers</div></div>");
            html.AppendLine($"<div class='metric-card'><div class='metric-value'>{rooms.Count:N0}</div><div class='metric-label'>Total Rooms</div></div>");
            html.AppendLine($"<div class='metric-card'><div class='metric-value'>{occupancyRate:F1}%</div><div class='metric-label'>Occupancy Rate</div></div>");
            html.AppendLine("</div>");
            
            // Business Insights
            html.AppendLine("<div class='insight'>");
            html.AppendLine("<h3>💡 Key Business Insights</h3>");
            html.AppendLine($"<p><strong>Revenue Performance:</strong> Total revenue of ${totalRevenue:0} with an average booking value of ${avgBookingValue:0}</p>");
            html.AppendLine($"<p><strong>Customer Engagement:</strong> {customers.Count} active customers with {bookings.Count} total bookings</p>");
            html.AppendLine($"<p><strong>Operational Efficiency:</strong> {occupancyRate:F1}% occupancy rate across {rooms.Count} available rooms</p>");
            if (topCustomer != null)
            {
                var topCustomerName = customers.FirstOrDefault(c => c.CustomerID == topCustomer.CustomerID)?.CustomerFullName ?? "Unknown";
                html.AppendLine($"<p><strong>Top Customer:</strong> {topCustomerName} with ${topCustomer.Revenue:0} in total revenue</p>");
            }
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            
            // Revenue by Month
            var monthlyRevenue = bookings
                .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:D2}", Revenue = g.Sum(b => b.TotalAmount) });
            
            html.AppendLine("<h2>📈 Monthly Revenue</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Month</th><th>Revenue</th></tr>");
            foreach (var month in monthlyRevenue)
            {
                html.AppendLine($"<tr><td>{month.Month}</td><td>${month.Revenue:0}</td></tr>");
            }
            html.AppendLine("</table>");
            
            // Top Customers
            var topCustomers = bookings
                .GroupBy(b => b.CustomerID)
                .Select(g => new { 
                    CustomerID = g.Key, 
                    CustomerName = customers.FirstOrDefault(c => c.CustomerID == g.Key)?.CustomerFullName ?? "Unknown",
                    TotalSpent = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10);
            
            html.AppendLine("<h2>👑 Top Customers</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Customer</th><th>Total Spent</th><th>Bookings</th></tr>");
            foreach (var customer in topCustomers)
            {
                html.AppendLine($"<tr><td>{customer.CustomerName}</td><td>${customer.TotalSpent:0}</td><td>{customer.BookingCount}</td></tr>");
            }
            html.AppendLine("</table>");
            
            // Room Performance
            var roomPerformance = bookings
                .GroupBy(b => b.RoomID)
                .Select(g => new {
                    RoomID = g.Key,
                    RoomNumber = rooms.FirstOrDefault(r => r.RoomID == g.Key)?.RoomNumber ?? "Unknown",
                    Revenue = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderByDescending(r => r.Revenue)
                .Take(10);
            
            html.AppendLine("<h2>🏆 Top Performing Rooms</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Room</th><th>Revenue</th><th>Bookings</th></tr>");
            foreach (var room in roomPerformance)
            {
                html.AppendLine($"<tr><td>{room.RoomNumber}</td><td>${room.Revenue:0}</td><td>{room.BookingCount}</td></tr>");
            }
            html.AppendLine("</table>");
            
            // Chart Placeholder
            html.AppendLine("<h2>📊 Visual Analytics</h2>");
            html.AppendLine("<div class='chart-placeholder'>");
            html.AppendLine("<p>📈 Revenue Trend Chart</p>");
            html.AppendLine("<p>📊 Customer Distribution</p>");
            html.AppendLine("<p>🏨 Room Occupancy Rate</p>");
            html.AppendLine("<p><em>Charts can be generated using the application's chart export feature</em></p>");
            html.AppendLine("</div>");
            
            html.AppendLine("</body></html>");
            
            var filePath = customFilePath ?? Path.Combine("Reports", $"HotelReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllTextAsync(filePath, html.ToString());
            
            return filePath;
        }
        
        private double CalculateOccupancyRate(List<Booking> bookings, List<RoomInformation> rooms)
        {
            if (!rooms.Any()) return 0;
            
            var currentDate = DateTime.Now;
            var occupiedRooms = bookings.Count(b => 
                b.CheckInDate <= currentDate && 
                b.CheckOutDate >= currentDate && 
                b.BookingStatus == 1);
            
            return (double)occupiedRooms / rooms.Count * 100;
        }
        
        public async Task<string> ExportToJSONAsync<T>(List<T> data, string fileName)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            var filePath = Path.Combine("Reports", $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, json);
            
            return filePath;
        }
        
        public async Task<string> ExportChartToImageAsync(string chartData, string chartType, string fileName)
        {
            // This would integrate with a chart library to export charts as images
            // For now, we'll create a placeholder HTML file with chart data
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<title>Chart Export</title>");
            html.AppendLine("<script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
            html.AppendLine("</head><body>");
            html.AppendLine($"<h1>{chartType} Chart</h1>");
            html.AppendLine($"<canvas id='chart' width='800' height='400'></canvas>");
            html.AppendLine("<script>");
            html.AppendLine($"const ctx = document.getElementById('chart').getContext('2d');");
            html.AppendLine($"const chartData = {chartData};");
            html.AppendLine("new Chart(ctx, chartData);");
            html.AppendLine("</script>");
            html.AppendLine("</body></html>");
            
            var filePath = Path.Combine("Reports", $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, html.ToString());
            
            return filePath;
        }
    }
}
