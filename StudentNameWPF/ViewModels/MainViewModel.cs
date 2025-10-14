using FUMiniHotelSystem.BusinessLogic;
using FUMiniHotelSystem.DataAccess;
using FUMiniHotelSystem.Models;
using StudentNameWPF.Services;
using StudentNameWPF.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

namespace StudentNameWPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly CustomerService _customerService;
        private readonly RoomService _roomService;
        private readonly BookingService _bookingService;
        private readonly ReportExportService _reportExportService;
        private readonly ChartExportService _chartExportService;
        private readonly PDFExportService _pdfExportService;
        private readonly ExcelExportService _excelExportService;
        private readonly RealtimeDataService _realtimeDataService;

        private Customer _currentUser = null!;
        private string _currentView = "Dashboard";
        private ObservableCollection<Customer> _customers = new();
        private ObservableCollection<RoomInformation> _rooms = new();
        private ObservableCollection<RoomType> _roomTypes = new();
        private ObservableCollection<BookingDisplayModel> _bookings = new();
        private Customer? _selectedCustomer;
        private RoomInformation? _selectedRoom;

        public Customer CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public ObservableCollection<RoomInformation> Rooms
        {
            get => _rooms;
            set => SetProperty(ref _rooms, value);
        }

        public ObservableCollection<RoomType> RoomTypes
        {
            get => _roomTypes;
            set => SetProperty(ref _roomTypes, value);
        }

        public ObservableCollection<BookingDisplayModel> Bookings
        {
            get => _bookings;
            set => SetProperty(ref _bookings, value);
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public RoomInformation? SelectedRoom
        {
            get => _selectedRoom;
            set => SetProperty(ref _selectedRoom, value);
        }

        // Commands
        public RelayCommand<string> NavigateCommand { get; }
        public RelayCommand AddCustomerCommand { get; }
        public RelayCommand EditCustomerCommand { get; }
        public RelayCommand DeleteCustomerCommand { get; }
        public RelayCommand AddRoomCommand { get; }
        public RelayCommand EditRoomCommand { get; }
        public RelayCommand DeleteRoomCommand { get; }
        public RelayCommand ExportReportCommand { get; }
        public RelayCommand ExportChartCommand { get; }
        public RelayCommand ExportPDFCommand { get; }
        public RelayCommand ExportExcelCommand { get; }
        public RelayCommand LogoutCommand { get; set; } = null!;

        public MainViewModel(Customer currentUser)
        {
            CurrentUser = currentUser;

            // Initialize services using service container
            _customerService = ServiceContainer.GetService<CustomerService>();
            _roomService = ServiceContainer.GetService<RoomService>();
            _bookingService = ServiceContainer.GetService<BookingService>();
            _reportExportService = ServiceContainer.GetService<ReportExportService>();
            _chartExportService = ServiceContainer.GetService<ChartExportService>();
            _pdfExportService = ServiceContainer.GetService<PDFExportService>();
            _excelExportService = ServiceContainer.GetService<ExcelExportService>();
            _realtimeDataService = ServiceContainer.GetService<RealtimeDataService>();

            // Initialize commands
            NavigateCommand = new RelayCommand<string>(Navigate);
            AddCustomerCommand = new RelayCommand(AddCustomer);
            EditCustomerCommand = new RelayCommand(EditCustomer, () => SelectedCustomer != null);
            DeleteCustomerCommand = new RelayCommand(DeleteCustomer, () => SelectedCustomer != null);
            AddRoomCommand = new RelayCommand(AddRoom);
            EditRoomCommand = new RelayCommand(EditRoom, () => SelectedRoom != null);
            DeleteRoomCommand = new RelayCommand(DeleteRoom, () => SelectedRoom != null);
            ExportReportCommand = new RelayCommand(ExportReport);
            ExportChartCommand = new RelayCommand(ExportChart);
            ExportPDFCommand = new RelayCommand(ExportPDF);
            ExportExcelCommand = new RelayCommand(ExportExcel);

            // Load initial data
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (CurrentUser.IsAdmin)
                {
                    var customers = await _customerService.GetActiveCustomersAsync();
                    Customers = new ObservableCollection<Customer>(customers);

                    var rooms = await _roomService.GetActiveRoomsAsync();
                    Rooms = new ObservableCollection<RoomInformation>(rooms);

                    var roomTypes = await _roomService.GetAllRoomTypesAsync();
                    RoomTypes = new ObservableCollection<RoomType>(roomTypes);
                    
                    // Load all bookings for admin
                    var allBookings = await _bookingService.GetAllBookingsAsync();
                    var bookingDisplayModels = allBookings.Select(booking => 
                    {
                        var customer = Customers.FirstOrDefault(c => c.CustomerID == booking.CustomerID);
                        var room = Rooms.FirstOrDefault(r => r.RoomID == booking.RoomID);
                        return BookingDisplayModel.FromBooking(
                            booking, 
                            customer?.CustomerFullName ?? "Unknown Customer",
                            room?.RoomNumber ?? "Unknown Room"
                        );
                    }).ToList();
                    Bookings = new ObservableCollection<BookingDisplayModel>(bookingDisplayModels);
                }
                else
                {
                    // Load only customer's bookings for regular users
                    var bookings = await _bookingService.GetBookingsByCustomerIdAsync(CurrentUser.CustomerID);
                    var bookingDisplayModels = bookings.Select(booking => 
                    {
                        var room = Rooms.FirstOrDefault(r => r.RoomID == booking.RoomID);
                        return BookingDisplayModel.FromBooking(
                            booking, 
                            CurrentUser.CustomerFullName,
                            room?.RoomNumber ?? "Unknown Room"
                        );
                    }).ToList();
                    Bookings = new ObservableCollection<BookingDisplayModel>(bookingDisplayModels);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Navigate(string view)
        {
            CurrentView = view;
        }

        private async void AddCustomer()
        {
            try
            {
                var newCustomer = new Customer
                {
                    CustomerFullName = "New Customer",
                    EmailAddress = "newcustomer@email.com",
                    Telephone = "0123456789",
                    CustomerBirthday = DateTime.Now.AddYears(-25),
                    CustomerStatus = 1,
                    Password = "password123"
                };

                await _customerService.AddCustomerAsync(newCustomer);
                await LoadDataAsync();
                MessageBox.Show("Customer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditCustomer()
        {
            if (SelectedCustomer == null) return;

            try
            {
                SelectedCustomer.CustomerFullName = $"Updated {SelectedCustomer.CustomerFullName}";
                await _customerService.UpdateCustomerAsync(SelectedCustomer);
                await LoadDataAsync();
                MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteCustomer()
        {
            if (SelectedCustomer == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {SelectedCustomer.CustomerFullName}?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _customerService.DeleteCustomerAsync(SelectedCustomer.CustomerID);
                    await LoadDataAsync();
                    MessageBox.Show("Customer deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AddRoom()
        {
            try
            {
                var newRoom = new RoomInformation
                {
                    RoomNumber = $"R{DateTime.Now.Ticks % 1000}",
                    RoomDescription = "New Room Description",
                    RoomMaxCapacity = 2,
                    RoomStatus = 1,
                    RoomPricePerDate = 100.00m,
                    RoomTypeID = 1
                };

                await _roomService.AddRoomAsync(newRoom);
                await LoadDataAsync();
                MessageBox.Show("Room added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditRoom()
        {
            if (SelectedRoom == null) return;

            try
            {
                SelectedRoom.RoomDescription = $"Updated {SelectedRoom.RoomDescription}";
                await _roomService.UpdateRoomAsync(SelectedRoom);
                await LoadDataAsync();
                MessageBox.Show("Room updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteRoom()
        {
            if (SelectedRoom == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete room {SelectedRoom.RoomNumber}?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _roomService.DeleteRoomAsync(SelectedRoom.RoomID);
                    await LoadDataAsync();
                    MessageBox.Show("Room deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExportReport()
        {
            try
            {
                // Ensure data is loaded
                await LoadDataAsync();
                
                var bookingsList = Bookings?.ToList() ?? new List<BookingDisplayModel>();
                var customersList = Customers?.ToList() ?? new List<Customer>();
                var roomsList = Rooms?.ToList() ?? new List<RoomInformation>();
                
                if (bookingsList.Count == 0)
                {
                    MessageBox.Show("No booking data available to export. Please ensure data is loaded.", 
                        "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show SaveFileDialog to let user choose save location
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save HTML Report",
                    Filter = "HTML Files (*.html)|*.html|All Files (*.*)|*.*",
                    DefaultExt = "html",
                    FileName = $"HotelReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var bookingsForExport = bookingsList.Select(b => b.ToBooking()).ToList();
                    var filePath = await _reportExportService.ExportToHTMLAsync(
                        bookingsForExport, 
                        customersList, 
                        roomsList, 
                        "Comprehensive Hotel Report",
                        saveFileDialog.FileName
                    );
                    
                    MessageBox.Show($"📊 HTML Report exported successfully!\n\nFile saved to:\n{filePath}\n\nYou can open this file in your web browser to view the report.", 
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportChart()
        {
            try
            {
                // Ensure data is loaded
                await LoadDataAsync();
                
                var bookingsList = Bookings?.ToList() ?? new List<BookingDisplayModel>();
                var customersList = Customers?.ToList() ?? new List<Customer>();
                var roomsList = Rooms?.ToList() ?? new List<RoomInformation>();
                var roomTypesList = RoomTypes?.ToList() ?? new List<RoomType>();
                
                if (bookingsList.Count == 0)
                {
                    MessageBox.Show("No booking data available to export charts. Please ensure data is loaded.", 
                        "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show SaveFileDialog to let user choose save location for charts
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save Chart Data",
                    Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"HotelCharts_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var bookingsForExport = bookingsList.Select(b => b.ToBooking()).ToList();
                    
                    var revenueChartPath = await _chartExportService.ExportRevenueChartAsync(
                        bookingsForExport, 
                        "HotelRevenueChart"
                    );
                    
                    var customerChartPath = await _chartExportService.ExportCustomerDistributionChartAsync(
                        customersList, 
                        bookingsForExport, 
                        "CustomerDistributionChart"
                    );
                    
                    var roomTypeChartPath = await _chartExportService.ExportRoomTypeChartAsync(
                        roomsList, 
                        roomTypesList, 
                        bookingsForExport, 
                        "RoomTypeChart"
                    );
                    
                    var occupancyChartPath = await _chartExportService.ExportOccupancyChartAsync(
                        bookingsForExport, 
                        roomsList, 
                        "OccupancyChart"
                    );
                    
                    MessageBox.Show($"📈 Charts exported successfully!\n\nFiles saved to:\n• Revenue Chart: {revenueChartPath}\n• Customer Chart: {customerChartPath}\n• Room Type Chart: {roomTypeChartPath}\n• Occupancy Chart: {occupancyChartPath}\n\nThese are JSON files that can be used with Chart.js or other charting libraries.", 
                        "Chart Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting charts: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportPDF()
        {
            try
            {
                // Ensure data is loaded
                await LoadDataAsync();
                
                var bookingsList = Bookings?.ToList() ?? new List<BookingDisplayModel>();
                var customersList = Customers?.ToList() ?? new List<Customer>();
                var roomsList = Rooms?.ToList() ?? new List<RoomInformation>();
                
                if (bookingsList.Count == 0)
                {
                    MessageBox.Show("No booking data available to export PDF. Please ensure data is loaded.", 
                        "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show SaveFileDialog to let user choose save location
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save PDF Report",
                    Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                    DefaultExt = "pdf",
                    FileName = $"HotelReport_PDF_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var bookingsForExport = bookingsList.Select(b => b.ToBooking()).ToList();
                    var filePath = await _pdfExportService.ExportToPDFAsync(
                        bookingsForExport, 
                        customersList, 
                        roomsList, 
                        "Comprehensive Business Report",
                        saveFileDialog.FileName
                    );
                    
                    if (filePath.EndsWith(".pdf"))
                    {
                        MessageBox.Show($"📄 PDF Report exported successfully!\n\nFile saved to:\n{filePath}\n\nThis is a real PDF file that can be opened with any PDF viewer.", 
                            "PDF Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"📄 Report exported successfully!\n\nFile saved to:\n{filePath}\n\nNote: PDF creation failed, so an HTML report was created instead. You can open this file in your browser and use 'Print to PDF' to create a PDF.", 
                            "Report Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting PDF: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportExcel()
        {
            try
            {
                // Ensure data is loaded
                await LoadDataAsync();
                
                var bookingsList = Bookings?.ToList() ?? new List<BookingDisplayModel>();
                var customersList = Customers?.ToList() ?? new List<Customer>();
                var roomsList = Rooms?.ToList() ?? new List<RoomInformation>();
                var roomTypesList = RoomTypes?.ToList() ?? new List<RoomType>();
                
                if (bookingsList.Count == 0)
                {
                    MessageBox.Show("No booking data available to export Excel. Please ensure data is loaded.", 
                        "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show SaveFileDialog to let user choose save location
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save Excel Report",
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"HotelDataAnalysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var bookingsForExport = bookingsList.Select(b => b.ToBooking()).ToList();
                    
                    var filePath = await _excelExportService.ExportToExcelAsync(
                        bookingsForExport, 
                        customersList, 
                        roomsList, 
                        roomTypesList, 
                        "HotelDataAnalysis",
                        saveFileDialog.FileName
                    );
                    
                    var customerAnalysisPath = await _excelExportService.ExportCustomerAnalysisAsync(
                        customersList, 
                        bookingsForExport, 
                        "CustomerAnalysis"
                    );
                    
                    MessageBox.Show($"📋 Excel files exported successfully!\n\nFiles saved to:\n• Main Report: {filePath}\n• Customer Analysis: {customerAnalysisPath}\n\nThese are CSV files that can be opened in Excel or Google Sheets.", 
                        "Excel Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting Excel: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
