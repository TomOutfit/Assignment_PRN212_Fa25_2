using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FUMiniHotelSystem.Models;

namespace StudentNameWPF.Services
{
    public class ExcelExportService
    {
        public async Task<string> ExportToExcelAsync(List<Booking> bookings, List<Customer> customers, List<RoomInformation> rooms, List<RoomType> roomTypes, string fileName, string? customFilePath = null)
        {
            var csv = new StringBuilder();
            
            // Sheet 1: Executive Summary
            csv.AppendLine("EXECUTIVE SUMMARY");
            csv.AppendLine($"Report Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine($"Total Revenue,${bookings.Sum(b => b.TotalAmount):N2}");
            csv.AppendLine($"Total Bookings,{bookings.Count}");
            csv.AppendLine($"Average Booking Value,${bookings.Average(b => b.TotalAmount):N2}");
            csv.AppendLine($"Total Customers,{customers.Count}");
            csv.AppendLine($"Total Rooms,{rooms.Count}");
            csv.AppendLine($"Average Occupancy Rate,{(double)bookings.Count / rooms.Count * 100:F1}%");
            csv.AppendLine("");
            
            // Sheet 2: Monthly Revenue
            csv.AppendLine("MONTHLY REVENUE ANALYSIS");
            csv.AppendLine("Month,Revenue,Bookings,Average per Booking");
            
            var monthlyRevenue = bookings
                .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { 
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                    Revenue = g.Sum(b => b.TotalAmount),
                    Bookings = g.Count()
                });
            
            foreach (var month in monthlyRevenue)
            {
                var avgPerBooking = month.Revenue / month.Bookings;
                csv.AppendLine($"{month.Month},${month.Revenue:N2},{month.Bookings},${avgPerBooking:N2}");
            }
            csv.AppendLine("");
            
            // Sheet 3: Top Customers
            csv.AppendLine("TOP CUSTOMERS BY REVENUE");
            csv.AppendLine("Customer Name,Total Spent,Bookings,Average per Booking,Email");
            
            var topCustomers = bookings
                .GroupBy(b => b.CustomerID)
                .Select(g => new { 
                    CustomerID = g.Key, 
                    CustomerName = customers.FirstOrDefault(c => c.CustomerID == g.Key)?.CustomerFullName ?? "Unknown",
                    Email = customers.FirstOrDefault(c => c.CustomerID == g.Key)?.EmailAddress ?? "Unknown",
                    TotalSpent = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(20);
            
            foreach (var customer in topCustomers)
            {
                var avgPerBooking = customer.TotalSpent / customer.BookingCount;
                csv.AppendLine($"{customer.CustomerName},${customer.TotalSpent:N2},{customer.BookingCount},${avgPerBooking:N2},{customer.Email}");
            }
            csv.AppendLine("");
            
            // Sheet 4: Room Performance
            csv.AppendLine("ROOM PERFORMANCE ANALYSIS");
            csv.AppendLine("Room Number,Room Type,Revenue,Bookings,Average per Booking,Occupancy Rate");
            
            var roomPerformance = bookings
                .GroupBy(b => b.RoomID)
                .Select(g => new {
                    RoomID = g.Key,
                    RoomNumber = rooms.FirstOrDefault(r => r.RoomID == g.Key)?.RoomNumber ?? "Unknown",
                    RoomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == rooms.FirstOrDefault(r => r.RoomID == g.Key)?.RoomTypeID)?.RoomTypeName ?? "Unknown",
                    Revenue = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderByDescending(r => r.Revenue)
                .Take(20);
            
            foreach (var room in roomPerformance)
            {
                var avgPerBooking = room.Revenue / room.BookingCount;
                var occupancyRate = (double)room.BookingCount / 1 * 100; // Simplified calculation
                csv.AppendLine($"{room.RoomNumber},{room.RoomType},${room.Revenue:N2},{room.BookingCount},${avgPerBooking:N2},{occupancyRate:F1}%");
            }
            csv.AppendLine("");
            
            // Sheet 5: Room Type Analysis
            csv.AppendLine("ROOM TYPE ANALYSIS");
            csv.AppendLine("Room Type,Room Count,Average Price,Total Revenue,Bookings,Occupancy Rate");
            
            var roomTypeStats = rooms
                .GroupBy(r => r.RoomTypeID)
                .Select(g => new {
                    RoomTypeID = g.Key,
                    RoomTypeName = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == g.Key)?.RoomTypeName ?? "Unknown",
                    RoomCount = g.Count(),
                    AvgPrice = g.Average(r => r.RoomPricePerDate),
                    TotalRevenue = bookings.Where(b => g.Any(r => r.RoomID == b.RoomID)).Sum(b => b.TotalAmount),
                    BookingCount = bookings.Count(b => g.Any(r => r.RoomID == b.RoomID))
                })
                .OrderByDescending(rt => rt.TotalRevenue);
            
            foreach (var roomType in roomTypeStats)
            {
                var occupancyRate = (double)roomType.BookingCount / roomType.RoomCount * 100;
                csv.AppendLine($"{roomType.RoomTypeName},{roomType.RoomCount},${roomType.AvgPrice:N2},${roomType.TotalRevenue:N2},{roomType.BookingCount},{occupancyRate:F1}%");
            }
            csv.AppendLine("");
            
            // Sheet 6: Seasonal Analysis
            csv.AppendLine("SEASONAL ANALYSIS");
            csv.AppendLine("Season,Revenue,Bookings,Average per Booking,Percentage of Total");
            
            var seasonalRevenue = bookings
                .GroupBy(b => GetSeason(b.CheckInDate))
                .Select(g => new { Season = g.Key, Revenue = g.Sum(b => b.TotalAmount), Bookings = g.Count() })
                .OrderByDescending(s => s.Revenue);
            
            var totalRevenue = bookings.Sum(b => b.TotalAmount);
            foreach (var season in seasonalRevenue)
            {
                var avgPerBooking = season.Revenue / season.Bookings;
                var percentage = (season.Revenue / totalRevenue) * 100;
                csv.AppendLine($"{season.Season},${season.Revenue:N2},{season.Bookings},${avgPerBooking:N2},{percentage:F1}%");
            }
            csv.AppendLine("");
            
            // Sheet 7: Detailed Bookings
            csv.AppendLine("DETAILED BOOKINGS");
            csv.AppendLine("Booking ID,Customer Name,Email,Room Number,Room Type,Check-in Date,Check-out Date,Duration (Days),Total Amount,Booking Date");
            
            var detailedBookings = bookings
                .Select(b => new {
                    BookingID = b.BookingID,
                    CustomerName = customers.FirstOrDefault(c => c.CustomerID == b.CustomerID)?.CustomerFullName ?? "Unknown",
                    Email = customers.FirstOrDefault(c => c.CustomerID == b.CustomerID)?.EmailAddress ?? "Unknown",
                    RoomNumber = rooms.FirstOrDefault(r => r.RoomID == b.RoomID)?.RoomNumber ?? "Unknown",
                    RoomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == rooms.FirstOrDefault(r => r.RoomID == b.RoomID)?.RoomTypeID)?.RoomTypeName ?? "Unknown",
                    CheckInDate = b.CheckInDate.ToString("yyyy-MM-dd"),
                    CheckOutDate = b.CheckOutDate.ToString("yyyy-MM-dd"),
                    Duration = (b.CheckOutDate - b.CheckInDate).Days,
                    TotalAmount = b.TotalAmount,
                    BookingDate = b.CreatedDate.ToString("yyyy-MM-dd")
                })
                .OrderByDescending(b => b.BookingDate);
            
            foreach (var booking in detailedBookings)
            {
                csv.AppendLine($"{booking.BookingID},{booking.CustomerName},{booking.Email},{booking.RoomNumber},{booking.RoomType},{booking.CheckInDate},{booking.CheckOutDate},{booking.Duration},${booking.TotalAmount:N2},{booking.BookingDate}");
            }
            
            var filePath = customFilePath ?? Path.Combine("Reports", $"{fileName}_Excel_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllTextAsync(filePath, csv.ToString());
            
            return filePath;
        }
        
        public async Task<string> ExportCustomerAnalysisAsync(List<Customer> customers, List<Booking> bookings, string fileName)
        {
            var csv = new StringBuilder();
            
            csv.AppendLine("CUSTOMER ANALYSIS REPORT");
            csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine("");
            
            csv.AppendLine("CUSTOMER DETAILS");
            csv.AppendLine("Customer ID,Full Name,Email,Telephone,Birthday,Age,Total Bookings,Total Spent,Average per Booking,Last Booking Date");
            
            var customerAnalysis = customers.Select(c => new {
                CustomerID = c.CustomerID,
                FullName = c.CustomerFullName,
                Email = c.EmailAddress,
                Telephone = c.Telephone,
                Birthday = c.CustomerBirthday.ToString("yyyy-MM-dd"),
                Age = DateTime.Now.Year - c.CustomerBirthday.Year,
                TotalBookings = bookings.Count(b => b.CustomerID == c.CustomerID),
                TotalSpent = bookings.Where(b => b.CustomerID == c.CustomerID).Sum(b => b.TotalAmount),
                LastBookingDate = bookings.Where(b => b.CustomerID == c.CustomerID).Max(b => b.CreatedDate).ToString("yyyy-MM-dd")
            }).OrderByDescending(c => c.TotalSpent);
            
            foreach (var customer in customerAnalysis)
            {
                var avgPerBooking = customer.TotalBookings > 0 ? customer.TotalSpent / customer.TotalBookings : 0;
                csv.AppendLine($"{customer.CustomerID},{customer.FullName},{customer.Email},{customer.Telephone},{customer.Birthday},{customer.Age},{customer.TotalBookings},${customer.TotalSpent:N2},${avgPerBooking:N2},{customer.LastBookingDate}");
            }
            
            var filePath = Path.Combine("Reports", $"{fileName}_CustomerAnalysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(filePath, csv.ToString());
            
            return filePath;
        }
        
        private string GetSeason(DateTime date)
        {
            int month = date.Month;
            if (month >= 3 && month <= 5) return "Spring";
            if (month >= 6 && month <= 8) return "Summer";
            if (month >= 9 && month <= 11) return "Autumn";
            return "Winter";
        }
    }
}
