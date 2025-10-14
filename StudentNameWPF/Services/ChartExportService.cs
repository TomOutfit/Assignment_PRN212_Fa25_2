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
                    Revenue = g.Sum(b => b.TotalAmount) 
                })
                .ToList();

            var chartData = new
            {
                type = "line",
                data = new
                {
                    labels = monthlyRevenue.Select(m => m.Month).ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            label = "Monthly Revenue",
                            data = monthlyRevenue.Select(m => m.Revenue).ToArray(),
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
                            text = "Monthly Revenue Trend"
                        }
                    },
                    scales = new
                    {
                        y = new
                        {
                            beginAtZero = true,
                            ticks = new
                            {
                                callback = "function(value) { return '$' + value.toLocaleString(); }"
                            }
                        }
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(chartData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var filePath = Path.Combine("Reports", $"{fileName}_Revenue_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, json);

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

            var chartData = new
            {
                type = "doughnut",
                data = new
                {
                    labels = customerBookings.Select(c => c.CustomerName).ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            label = "Booking Count",
                            data = customerBookings.Select(c => c.BookingCount).ToArray(),
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
                            text = "Top 10 Customers by Booking Count"
                        },
                        legend = new
                        {
                            position = "bottom"
                        }
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(chartData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var filePath = Path.Combine("Reports", $"{fileName}_CustomerDistribution_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, json);

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
