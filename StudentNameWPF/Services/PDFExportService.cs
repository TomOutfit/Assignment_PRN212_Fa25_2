using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FUMiniHotelSystem.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace StudentNameWPF.Services
{
    public class PDFExportService
    {
        public Task<string> ExportToPDFAsync(List<Booking> bookings, List<Customer> customers, List<RoomInformation> rooms, string reportType, string? customFilePath = null)
        {
            System.Diagnostics.Debug.WriteLine($"PDFExportService: Starting PDF export");
            System.Diagnostics.Debug.WriteLine($"PDFExportService: Bookings: {bookings?.Count ?? 0}, Customers: {customers?.Count ?? 0}, Rooms: {rooms?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"PDFExportService: Report type: {reportType}");
            System.Diagnostics.Debug.WriteLine($"PDFExportService: Custom file path: {customFilePath}");
            
            var tempFile = "";
            try
            {
                var filePath = customFilePath ?? Path.Combine("Reports", $"HotelReport_{reportType}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Ensure the file path is valid and accessible
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Create PDF document using direct file approach with better error handling
                System.Diagnostics.Debug.WriteLine("PDFExportService: Creating PDF directly to file...");
                
                // Create a temporary file in the same directory to avoid permission issues
                tempFile = Path.Combine(directory ?? Path.GetTempPath(), $"temp_{Guid.NewGuid()}.pdf");
                System.Diagnostics.Debug.WriteLine($"PDFExportService: Using temp file: {tempFile}");
                
                // Try the simplest approach - direct file creation
                System.Diagnostics.Debug.WriteLine("PDFExportService: Attempting direct PDF creation...");
                
                using var writer = new PdfWriter(tempFile);
                System.Diagnostics.Debug.WriteLine("PDFExportService: Created PdfWriter");
                
                using var pdf = new PdfDocument(writer);
                System.Diagnostics.Debug.WriteLine("PDFExportService: Created PdfDocument");
                
                using var document = new Document(pdf);
                System.Diagnostics.Debug.WriteLine("PDFExportService: Created Document");

            // Set up fonts
            System.Diagnostics.Debug.WriteLine("PDFExportService: Creating fonts...");
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var smallFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            System.Diagnostics.Debug.WriteLine("PDFExportService: Fonts created successfully");
            
            // Header
            System.Diagnostics.Debug.WriteLine("PDFExportService: Creating header...");
            var header = new Paragraph("🏨 FUMiniHotel Management System")
                .SetFont(headerFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(header);
            System.Diagnostics.Debug.WriteLine("PDFExportService: Header added");

            var subtitle = new Paragraph("Comprehensive Business Report")
                .SetFont(titleFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(subtitle);
            
            // Report Info
            document.Add(new Paragraph($"Report Type: {reportType}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetMarginBottom(5));
            document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetMarginBottom(5));
            if (bookings?.Any() == true)
            {
                document.Add(new Paragraph($"Report Period: {bookings.Min(b => b.CheckInDate):yyyy-MM-dd} to {bookings.Max(b => b.CheckInDate):yyyy-MM-dd}")
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetMarginBottom(20));
            }
            
            // Executive Summary
            var summaryTitle = new Paragraph("📊 Executive Summary")
                .SetFont(titleFont)
                .SetFontSize(16)
                .SetMarginBottom(10);
            document.Add(summaryTitle);

            var summaryTable = new Table(2).UseAllAvailableWidth();
            summaryTable.AddCell(new Cell().Add(new Paragraph("Total Revenue").SetFont(titleFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            summaryTable.AddCell(new Cell().Add(new Paragraph($"${bookings?.Sum(b => b.TotalAmount) ?? 0:0}").SetFont(normalFont)));

            summaryTable.AddCell(new Cell().Add(new Paragraph("Total Bookings").SetFont(titleFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            summaryTable.AddCell(new Cell().Add(new Paragraph($"{bookings?.Count ?? 0:N0}").SetFont(normalFont)));

            summaryTable.AddCell(new Cell().Add(new Paragraph("Average Booking Value").SetFont(titleFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            summaryTable.AddCell(new Cell().Add(new Paragraph($"${bookings?.Average(b => b.TotalAmount) ?? 0:0}").SetFont(normalFont)));

            summaryTable.AddCell(new Cell().Add(new Paragraph("Total Customers").SetFont(titleFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            summaryTable.AddCell(new Cell().Add(new Paragraph($"{customers?.Count ?? 0:N0}").SetFont(normalFont)));

            summaryTable.AddCell(new Cell().Add(new Paragraph("Total Rooms").SetFont(titleFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            summaryTable.AddCell(new Cell().Add(new Paragraph($"{rooms?.Count ?? 0:N0}").SetFont(normalFont)));

            summaryTable.AddCell(new Cell().Add(new Paragraph("Average Occupancy Rate").SetFont(titleFont)).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            var occupancyRate = CalculateOccupancyRate(bookings ?? new List<Booking>(), rooms ?? new List<RoomInformation>());
            summaryTable.AddCell(new Cell().Add(new Paragraph($"{occupancyRate:F1}%").SetFont(normalFont)));

            document.Add(summaryTable);
            
            // Revenue Analysis
            document.Add(new Paragraph("💰 Revenue Analysis")
                .SetFont(titleFont)
                .SetFontSize(16)
                .SetMarginTop(20)
                .SetMarginBottom(10));
            
            var monthlyRevenue = (bookings ?? new List<Booking>())
                .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { 
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                    Revenue = g.Sum(b => b.TotalAmount),
                    Bookings = g.Count()
                })
                .ToList();
            
            var revenueTable = new Table(4).UseAllAvailableWidth();
            revenueTable.AddHeaderCell(new Cell().Add(new Paragraph("Month").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));
            revenueTable.AddHeaderCell(new Cell().Add(new Paragraph("Revenue").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));
            revenueTable.AddHeaderCell(new Cell().Add(new Paragraph("Bookings").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));
            revenueTable.AddHeaderCell(new Cell().Add(new Paragraph("Avg per Booking").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));

            foreach (var month in monthlyRevenue)
            {
                var avgPerBooking = month.Revenue / month.Bookings;
                revenueTable.AddCell(new Cell().Add(new Paragraph(month.Month).SetFont(normalFont)));
                revenueTable.AddCell(new Cell().Add(new Paragraph($"${month.Revenue:0}").SetFont(normalFont)));
                revenueTable.AddCell(new Cell().Add(new Paragraph(month.Bookings.ToString()).SetFont(normalFont)));
                revenueTable.AddCell(new Cell().Add(new Paragraph($"${avgPerBooking:0}").SetFont(normalFont)));
            }
            
            document.Add(revenueTable);
            
            // Top Customers
            document.Add(new Paragraph("🏆 Top 10 Customers by Revenue")
                .SetFont(titleFont)
                .SetFontSize(16)
                .SetMarginTop(20)
                .SetMarginBottom(10));

            var topCustomers = (bookings ?? new List<Booking>())
                .GroupBy(b => b.CustomerID)
                .Select(g => new { 
                    CustomerID = g.Key, 
                    CustomerName = (customers ?? new List<Customer>()).FirstOrDefault(c => c.CustomerID == g.Key)?.CustomerFullName ?? "Unknown",
                    TotalSpent = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10);
            
            var customerTable = new Table(4).UseAllAvailableWidth();
            customerTable.AddHeaderCell(new Cell().Add(new Paragraph("Customer").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));
            customerTable.AddHeaderCell(new Cell().Add(new Paragraph("Total Spent").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));
            customerTable.AddHeaderCell(new Cell().Add(new Paragraph("Bookings").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));
            customerTable.AddHeaderCell(new Cell().Add(new Paragraph("Avg per Booking").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));

            foreach (var customer in topCustomers)
            {
                var avgPerBooking = customer.TotalSpent / customer.BookingCount;
                customerTable.AddCell(new Cell().Add(new Paragraph(customer.CustomerName).SetFont(normalFont)));
                customerTable.AddCell(new Cell().Add(new Paragraph($"${customer.TotalSpent:0}").SetFont(normalFont)));
                customerTable.AddCell(new Cell().Add(new Paragraph(customer.BookingCount.ToString()).SetFont(normalFont)));
                customerTable.AddCell(new Cell().Add(new Paragraph($"${avgPerBooking:0}").SetFont(normalFont)));
            }

            document.Add(customerTable);
            
            // Top Rooms
            document.Add(new Paragraph("🏨 Top 10 Rooms by Revenue")
                .SetFont(titleFont)
                .SetFontSize(16)
                .SetMarginTop(20)
                .SetMarginBottom(10));

            var topRooms = (bookings ?? new List<Booking>())
                .GroupBy(b => b.RoomID)
                .Select(g => new {
                    RoomID = g.Key,
                    RoomNumber = (rooms ?? new List<RoomInformation>()).FirstOrDefault(r => r.RoomID == g.Key)?.RoomNumber ?? "Unknown",
                    Revenue = g.Sum(b => b.TotalAmount),
                    BookingCount = g.Count()
                })
                .OrderByDescending(r => r.Revenue)
                .Take(10);
            
            var roomTable = new Table(4).UseAllAvailableWidth();
            roomTable.AddHeaderCell(new Cell().Add(new Paragraph("Room").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));
            roomTable.AddHeaderCell(new Cell().Add(new Paragraph("Revenue").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));
            roomTable.AddHeaderCell(new Cell().Add(new Paragraph("Bookings").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));
            roomTable.AddHeaderCell(new Cell().Add(new Paragraph("Avg per Booking").SetFont(titleFont)).SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE));

            foreach (var room in topRooms)
            {
                var avgPerBooking = room.Revenue / room.BookingCount;
                roomTable.AddCell(new Cell().Add(new Paragraph(room.RoomNumber).SetFont(normalFont)));
                roomTable.AddCell(new Cell().Add(new Paragraph($"${room.Revenue:0}").SetFont(normalFont)));
                roomTable.AddCell(new Cell().Add(new Paragraph(room.BookingCount.ToString()).SetFont(normalFont)));
                roomTable.AddCell(new Cell().Add(new Paragraph($"${avgPerBooking:0}").SetFont(normalFont)));
            }

            document.Add(roomTable);

            // Footer
            document.Add(new Paragraph("This report was generated by FUMiniHotel Management System")
                .SetFont(smallFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(30));
            document.Add(new Paragraph($"Report generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                .SetFont(smallFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER));

                // Move the temporary file to the final location
                System.Diagnostics.Debug.WriteLine($"PDFExportService: Moving temp file to final location: {filePath}");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempFile, filePath);
                System.Diagnostics.Debug.WriteLine("PDFExportService: PDF export completed successfully");
                return Task.FromResult(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PDFExportService: PDF creation failed - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PDFExportService: Stack trace - {ex.StackTrace}");
                
                // Clean up temp file if it exists
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch { }

                // If PDF creation fails, fall back to HTML export
                var fallbackPath = customFilePath?.Replace(".pdf", ".html") ?? Path.Combine("Reports", $"HotelReport_{reportType}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                var directory = Path.GetDirectoryName(fallbackPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create HTML content as fallback
                var html = CreateHTMLContent(bookings ?? new List<Booking>(), customers ?? new List<Customer>(), rooms ?? new List<RoomInformation>(), reportType);
                File.WriteAllText(fallbackPath, html);
                
                System.Diagnostics.Debug.WriteLine($"PDFExportService: Fallback to HTML export - {fallbackPath}");
                return Task.FromResult(fallbackPath);
            }
        }

        private string CreateHTMLContent(List<Booking> bookings, List<Customer> customers, List<RoomInformation> rooms, string reportType)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<title>FUMiniHotel System Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("@page { size: A4; margin: 20mm; }");
            html.AppendLine("body { font-family: Arial, sans-serif; font-size: 12px; line-height: 1.4; }");
            html.AppendLine("h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; margin-bottom: 20px; }");
            html.AppendLine("h2 { color: #34495e; margin-top: 25px; margin-bottom: 15px; font-size: 16px; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 15px 0; font-size: 11px; }");
            html.AppendLine("th, td { border: 1px solid #bdc3c7; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #3498db; color: white; font-weight: bold; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f8f9fa; }");
            html.AppendLine(".summary { background-color: #ecf0f1; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #3498db; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            
            // Header
            html.AppendLine("<h1>🏨 FUMiniHotel Management System</h1>");
            html.AppendLine($"<p><strong>Report Type:</strong> {reportType}</p>");
            html.AppendLine($"<p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            
            // Executive Summary
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>📊 Executive Summary</h2>");
            html.AppendLine($"<p><strong>Total Revenue:</strong> ${bookings.Sum(b => b.TotalAmount):0}</p>");
            html.AppendLine($"<p><strong>Total Bookings:</strong> {bookings.Count:N0}</p>");
            html.AppendLine($"<p><strong>Average Booking Value:</strong> ${bookings.Average(b => b.TotalAmount):0}</p>");
            html.AppendLine($"<p><strong>Total Customers:</strong> {customers.Count:N0}</p>");
            html.AppendLine($"<p><strong>Total Rooms:</strong> {rooms.Count:N0}</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("</body></html>");
            
            return html.ToString();
        }
        
        private string GetSeason(DateTime date)
        {
            int month = date.Month;
            if (month >= 3 && month <= 5) return "Spring";
            if (month >= 6 && month <= 8) return "Summer";
            if (month >= 9 && month <= 11) return "Autumn";
            return "Winter";
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
    }
}
