using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FUMiniHotelSystem.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace StudentNameWPF.Services
{
    public class ChartExportService
    {
        public async Task<string> ExportRevenueChartAsync(List<Booking> bookings, string fileName)
        {
            var monthlyRevenue = bookings
                .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { 
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                    Revenue = g.Sum(b => b.TotalAmount),
                    Bookings = g.Count()
                })
                .ToList();

            // Create interactive HTML chart with Chart.js
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<title>Revenue Trend Chart - FUMiniHotel System</title>");
            html.AppendLine("<script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background-color: #f8f9fa; }");
            html.AppendLine(".chart-container { background: white; padding: 30px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); margin: 20px 0; }");
            html.AppendLine("h1 { color: #2c3e50; text-align: center; margin-bottom: 30px; }");
            html.AppendLine(".chart-wrapper { position: relative; height: 400px; margin: 20px 0; }");
            html.AppendLine(".stats { display: flex; justify-content: space-around; margin: 20px 0; }");
            html.AppendLine(".stat-card { background: linear-gradient(135deg, #3498db, #2980b9); color: white; padding: 20px; border-radius: 8px; text-align: center; min-width: 150px; }");
            html.AppendLine(".stat-value { font-size: 24px; font-weight: bold; }");
            html.AppendLine(".stat-label { font-size: 14px; margin-top: 5px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            
            html.AppendLine("<div class='chart-container'>");
            html.AppendLine("<h1>📈 Monthly Revenue Trend Analysis</h1>");
            
            // Statistics cards
            var totalRevenue = monthlyRevenue.Sum(m => m.Revenue);
            var avgMonthlyRevenue = monthlyRevenue.Average(m => m.Revenue);
            var peakMonth = monthlyRevenue.OrderByDescending(m => m.Revenue).FirstOrDefault();
            var totalBookings = monthlyRevenue.Sum(m => m.Bookings);
            
            html.AppendLine("<div class='stats'>");
            html.AppendLine($"<div class='stat-card'><div class='stat-value'>${totalRevenue:0}</div><div class='stat-label'>Total Revenue</div></div>");
            html.AppendLine($"<div class='stat-card'><div class='stat-value'>${avgMonthlyRevenue:0}</div><div class='stat-label'>Avg Monthly</div></div>");
            html.AppendLine($"<div class='stat-card'><div class='stat-value'>{peakMonth?.Month ?? "N/A"}</div><div class='stat-label'>Peak Month</div></div>");
            html.AppendLine($"<div class='stat-card'><div class='stat-value'>{totalBookings:N0}</div><div class='stat-label'>Total Bookings</div></div>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='chart-wrapper'>");
            html.AppendLine("<canvas id='revenueChart'></canvas>");
            html.AppendLine("</div>");
            
            html.AppendLine("<script>");
            html.AppendLine("const ctx = document.getElementById('revenueChart').getContext('2d');");
            html.AppendLine("const revenueChart = new Chart(ctx, {");
            html.AppendLine("    type: 'line',");
            html.AppendLine("    data: {");
            html.AppendLine($"        labels: {System.Text.Json.JsonSerializer.Serialize(monthlyRevenue.Select(m => m.Month).ToArray())},");
            html.AppendLine("        datasets: [{");
            html.AppendLine("            label: 'Monthly Revenue',");
            html.AppendLine($"            data: {System.Text.Json.JsonSerializer.Serialize(monthlyRevenue.Select(m => m.Revenue).ToArray())},");
            html.AppendLine("            borderColor: 'rgb(75, 192, 192)',");
            html.AppendLine("            backgroundColor: 'rgba(75, 192, 192, 0.2)',");
            html.AppendLine("            tension: 0.4,");
            html.AppendLine("            fill: true,");
            html.AppendLine("            pointBackgroundColor: 'rgb(75, 192, 192)',");
            html.AppendLine("            pointBorderColor: '#fff',");
            html.AppendLine("            pointBorderWidth: 2,");
            html.AppendLine("            pointRadius: 6");
            html.AppendLine("        }]");
            html.AppendLine("    },");
            html.AppendLine("    options: {");
            html.AppendLine("        responsive: true,");
            html.AppendLine("        maintainAspectRatio: false,");
            html.AppendLine("        plugins: {");
            html.AppendLine("            title: {");
            html.AppendLine("                display: true,");
            html.AppendLine("                text: 'Monthly Revenue Trend - FUMiniHotel System',");
            html.AppendLine("                font: { size: 16, weight: 'bold' }");
            html.AppendLine("            },");
            html.AppendLine("            legend: {");
            html.AppendLine("                display: true,");
            html.AppendLine("                position: 'top'");
            html.AppendLine("            }");
            html.AppendLine("        },");
            html.AppendLine("        scales: {");
            html.AppendLine("            y: {");
            html.AppendLine("                beginAtZero: true,");
            html.AppendLine("                ticks: {");
            html.AppendLine("                    callback: function(value) {");
            html.AppendLine("                        return '$' + value.toLocaleString();");
            html.AppendLine("                    }");
            html.AppendLine("                }");
            html.AppendLine("            }");
            html.AppendLine("        }");
            html.AppendLine("    }");
            html.AppendLine("});");
            html.AppendLine("</script>");
            
            html.AppendLine("</div>");
            html.AppendLine("</body></html>");

            var filePath = Path.Combine("Reports", $"{fileName}_RevenueChart_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, html.ToString());

            return filePath;
        }

        public async Task<string> ExportCustomerDistributionChartAsync(List<Customer> customers, List<Booking> bookings, string fileName)
        {
            var customerBookings = bookings
                .GroupBy(b => b.CustomerID)
                .Select(g => new
                {
                    CustomerID = g.Key,
                    CustomerName = customers.FirstOrDefault(c => c.CustomerID == g.Key)?.CustomerFullName ?? "Unknown",
                    BookingCount = g.Count(),
                    TotalSpent = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(c => c.BookingCount)
                .Take(10)
                .ToList();

            // Create interactive HTML chart with Chart.js
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<title>Customer Distribution Chart - FUMiniHotel System</title>");
            html.AppendLine("<script src='https://cdn.jsdelivr.net/npm/chart.js'></script>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background-color: #f8f9fa; }");
            html.AppendLine(".chart-container { background: white; padding: 30px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); margin: 20px 0; }");
            html.AppendLine("h1 { color: #2c3e50; text-align: center; margin-bottom: 30px; }");
            html.AppendLine(".chart-wrapper { position: relative; height: 400px; margin: 20px 0; }");
            html.AppendLine(".stats { display: flex; justify-content: space-around; margin: 20px 0; }");
            html.AppendLine(".stat-card { background: linear-gradient(135deg, #e74c3c, #c0392b); color: white; padding: 20px; border-radius: 8px; text-align: center; min-width: 150px; }");
            html.AppendLine(".stat-value { font-size: 24px; font-weight: bold; }");
            html.AppendLine(".stat-label { font-size: 14px; margin-top: 5px; }");
            html.AppendLine(".customer-list { margin-top: 20px; }");
            html.AppendLine(".customer-item { display: flex; justify-content: space-between; padding: 10px; margin: 5px 0; background: #f8f9fa; border-radius: 5px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            
            html.AppendLine("<div class='chart-container'>");
            html.AppendLine("<h1>👥 Customer Distribution Analysis</h1>");
            
            // Statistics cards
            var totalCustomers = customers.Count;
            var totalBookings = customerBookings.Sum(c => c.BookingCount);
            var topCustomer = customerBookings.FirstOrDefault();
            var avgBookingsPerCustomer = totalBookings / (double)totalCustomers;
            
            html.AppendLine("<div class='stats'>");
            html.AppendLine($"<div class='stat-card'><div class='stat-value'>{totalCustomers:N0}</div><div class='stat-label'>Total Customers</div></div>");
            html.AppendLine($"<div class='stat-card'><div class='stat-value'>{totalBookings:N0}</div><div class='stat-label'>Total Bookings</div></div>");
            html.AppendLine($"<div class='stat-card'><div class='stat-value'>{avgBookingsPerCustomer:F1}</div><div class='stat-label'>Avg per Customer</div></div>");
            html.AppendLine($"<div class='stat-card'><div class='stat-value'>{topCustomer?.CustomerName ?? "N/A"}</div><div class='stat-label'>Top Customer</div></div>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='chart-wrapper'>");
            html.AppendLine("<canvas id='customerChart'></canvas>");
            html.AppendLine("</div>");
            
            html.AppendLine("<script>");
            html.AppendLine("const ctx = document.getElementById('customerChart').getContext('2d');");
            html.AppendLine("const customerChart = new Chart(ctx, {");
            html.AppendLine("    type: 'doughnut',");
            html.AppendLine("    data: {");
            html.AppendLine($"        labels: {System.Text.Json.JsonSerializer.Serialize(customerBookings.Select(c => c.CustomerName).ToArray())},");
            html.AppendLine("        datasets: [{");
            html.AppendLine("            label: 'Booking Count',");
            html.AppendLine($"            data: {System.Text.Json.JsonSerializer.Serialize(customerBookings.Select(c => c.BookingCount).ToArray())},");
            html.AppendLine("            backgroundColor: [");
            html.AppendLine("                '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF',");
            html.AppendLine("                '#FF9F40', '#FF6384', '#C9CBCF', '#4BC0C0', '#FF6384'");
            html.AppendLine("            ],");
            html.AppendLine("            borderWidth: 2,");
            html.AppendLine("            borderColor: '#fff'");
            html.AppendLine("        }]");
            html.AppendLine("    },");
            html.AppendLine("    options: {");
            html.AppendLine("        responsive: true,");
            html.AppendLine("        maintainAspectRatio: false,");
            html.AppendLine("        plugins: {");
            html.AppendLine("            title: {");
            html.AppendLine("                display: true,");
            html.AppendLine("                text: 'Top 10 Customers by Booking Count - FUMiniHotel System',");
            html.AppendLine("                font: { size: 16, weight: 'bold' }");
            html.AppendLine("            },");
            html.AppendLine("            legend: {");
            html.AppendLine("                display: true,");
            html.AppendLine("                position: 'bottom'");
            html.AppendLine("            }");
            html.AppendLine("        }");
            html.AppendLine("    }");
            html.AppendLine("});");
            html.AppendLine("</script>");
            
            // Customer details table
            html.AppendLine("<div class='customer-list'>");
            html.AppendLine("<h3>📋 Top Customer Details</h3>");
            foreach (var customer in customerBookings)
            {
                html.AppendLine($"<div class='customer-item'>");
                html.AppendLine($"<span><strong>{customer.CustomerName}</strong></span>");
                html.AppendLine($"<span>{customer.BookingCount} bookings | ${customer.TotalSpent:0} spent</span>");
                html.AppendLine("</div>");
            }
            html.AppendLine("</div>");
            
            html.AppendLine("</div>");
            html.AppendLine("</body></html>");

            var filePath = Path.Combine("Reports", $"{fileName}_CustomerDistribution_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, html.ToString());

            return filePath;
        }

        public async Task<string> ExportRoomTypeChartAsync(List<RoomInformation> rooms, List<RoomType> roomTypes, List<Booking> bookings, string fileName)
        {
            var roomTypeStats = roomTypes.Select(rt => new
            {
                RoomTypeName = rt.RoomTypeName,
                RoomCount = rooms.Count(r => r.RoomTypeID == rt.RoomTypeID),
                BookingCount = bookings.Count(b => rooms.Any(r => r.RoomID == b.RoomID && r.RoomTypeID == rt.RoomTypeID)),
                Revenue = bookings.Where(b => rooms.Any(r => r.RoomID == b.RoomID && r.RoomTypeID == rt.RoomTypeID))
                                .Sum(b => b.TotalAmount)
            }).ToList();

            var chartData = new
            {
                type = "bar",
                data = new
                {
                    labels = roomTypeStats.Select(r => r.RoomTypeName).ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            label = "Room Count",
                            data = roomTypeStats.Select(r => r.RoomCount).ToArray(),
                            backgroundColor = "rgba(54, 162, 235, 0.6)",
                            borderColor = "rgba(54, 162, 235, 1)",
                            borderWidth = 1
                        },
                        new
                        {
                            label = "Booking Count",
                            data = roomTypeStats.Select(r => r.BookingCount).ToArray(),
                            backgroundColor = "rgba(255, 99, 132, 0.6)",
                            borderColor = "rgba(255, 99, 132, 1)",
                            borderWidth = 1
                        }
                    }
                },
                options = new
                {
                    responsive = true,
                    plugins = new
                    {
                        title = new
                        {
                            display = true,
                            text = "Room Type Performance"
                        }
                    },
                    scales = new
                    {
                        y = new
                        {
                            beginAtZero = true
                        }
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(chartData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var filePath = Path.Combine("Reports", $"{fileName}_RoomType_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, json);

            return filePath;
        }

        public async Task<string> ExportOccupancyChartAsync(List<Booking> bookings, List<RoomInformation> rooms, string fileName)
        {
            var monthlyOccupancy = bookings
                .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    OccupancyRate = (double)g.Count() / rooms.Count * 100
                })
                .ToList();

            var chartData = new
            {
                type = "line",
                data = new
                {
                    labels = monthlyOccupancy.Select(m => m.Month).ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            label = "Occupancy Rate (%)",
                            data = monthlyOccupancy.Select(m => Math.Round(m.OccupancyRate, 2)).ToArray(),
                            borderColor = "rgb(255, 159, 64)",
                            backgroundColor = "rgba(255, 159, 64, 0.2)",
                            tension = 0.1,
                            fill = true
                        }
                    }
                },
                options = new
                {
                    responsive = true,
                    plugins = new
                    {
                        title = new
                        {
                            display = true,
                            text = "Monthly Occupancy Rate"
                        }
                    },
                    scales = new
                    {
                        y = new
                        {
                            beginAtZero = true,
                            max = 100,
                            ticks = new
                            {
                                callback = "function(value) { return value + '%'; }"
                            }
                        }
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(chartData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var filePath = Path.Combine("Reports", $"{fileName}_Occupancy_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, json);

            return filePath;
        }

        public async Task<string> ExportFinancialSummaryAsync(List<Booking> bookings, string fileName)
        {
            var financialData = new
            {
                TotalRevenue = bookings.Sum(b => b.TotalAmount),
                AverageBookingValue = bookings.Average(b => b.TotalAmount),
                TotalBookings = bookings.Count,
                RevenueByYear = bookings
                    .GroupBy(b => b.CheckInDate.Year)
                    .Select(g => new { Year = g.Key, Revenue = g.Sum(b => b.TotalAmount) })
                    .OrderBy(g => g.Year),
                RevenueByMonth = bookings
                    .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                    .Select(g => new { 
                        Year = g.Key.Year, 
                        Month = g.Key.Month, 
                        Revenue = g.Sum(b => b.TotalAmount) 
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month),
                TopRevenueMonths = bookings
                    .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                    .Select(g => new { 
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                        Revenue = g.Sum(b => b.TotalAmount) 
                    })
                    .OrderByDescending(g => g.Revenue)
                    .Take(5)
            };

            var json = System.Text.Json.JsonSerializer.Serialize(financialData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var filePath = Path.Combine("Reports", $"{fileName}_FinancialSummary_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, json);

            return filePath;
        }

        public async Task<string> ExportRealtimeDashboardAsync(RealtimeData data, string fileName)
        {
            var dashboardData = new
            {
                timestamp = data.Timestamp,
                summary = new
                {
                    totalCustomers = data.TotalCustomers,
                    totalBookings = data.TotalBookings,
                    totalRooms = data.TotalRooms,
                    totalRevenue = data.TotalRevenue,
                    averageBookingValue = data.AverageBookingValue,
                    occupancyRate = data.OccupancyRate
                },
                charts = new
                {
                    revenueChart = new
                    {
                        type = "line",
                        data = new
                        {
                            labels = data.MonthlyRevenue.Select(m => m.MonthName).ToArray(),
                            datasets = new[]
                            {
                                new
                                {
                                    label = "Monthly Revenue",
                                    data = data.MonthlyRevenue.Select(m => m.Revenue).ToArray(),
                                    borderColor = "rgb(75, 192, 192)",
                                    backgroundColor = "rgba(75, 192, 192, 0.2)",
                                    tension = 0.1
                                }
                            }
                        },
                        options = new
                        {
                            responsive = true,
                            plugins = new
                            {
                                title = new
                                {
                                    display = true,
                                    text = "Monthly Revenue Trend (Realtime)"
                                }
                            }
                        }
                    },
                    customerChart = new
                    {
                        type = "doughnut",
                        data = new
                        {
                            labels = data.CustomerDistribution.Select(c => c.CustomerName).ToArray(),
                            datasets = new[]
                            {
                                new
                                {
                                    label = "Booking Count",
                                    data = data.CustomerDistribution.Select(c => c.BookingCount).ToArray(),
                                    backgroundColor = new[]
                                    {
                                        "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
                                        "#FF9F40", "#FF6384", "#C9CBCF", "#4BC0C0", "#FF6384"
                                    }
                                }
                            }
                        },
                        options = new
                        {
                            responsive = true,
                            plugins = new
                            {
                                title = new
                                {
                                    display = true,
                                    text = "Top Customers by Booking Count (Realtime)"
                                }
                            }
                        }
                    },
                    roomTypeChart = new
                    {
                        type = "bar",
                        data = new
                        {
                            labels = data.RoomTypePerformance.Select(r => r.RoomTypeName).ToArray(),
                            datasets = new[]
                            {
                                new
                                {
                                    label = "Room Count",
                                    data = data.RoomTypePerformance.Select(r => r.RoomCount).ToArray(),
                                    backgroundColor = "rgba(54, 162, 235, 0.6)",
                                    borderColor = "rgba(54, 162, 235, 1)",
                                    borderWidth = 1
                                },
                                new
                                {
                                    label = "Booking Count",
                                    data = data.RoomTypePerformance.Select(r => r.BookingCount).ToArray(),
                                    backgroundColor = "rgba(255, 99, 132, 0.6)",
                                    borderColor = "rgba(255, 99, 132, 1)",
                                    borderWidth = 1
                                }
                            }
                        },
                        options = new
                        {
                            responsive = true,
                            plugins = new
                            {
                                title = new
                                {
                                    display = true,
                                    text = "Room Type Performance (Realtime)"
                                }
                            }
                        }
                    }
                },
                recentBookings = data.RecentBookings.Select(b => new
                {
                    bookingId = b.BookingID,
                    customerName = b.CustomerName,
                    roomNumber = b.RoomNumber,
                    checkInDate = b.CheckInDate.ToString("yyyy-MM-dd"),
                    totalAmount = b.TotalAmount
                }).ToArray()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(dashboardData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var filePath = Path.Combine("Reports", $"{fileName}_RealtimeDashboard_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, json);

            return filePath;
        }
    }
}
