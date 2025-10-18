using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FUMiniHotelSystem.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace StudentNameWPF.Services
{
    public class ExcelExportService
    {
        public async Task<string> ExportToExcelAsync(List<Booking> bookings, List<Customer> customers, List<RoomInformation> rooms, List<RoomType> roomTypes, string fileName, string? customFilePath = null)
        {
            var filePath = customFilePath ?? Path.Combine("Reports", $"{fileName}_Excel_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                using var package = new ExcelPackage();
            
            // Sheet 1: Executive Summary
            var summarySheet = package.Workbook.Worksheets.Add("Executive Summary");
            CreateExecutiveSummarySheet(summarySheet, bookings, customers, rooms, roomTypes);
            
            // Sheet 2: Monthly Revenue Analysis
            var revenueSheet = package.Workbook.Worksheets.Add("Monthly Revenue");
            CreateMonthlyRevenueSheet(revenueSheet, bookings);
            
            // Sheet 3: Top Customers
            var customersSheet = package.Workbook.Worksheets.Add("Top Customers");
            CreateTopCustomersSheet(customersSheet, bookings, customers);
            
            // Sheet 4: Room Performance
            var roomsSheet = package.Workbook.Worksheets.Add("Room Performance");
            CreateRoomPerformanceSheet(roomsSheet, bookings, rooms, roomTypes);
            
            // Sheet 5: Room Type Analysis
            var roomTypesSheet = package.Workbook.Worksheets.Add("Room Type Analysis");
            CreateRoomTypeAnalysisSheet(roomTypesSheet, bookings, rooms, roomTypes);
            
            // Sheet 6: Seasonal Analysis
            var seasonalSheet = package.Workbook.Worksheets.Add("Seasonal Analysis");
            CreateSeasonalAnalysisSheet(seasonalSheet, bookings);
            
            // Sheet 7: Detailed Bookings
            var bookingsSheet = package.Workbook.Worksheets.Add("Detailed Bookings");
            CreateDetailedBookingsSheet(bookingsSheet, bookings, customers, rooms, roomTypes);
            
                // Save the Excel file
                await package.SaveAsAsync(new FileInfo(filePath));
                
                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excel export failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Falling back to CSV export...");
                
                // Fallback to CSV export
                return await ExportToCSVAsync(bookings, customers, rooms, roomTypes, fileName, customFilePath);
            }
        }
        
        public async Task<string> ExportToCSVAsync(List<Booking> bookings, List<Customer> customers, List<RoomInformation> rooms, List<RoomType> roomTypes, string fileName, string? customFilePath = null)
        {
            var csvContent = new StringBuilder();

            // Sheet 1: Executive Summary
            csvContent.AppendLine("=== EXECUTIVE SUMMARY ===");
            csvContent.AppendLine("Metric,Value,Format,Status");
            csvContent.AppendLine($"Total Revenue,${bookings.Sum(b => b.TotalAmount):0},Currency,{(bookings.Sum(b => b.TotalAmount) > 50000 ? "Excellent" : "Good")}");
            csvContent.AppendLine($"Total Bookings,{bookings.Count},Count,{(bookings.Count > 100 ? "High Volume" : "Normal")}");
            csvContent.AppendLine($"Average Booking Value,${bookings.Average(b => b.TotalAmount):0},Currency,{(bookings.Average(b => b.TotalAmount) > 500 ? "Premium" : "Standard")}");
            csvContent.AppendLine($"Total Customers,{customers.Count},Count,{(customers.Count > 50 ? "Large Base" : "Growing")}");
            csvContent.AppendLine($"Total Rooms,{rooms.Count},Count,{(rooms.Count > 30 ? "Large Property" : "Medium Property")}");
            csvContent.AppendLine($"Occupancy Rate,{(double)bookings.Count / rooms.Count * 100:F1}%,Percentage,{((double)bookings.Count / rooms.Count * 100 > 70 ? "High Occupancy" : "Room for Growth")}");
            csvContent.AppendLine($"Peak Revenue Month,{GetPeakRevenueMonth(bookings)},Date,Best Performance");
            csvContent.AppendLine($"Top Customer,{GetTopCustomer(bookings, customers)},Name,Highest Spender");
            csvContent.AppendLine($"Most Popular Room Type,{GetMostPopularRoomType(bookings, rooms, roomTypes)},Type,Most Booked");
            csvContent.AppendLine("");

            // Sheet 2: Monthly Revenue
            csvContent.AppendLine("=== MONTHLY REVENUE ===");
            csvContent.AppendLine("Month,Revenue,Bookings,Average per Booking");
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
                var avgPerBooking = month.Bookings > 0 ? month.Revenue / month.Bookings : 0;
                csvContent.AppendLine($"{month.Month},${month.Revenue:0},{month.Bookings},${avgPerBooking:0}");
            }
            csvContent.AppendLine("");

            // Sheet 3: Top Customers
            csvContent.AppendLine("=== TOP CUSTOMERS ===");
            csvContent.AppendLine("Rank,Customer,Total Spent,Bookings,Average per Booking,Email");
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
            int rank = 1;
            foreach (var customer in topCustomers)
            {
                var avgPerBooking = customer.BookingCount > 0 ? customer.TotalSpent / customer.BookingCount : 0;
                csvContent.AppendLine($"#{rank},{customer.CustomerName},${customer.TotalSpent:0},{customer.BookingCount},${avgPerBooking:0},{customer.Email}");
                rank++;
            }
            csvContent.AppendLine("");

            // Sheet 4: Room Performance
            csvContent.AppendLine("=== ROOM PERFORMANCE ===");
            csvContent.AppendLine("Room Number,Room Type,Revenue,Bookings,Average per Booking");
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
                var avgPerBooking = room.BookingCount > 0 ? room.Revenue / room.BookingCount : 0;
                csvContent.AppendLine($"{room.RoomNumber},{room.RoomType},${room.Revenue:0},{room.BookingCount},${avgPerBooking:0}");
            }
            csvContent.AppendLine("");

            // Sheet 5: Room Type Analysis
            csvContent.AppendLine("=== ROOM TYPE ANALYSIS ===");
            csvContent.AppendLine("Room Type,Room Count,Average Price,Total Revenue,Bookings,Occupancy Rate");
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
                csvContent.AppendLine($"{roomType.RoomTypeName},{roomType.RoomCount},${roomType.AvgPrice:0},${roomType.TotalRevenue:0},{roomType.BookingCount},{occupancyRate:F1}%");
            }
            csvContent.AppendLine("");

            // Sheet 6: Seasonal Analysis
            csvContent.AppendLine("=== SEASONAL ANALYSIS ===");
            csvContent.AppendLine("Season,Revenue,Bookings,Average per Booking,Percentage of Total");
            var seasonalRevenue = bookings
                .GroupBy(b => GetSeason(b.CheckInDate))
                .Select(g => new { Season = g.Key, Revenue = g.Sum(b => b.TotalAmount), Bookings = g.Count() })
                .OrderByDescending(s => s.Revenue);
            var totalRevenue = bookings.Sum(b => b.TotalAmount);
            foreach (var season in seasonalRevenue)
            {
                var avgPerBooking = season.Bookings > 0 ? season.Revenue / season.Bookings : 0;
                var percentage = totalRevenue > 0 ? (season.Revenue / totalRevenue) * 100 : 0;
                csvContent.AppendLine($"{season.Season},${season.Revenue:0},{season.Bookings},${avgPerBooking:0},{percentage:F1}%");
            }
            csvContent.AppendLine("");

            // Sheet 7: Detailed Bookings
            csvContent.AppendLine("=== DETAILED BOOKINGS ===");
            csvContent.AppendLine("Booking ID,Customer Name,Email,Room Number,Room Type,Check-in Date,Check-out Date,Duration (Days),Total Amount,Booking Date");
            var detailedBookings = bookings
                .Select(b => new {
                    BookingID = b.BookingID,
                    CustomerName = customers.FirstOrDefault(c => c.CustomerID == b.CustomerID)?.CustomerFullName ?? "Unknown",
                    Email = customers.FirstOrDefault(c => c.CustomerID == b.CustomerID)?.EmailAddress ?? "Unknown",
                    RoomNumber = rooms.FirstOrDefault(r => r.RoomID == b.RoomID)?.RoomNumber ?? "Unknown",
                    RoomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == rooms.FirstOrDefault(r => r.RoomID == b.RoomID)?.RoomTypeID)?.RoomTypeName ?? "Unknown",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Duration = (b.CheckOutDate - b.CheckInDate).Days,
                    TotalAmount = b.TotalAmount,
                    BookingDate = b.CreatedDate
                })
                .OrderByDescending(b => b.BookingDate);
            foreach (var booking in detailedBookings)
            {
                csvContent.AppendLine($"{booking.BookingID},{booking.CustomerName},{booking.Email},{booking.RoomNumber},{booking.RoomType},{booking.CheckInDate:yyyy-MM-dd},{booking.CheckOutDate:yyyy-MM-dd},{booking.Duration},${booking.TotalAmount:0},{booking.BookingDate:yyyy-MM-dd}");
            }

            var csvFilePath = customFilePath?.Replace(".xlsx", ".csv") ?? Path.Combine("Reports", $"{fileName}_CSV_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            var csvDirectory = Path.GetDirectoryName(csvFilePath);
            if (!string.IsNullOrEmpty(csvDirectory))
            {
                Directory.CreateDirectory(csvDirectory);
            }
            await File.WriteAllTextAsync(csvFilePath, csvContent.ToString());

            return csvFilePath;
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
                csv.AppendLine($"{customer.CustomerID},{customer.FullName},{customer.Email},{customer.Telephone},{customer.Birthday},{customer.Age},{customer.TotalBookings},${customer.TotalSpent:0},${avgPerBooking:0},{customer.LastBookingDate}");
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
        
        private string GetPeakRevenueMonth(List<Booking> bookings)
        {
            var peakMonth = bookings
                .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                .OrderByDescending(g => g.Sum(b => b.TotalAmount))
                .FirstOrDefault();
            
            return peakMonth != null ? $"{peakMonth.Key.Year}-{peakMonth.Key.Month:D2}" : "N/A";
        }
        
        private string GetTopCustomer(List<Booking> bookings, List<Customer> customers)
        {
            var topCustomer = bookings
                .GroupBy(b => b.CustomerID)
                .OrderByDescending(g => g.Sum(b => b.TotalAmount))
                .FirstOrDefault();
            
            if (topCustomer == null) return "N/A";
            
            var customer = customers.FirstOrDefault(c => c.CustomerID == topCustomer.Key);
            return customer?.CustomerFullName ?? "Unknown";
        }
        
        private string GetMostPopularRoomType(List<Booking> bookings, List<RoomInformation> rooms, List<RoomType> roomTypes)
        {
            var popularRoomType = bookings
                .GroupBy(b => b.RoomID)
                .Select(g => new { RoomID = g.Key, BookingCount = g.Count() })
                .OrderByDescending(r => r.BookingCount)
                .FirstOrDefault();
            
            if (popularRoomType == null) return "N/A";
            
            var room = rooms.FirstOrDefault(r => r.RoomID == popularRoomType.RoomID);
            if (room == null) return "N/A";
            
            var roomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == room.RoomTypeID);
            return roomType?.RoomTypeName ?? "Unknown";
        }
        
        private void CreateExecutiveSummarySheet(ExcelWorksheet sheet, List<Booking> bookings, List<Customer> customers, List<RoomInformation> rooms, List<RoomType> roomTypes)
        {
            // Header with company branding
            sheet.Cells[1, 1].Value = "FUMiniHotel System - Executive Summary";
            sheet.Cells[1, 1].Style.Font.Size = 18;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            sheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
            sheet.Cells[1, 1, 1, 4].Merge = true;
            sheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            // Report Info section
            sheet.Cells[3, 1].Value = "Report Information";
            sheet.Cells[3, 1].Style.Font.Bold = true;
            sheet.Cells[3, 1].Style.Font.Size = 14;
            sheet.Cells[3, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            sheet.Cells[3, 1, 3, 2].Merge = true;
            
            sheet.Cells[4, 1].Value = "Generated:";
            sheet.Cells[4, 2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            sheet.Cells[5, 1].Value = "Period:";
            sheet.Cells[5, 2].Value = $"{bookings.Min(b => b.CheckInDate):yyyy-MM-dd} to {bookings.Max(b => b.CheckInDate):yyyy-MM-dd}";
            
            // Key Metrics with professional table format
            int row = 7;
            sheet.Cells[row, 1].Value = "Key Performance Indicators";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 1].Style.Font.Size = 14;
            sheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
            sheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            sheet.Cells[row, 1, row, 4].Merge = true;
            
            // Table headers
            row++;
            sheet.Cells[row, 1].Value = "Metric";
            sheet.Cells[row, 2].Value = "Value";
            sheet.Cells[row, 3].Value = "Format";
            sheet.Cells[row, 4].Value = "Status";
            
            // Style headers
            for (int col = 1; col <= 4; col++)
            {
                sheet.Cells[row, col].Style.Font.Bold = true;
                sheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                sheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            
            // Data rows
            row++;
            sheet.Cells[row, 1].Value = "Total Revenue";
            sheet.Cells[row, 2].Value = bookings.Sum(b => b.TotalAmount);
            sheet.Cells[row, 2].Style.Numberformat.Format = "$#0";
            sheet.Cells[row, 3].Value = "Currency";
            sheet.Cells[row, 4].Value = bookings.Sum(b => b.TotalAmount) > 50000 ? "Excellent" : "Good";
            
            row++;
            sheet.Cells[row, 1].Value = "Total Bookings";
            sheet.Cells[row, 2].Value = bookings.Count;
            sheet.Cells[row, 3].Value = "Count";
            sheet.Cells[row, 4].Value = bookings.Count > 100 ? "High Volume" : "Normal";
            
            row++;
            sheet.Cells[row, 1].Value = "Average Booking Value";
            sheet.Cells[row, 2].Value = bookings.Average(b => b.TotalAmount);
            sheet.Cells[row, 2].Style.Numberformat.Format = "$#0";
            sheet.Cells[row, 3].Value = "Currency";
            sheet.Cells[row, 4].Value = bookings.Average(b => b.TotalAmount) > 500 ? "Premium" : "Standard";
            
            row++;
            sheet.Cells[row, 1].Value = "Total Customers";
            sheet.Cells[row, 2].Value = customers.Count;
            sheet.Cells[row, 3].Value = "Count";
            sheet.Cells[row, 4].Value = customers.Count > 50 ? "Large Base" : "Growing";
            
            row++;
            sheet.Cells[row, 1].Value = "Total Rooms";
            sheet.Cells[row, 2].Value = rooms.Count;
            sheet.Cells[row, 3].Value = "Count";
            sheet.Cells[row, 4].Value = rooms.Count > 30 ? "Large Property" : "Medium Property";
            
            row++;
            sheet.Cells[row, 1].Value = "Occupancy Rate";
            sheet.Cells[row, 2].Value = (double)bookings.Count / rooms.Count * 100;
            sheet.Cells[row, 2].Style.Numberformat.Format = "0.0%";
            sheet.Cells[row, 3].Value = "Percentage";
            var occupancyRate = (double)bookings.Count / rooms.Count * 100;
            sheet.Cells[row, 4].Value = occupancyRate > 70 ? "High Occupancy" : "Room for Growth";
            
            row++;
            sheet.Cells[row, 1].Value = "Peak Revenue Month";
            sheet.Cells[row, 2].Value = GetPeakRevenueMonth(bookings);
            sheet.Cells[row, 3].Value = "Date";
            sheet.Cells[row, 4].Value = "Best Performance";
            
            row++;
            sheet.Cells[row, 1].Value = "Top Customer";
            sheet.Cells[row, 2].Value = GetTopCustomer(bookings, customers);
            sheet.Cells[row, 3].Value = "Name";
            sheet.Cells[row, 4].Value = "Highest Spender";
            
            row++;
            sheet.Cells[row, 1].Value = "Most Popular Room Type";
            sheet.Cells[row, 2].Value = GetMostPopularRoomType(bookings, rooms, roomTypes);
            sheet.Cells[row, 3].Value = "Type";
            sheet.Cells[row, 4].Value = "Most Booked";
            
            // Apply borders to all data cells
            for (int r = 8; r <= row; r++)
            {
                for (int c = 1; c <= 4; c++)
                {
                    sheet.Cells[r, c].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }
            
            // Set column widths explicitly
            sheet.Column(1).Width = 25;
            sheet.Column(2).Width = 20;
            sheet.Column(3).Width = 15;
            sheet.Column(4).Width = 20;
            
            // Auto-fit columns
            sheet.Cells.AutoFitColumns();
        }
        
        private void CreateMonthlyRevenueSheet(ExcelWorksheet sheet, List<Booking> bookings)
        {
            // Header with professional styling
            sheet.Cells[1, 1].Value = "Monthly Revenue Analysis";
            sheet.Cells[1, 1].Style.Font.Size = 18;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            sheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);
            sheet.Cells[1, 1, 1, 4].Merge = true;
            sheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            // Column headers with color coding
            sheet.Cells[3, 1].Value = "Month";
            sheet.Cells[3, 2].Value = "Revenue";
            sheet.Cells[3, 3].Value = "Bookings";
            sheet.Cells[3, 4].Value = "Average per Booking";
            
            // Style headers with different colors
            sheet.Cells[3, 1].Style.Font.Bold = true;
            sheet.Cells[3, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            sheet.Cells[3, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            
            sheet.Cells[3, 2].Style.Font.Bold = true;
            sheet.Cells[3, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[3, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            sheet.Cells[3, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            
            sheet.Cells[3, 3].Style.Font.Bold = true;
            sheet.Cells[3, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[3, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
            sheet.Cells[3, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            
            sheet.Cells[3, 4].Style.Font.Bold = true;
            sheet.Cells[3, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[3, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
            sheet.Cells[3, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            
            // Data with alternating row colors
            var monthlyRevenue = bookings
                .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { 
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                    Revenue = g.Sum(b => b.TotalAmount),
                    Bookings = g.Count()
                })
                .ToList();
            
            int row = 4;
            bool isEvenRow = false;
            foreach (var month in monthlyRevenue)
            {
                // Alternating row colors
                var rowColor = isEvenRow ? System.Drawing.Color.LightGray : System.Drawing.Color.White;
                
                // Ensure data goes to correct columns
                sheet.Cells[row, 1].Value = month.Month;
                sheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                sheet.Cells[row, 2].Value = month.Revenue;
                sheet.Cells[row, 2].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                sheet.Cells[row, 3].Value = month.Bookings;
                sheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                sheet.Cells[row, 4].Value = month.Bookings > 0 ? month.Revenue / month.Bookings : 0;
                sheet.Cells[row, 4].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                isEvenRow = !isEvenRow;
                row++;
            }
            
            // Add summary row
            sheet.Cells[row, 1].Value = "TOTAL";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
            sheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            sheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            
            sheet.Cells[row, 2].Value = monthlyRevenue.Sum(m => m.Revenue);
            sheet.Cells[row, 2].Style.Numberformat.Format = "$#0";
            sheet.Cells[row, 2].Style.Font.Bold = true;
            sheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
            sheet.Cells[row, 2].Style.Font.Color.SetColor(System.Drawing.Color.White);
            sheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            
            sheet.Cells[row, 3].Value = monthlyRevenue.Sum(m => m.Bookings);
            sheet.Cells[row, 3].Style.Font.Bold = true;
            sheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
            sheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.White);
            sheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            
            sheet.Cells[row, 4].Value = monthlyRevenue.Sum(m => m.Revenue) / monthlyRevenue.Sum(m => m.Bookings);
            sheet.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";
            sheet.Cells[row, 4].Style.Font.Bold = true;
            sheet.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
            sheet.Cells[row, 4].Style.Font.Color.SetColor(System.Drawing.Color.White);
            sheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            
            // Set column widths explicitly
            sheet.Column(1).Width = 15; // Month
            sheet.Column(2).Width = 15; // Revenue
            sheet.Column(3).Width = 12; // Bookings
            sheet.Column(4).Width = 18; // Average per Booking
            
            // Auto-fit columns
            sheet.Cells.AutoFitColumns();
        }
        
        private void CreateTopCustomersSheet(ExcelWorksheet sheet, List<Booking> bookings, List<Customer> customers)
        {
            // Header with professional styling
            sheet.Cells[1, 1].Value = "Top Customers by Revenue";
            sheet.Cells[1, 1].Style.Font.Size = 18;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            sheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkOrange);
            sheet.Cells[1, 1, 1, 6].Merge = true;
            sheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            // Column headers with color coding
            sheet.Cells[3, 1].Value = "Rank";
            sheet.Cells[3, 2].Value = "Customer Name";
            sheet.Cells[3, 3].Value = "Total Spent";
            sheet.Cells[3, 4].Value = "Bookings";
            sheet.Cells[3, 5].Value = "Average per Booking";
            sheet.Cells[3, 6].Value = "Email";
            
            // Style headers with different colors
            var headerColors = new System.Drawing.Color[] {
                System.Drawing.Color.Gold,
                System.Drawing.Color.LightBlue,
                System.Drawing.Color.LightGreen,
                System.Drawing.Color.LightCoral,
                System.Drawing.Color.LightYellow,
                System.Drawing.Color.LightPink
            };
            
            for (int col = 1; col <= 6; col++)
            {
                sheet.Cells[3, col].Style.Font.Bold = true;
                sheet.Cells[3, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, col].Style.Fill.BackgroundColor.SetColor(headerColors[col - 1]);
                sheet.Cells[3, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            
            // Data with alternating row colors and ranking
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
                .Take(20)
                .ToList();
            
            int row = 4;
            int rank = 1;
            bool isEvenRow = false;
            foreach (var customer in topCustomers)
            {
                var rowColor = isEvenRow ? System.Drawing.Color.LightGray : System.Drawing.Color.White;
                
                // Add ranking in column A (index 1)
                sheet.Cells[row, 1].Value = $"#{rank}";
                sheet.Cells[row, 1].Style.Font.Bold = true;
                sheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gold);
                sheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Customer Name in column B (index 2)
                sheet.Cells[row, 2].Value = customer.CustomerName;
                sheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                // Total Spent in column C (index 3)
                sheet.Cells[row, 3].Value = customer.TotalSpent;
                sheet.Cells[row, 3].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Bookings in column D (index 4)
                sheet.Cells[row, 4].Value = customer.BookingCount;
                sheet.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Average per Booking in column E (index 5)
                sheet.Cells[row, 5].Value = customer.BookingCount > 0 ? customer.TotalSpent / customer.BookingCount : 0;
                sheet.Cells[row, 5].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Email in column F (index 6)
                sheet.Cells[row, 6].Value = customer.Email;
                sheet.Cells[row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 6].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                isEvenRow = !isEvenRow;
                rank++;
                row++;
            }
            
            // Set column widths explicitly
            sheet.Column(1).Width = 8;  // Rank
            sheet.Column(2).Width = 25; // Customer Name
            sheet.Column(3).Width = 15; // Total Spent
            sheet.Column(4).Width = 12; // Bookings
            sheet.Column(5).Width = 18; // Average per Booking
            sheet.Column(6).Width = 30; // Email
            
            // Auto-fit columns
            sheet.Cells.AutoFitColumns();
        }
        
        private void CreateRoomPerformanceSheet(ExcelWorksheet sheet, List<Booking> bookings, List<RoomInformation> rooms, List<RoomType> roomTypes)
        {
            // Header
            sheet.Cells[1, 1].Value = "Room Performance Analysis";
            sheet.Cells[1, 1].Style.Font.Size = 16;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            
            // Column headers
            sheet.Cells[3, 1].Value = "Room Number";
            sheet.Cells[3, 2].Value = "Room Type";
            sheet.Cells[3, 3].Value = "Revenue";
            sheet.Cells[3, 4].Value = "Bookings";
            sheet.Cells[3, 5].Value = "Average per Booking";
            
            // Style headers
            for (int col = 1; col <= 5; col++)
            {
                sheet.Cells[3, col].Style.Font.Bold = true;
                sheet.Cells[3, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            
            // Data with proper alignment
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
                .Take(20)
                .ToList();
            
            int row = 4;
            bool isEvenRow = false;
            foreach (var room in roomPerformance)
            {
                var rowColor = isEvenRow ? System.Drawing.Color.LightGray : System.Drawing.Color.White;
                
                // Room Number in column A
                sheet.Cells[row, 1].Value = room.RoomNumber;
                sheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                // Room Type in column B
                sheet.Cells[row, 2].Value = room.RoomType;
                sheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                // Revenue in column C
                sheet.Cells[row, 3].Value = room.Revenue;
                sheet.Cells[row, 3].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Bookings in column D
                sheet.Cells[row, 4].Value = room.BookingCount;
                sheet.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Average per Booking in column E
                sheet.Cells[row, 5].Value = room.BookingCount > 0 ? room.Revenue / room.BookingCount : 0;
                sheet.Cells[row, 5].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                isEvenRow = !isEvenRow;
                row++;
            }
            
            // Set column widths explicitly
            sheet.Column(1).Width = 15; // Room Number
            sheet.Column(2).Width = 20; // Room Type
            sheet.Column(3).Width = 15; // Revenue
            sheet.Column(4).Width = 12; // Bookings
            sheet.Column(5).Width = 18; // Average per Booking
            
            // Auto-fit columns
            sheet.Cells.AutoFitColumns();
        }
        
        private void CreateRoomTypeAnalysisSheet(ExcelWorksheet sheet, List<Booking> bookings, List<RoomInformation> rooms, List<RoomType> roomTypes)
        {
            // Header
            sheet.Cells[1, 1].Value = "Room Type Analysis";
            sheet.Cells[1, 1].Style.Font.Size = 16;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            
            // Column headers
            sheet.Cells[3, 1].Value = "Room Type";
            sheet.Cells[3, 2].Value = "Room Count";
            sheet.Cells[3, 3].Value = "Average Price";
            sheet.Cells[3, 4].Value = "Total Revenue";
            sheet.Cells[3, 5].Value = "Bookings";
            sheet.Cells[3, 6].Value = "Occupancy Rate";
            
            // Style headers
            for (int col = 1; col <= 6; col++)
            {
                sheet.Cells[3, col].Style.Font.Bold = true;
                sheet.Cells[3, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            
            // Data with proper alignment
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
                .OrderByDescending(rt => rt.TotalRevenue)
                .ToList();
            
            int row = 4;
            bool isEvenRow = false;
            foreach (var roomType in roomTypeStats)
            {
                var rowColor = isEvenRow ? System.Drawing.Color.LightGray : System.Drawing.Color.White;
                var occupancyRate = (double)roomType.BookingCount / roomType.RoomCount * 100;
                
                // Room Type in column A
                sheet.Cells[row, 1].Value = roomType.RoomTypeName;
                sheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                // Room Count in column B
                sheet.Cells[row, 2].Value = roomType.RoomCount;
                sheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Average Price in column C
                sheet.Cells[row, 3].Value = roomType.AvgPrice;
                sheet.Cells[row, 3].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Total Revenue in column D
                sheet.Cells[row, 4].Value = roomType.TotalRevenue;
                sheet.Cells[row, 4].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Bookings in column E
                sheet.Cells[row, 5].Value = roomType.BookingCount;
                sheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Occupancy Rate in column F
                sheet.Cells[row, 6].Value = occupancyRate / 100;
                sheet.Cells[row, 6].Style.Numberformat.Format = "0.0%";
                sheet.Cells[row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 6].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                isEvenRow = !isEvenRow;
                row++;
            }
            
            // Set column widths explicitly
            sheet.Column(1).Width = 20; // Room Type
            sheet.Column(2).Width = 12; // Room Count
            sheet.Column(3).Width = 15; // Average Price
            sheet.Column(4).Width = 15; // Total Revenue
            sheet.Column(5).Width = 12; // Bookings
            sheet.Column(6).Width = 15; // Occupancy Rate
            
            // Auto-fit columns
            sheet.Cells.AutoFitColumns();
        }
        
        private void CreateSeasonalAnalysisSheet(ExcelWorksheet sheet, List<Booking> bookings)
        {
            // Header
            sheet.Cells[1, 1].Value = "Seasonal Analysis";
            sheet.Cells[1, 1].Style.Font.Size = 16;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            
            // Column headers
            sheet.Cells[3, 1].Value = "Season";
            sheet.Cells[3, 2].Value = "Revenue";
            sheet.Cells[3, 3].Value = "Bookings";
            sheet.Cells[3, 4].Value = "Average per Booking";
            sheet.Cells[3, 5].Value = "Percentage of Total";
            
            // Style headers
            for (int col = 1; col <= 5; col++)
            {
                sheet.Cells[3, col].Style.Font.Bold = true;
                sheet.Cells[3, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            
            // Data with proper alignment
            var seasonalRevenue = bookings
                .GroupBy(b => GetSeason(b.CheckInDate))
                .Select(g => new { Season = g.Key, Revenue = g.Sum(b => b.TotalAmount), Bookings = g.Count() })
                .OrderByDescending(s => s.Revenue)
                .ToList();
            
            var totalRevenue = bookings.Sum(b => b.TotalAmount);
            int row = 4;
            bool isEvenRow = false;
            foreach (var season in seasonalRevenue)
            {
                var rowColor = isEvenRow ? System.Drawing.Color.LightGray : System.Drawing.Color.White;
                var avgPerBooking = season.Bookings > 0 ? season.Revenue / season.Bookings : 0;
                var percentage = totalRevenue > 0 ? (season.Revenue / totalRevenue) * 100 : 0;
                
                // Season in column A
                sheet.Cells[row, 1].Value = season.Season;
                sheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                // Revenue in column B
                sheet.Cells[row, 2].Value = season.Revenue;
                sheet.Cells[row, 2].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Bookings in column C
                sheet.Cells[row, 3].Value = season.Bookings;
                sheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Average per Booking in column D
                sheet.Cells[row, 4].Value = avgPerBooking;
                sheet.Cells[row, 4].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Percentage of Total in column E
                sheet.Cells[row, 5].Value = percentage / 100;
                sheet.Cells[row, 5].Style.Numberformat.Format = "0.0%";
                sheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                isEvenRow = !isEvenRow;
                row++;
            }
            
            // Set column widths explicitly
            sheet.Column(1).Width = 12; // Season
            sheet.Column(2).Width = 15; // Revenue
            sheet.Column(3).Width = 12; // Bookings
            sheet.Column(4).Width = 18; // Average per Booking
            sheet.Column(5).Width = 18; // Percentage of Total
            
            // Auto-fit columns
            sheet.Cells.AutoFitColumns();
        }
        
        private void CreateDetailedBookingsSheet(ExcelWorksheet sheet, List<Booking> bookings, List<Customer> customers, List<RoomInformation> rooms, List<RoomType> roomTypes)
        {
            // Header
            sheet.Cells[1, 1].Value = "Detailed Bookings";
            sheet.Cells[1, 1].Style.Font.Size = 16;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            
            // Column headers
            sheet.Cells[3, 1].Value = "Booking ID";
            sheet.Cells[3, 2].Value = "Customer Name";
            sheet.Cells[3, 3].Value = "Email";
            sheet.Cells[3, 4].Value = "Room Number";
            sheet.Cells[3, 5].Value = "Room Type";
            sheet.Cells[3, 6].Value = "Check-in Date";
            sheet.Cells[3, 7].Value = "Check-out Date";
            sheet.Cells[3, 8].Value = "Duration (Days)";
            sheet.Cells[3, 9].Value = "Total Amount";
            sheet.Cells[3, 10].Value = "Booking Date";
            
            // Style headers
            for (int col = 1; col <= 10; col++)
            {
                sheet.Cells[3, col].Style.Font.Bold = true;
                sheet.Cells[3, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            
            // Data with proper alignment
            var detailedBookings = bookings
                .Select(b => new {
                    BookingID = b.BookingID,
                    CustomerName = customers.FirstOrDefault(c => c.CustomerID == b.CustomerID)?.CustomerFullName ?? "Unknown",
                    Email = customers.FirstOrDefault(c => c.CustomerID == b.CustomerID)?.EmailAddress ?? "Unknown",
                    RoomNumber = rooms.FirstOrDefault(r => r.RoomID == b.RoomID)?.RoomNumber ?? "Unknown",
                    RoomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == rooms.FirstOrDefault(r => r.RoomID == b.RoomID)?.RoomTypeID)?.RoomTypeName ?? "Unknown",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Duration = (b.CheckOutDate - b.CheckInDate).Days,
                    TotalAmount = b.TotalAmount,
                    BookingDate = b.CreatedDate
                })
                .OrderByDescending(b => b.BookingDate)
                .ToList();
            
            int row = 4;
            bool isEvenRow = false;
            foreach (var booking in detailedBookings)
            {
                var rowColor = isEvenRow ? System.Drawing.Color.LightGray : System.Drawing.Color.White;
                
                // Booking ID in column A
                sheet.Cells[row, 1].Value = booking.BookingID;
                sheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Customer Name in column B
                sheet.Cells[row, 2].Value = booking.CustomerName;
                sheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                // Email in column C
                sheet.Cells[row, 3].Value = booking.Email;
                sheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                // Room Number in column D
                sheet.Cells[row, 4].Value = booking.RoomNumber;
                sheet.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                // Room Type in column E
                sheet.Cells[row, 5].Value = booking.RoomType;
                sheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                // Check-in Date in column F
                sheet.Cells[row, 6].Value = booking.CheckInDate;
                sheet.Cells[row, 6].Style.Numberformat.Format = "yyyy-mm-dd";
                sheet.Cells[row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 6].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Check-out Date in column G
                sheet.Cells[row, 7].Value = booking.CheckOutDate;
                sheet.Cells[row, 7].Style.Numberformat.Format = "yyyy-mm-dd";
                sheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Duration in column H
                sheet.Cells[row, 8].Value = booking.Duration;
                sheet.Cells[row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Total Amount in column I
                sheet.Cells[row, 9].Value = booking.TotalAmount;
                sheet.Cells[row, 9].Style.Numberformat.Format = "$#0";
                sheet.Cells[row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                // Booking Date in column J
                sheet.Cells[row, 10].Value = booking.BookingDate;
                sheet.Cells[row, 10].Style.Numberformat.Format = "yyyy-mm-dd";
                sheet.Cells[row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 10].Style.Fill.BackgroundColor.SetColor(rowColor);
                sheet.Cells[row, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                sheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                isEvenRow = !isEvenRow;
                row++;
            }
            
            // Set column widths explicitly
            sheet.Column(1).Width = 12; // Booking ID
            sheet.Column(2).Width = 25; // Customer Name
            sheet.Column(3).Width = 30; // Email
            sheet.Column(4).Width = 15; // Room Number
            sheet.Column(5).Width = 20; // Room Type
            sheet.Column(6).Width = 15; // Check-in Date
            sheet.Column(7).Width = 15; // Check-out Date
            sheet.Column(8).Width = 12; // Duration
            sheet.Column(9).Width = 15; // Total Amount
            sheet.Column(10).Width = 15; // Booking Date
            
            // Auto-fit columns
            sheet.Cells.AutoFitColumns();
        }
    }
}
