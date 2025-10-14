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
            html.AppendLine("<title>FUMiniHotel System Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #2c3e50; border-bottom: 2px solid #3498db; }");
            html.AppendLine("h2 { color: #34495e; margin-top: 30px; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            html.AppendLine("th { background-color: #3498db; color: white; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            html.AppendLine(".summary { background-color: #ecf0f1; padding: 15px; border-radius: 5px; margin: 20px 0; }");
            html.AppendLine(".chart-placeholder { background-color: #f8f9fa; border: 2px dashed #dee2e6; padding: 40px; text-align: center; margin: 20px 0; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            
            html.AppendLine($"<h1>🏨 FUMiniHotel System Report</h1>");
            html.AppendLine($"<p><strong>Report Type:</strong> {reportType}</p>");
            html.AppendLine($"<p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            
            // Summary Statistics
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>📊 Summary Statistics</h2>");
            html.AppendLine($"<p><strong>Total Bookings:</strong> {bookings.Count}</p>");
            html.AppendLine($"<p><strong>Total Revenue:</strong> ${bookings.Sum(b => b.TotalAmount):N2}</p>");
            html.AppendLine($"<p><strong>Average Booking Value:</strong> ${bookings.Average(b => b.TotalAmount):N2}</p>");
            html.AppendLine($"<p><strong>Total Customers:</strong> {customers.Count}</p>");
            html.AppendLine($"<p><strong>Total Rooms:</strong> {rooms.Count}</p>");
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
                html.AppendLine($"<tr><td>{month.Month}</td><td>${month.Revenue:N2}</td></tr>");
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
                html.AppendLine($"<tr><td>{customer.CustomerName}</td><td>${customer.TotalSpent:N2}</td><td>{customer.BookingCount}</td></tr>");
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
                html.AppendLine($"<tr><td>{room.RoomNumber}</td><td>${room.Revenue:N2}</td><td>{room.BookingCount}</td></tr>");
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
