using FUMiniHotelSystem.BusinessLogic;
using FUMiniHotelSystem.DataAccess;
using FUMiniHotelSystem.Models;
using StudentNameWPF.Services;
using StudentNameWPF.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using Microsoft.VisualBasic;
using System.IO;

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
        private string _customerSearchText = string.Empty;
        private string _roomSearchText = string.Empty;
        private ObservableCollection<Customer> _filteredCustomers = new();
        private ObservableCollection<RoomInformation> _filteredRooms = new();
        private BookingDisplayModel? _selectedBooking;

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

        public string CustomerSearchText
        {
            get => _customerSearchText;
            set
            {
                SetProperty(ref _customerSearchText, value);
                FilterCustomers();
            }
        }

        public string RoomSearchText
        {
            get => _roomSearchText;
            set
            {
                SetProperty(ref _roomSearchText, value);
                FilterRooms();
            }
        }

        public ObservableCollection<Customer> FilteredCustomers
        {
            get => _filteredCustomers;
            set => SetProperty(ref _filteredCustomers, value);
        }

        public ObservableCollection<RoomInformation> FilteredRooms
        {
            get => _filteredRooms;
            set => SetProperty(ref _filteredRooms, value);
        }

        public BookingDisplayModel? SelectedBooking
        {
            get => _selectedBooking;
            set => SetProperty(ref _selectedBooking, value);
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
        public RelayCommand SearchCustomersCommand { get; }
        public RelayCommand ClearCustomerSearchCommand { get; }
        public RelayCommand SearchRoomsCommand { get; }
        public RelayCommand ClearRoomSearchCommand { get; }
        public RelayCommand EditProfileCommand { get; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand AddBookingCommand { get; }
        public RelayCommand EditBookingCommand { get; }
        public RelayCommand CancelBookingCommand { get; }
        public RelayCommand LogoutCommand { get; set; } = null!;

        public MainViewModel(Customer currentUser)
        {
            CurrentUser = currentUser;

            // Get connection string from appsettings.json
            var connectionString = GetConnectionString();

            // Initialize services with SQL repositories
            var customerRepository = new CustomerRepository(connectionString);
            var roomRepository = new RoomRepository(connectionString);
            var roomTypeRepository = new RoomTypeRepository(connectionString);
            var bookingRepository = new BookingRepository(connectionString);

            _customerService = new CustomerService(customerRepository);
            _roomService = new RoomService(roomRepository, roomTypeRepository);
            _bookingService = new BookingService(bookingRepository, roomRepository, customerRepository);
            _reportExportService = new ReportExportService();
            _chartExportService = new ChartExportService();
            _pdfExportService = new PDFExportService();
            _excelExportService = new ExcelExportService();
            _realtimeDataService = new RealtimeDataService();

            // Set initial view based on user role
            if (currentUser.IsAdmin)
            {
                CurrentView = "Dashboard";
            }
            else
            {
                // For regular customers, start with Bookings view to show their booking history
                CurrentView = "Bookings";
            }

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
            SearchCustomersCommand = new RelayCommand(SearchCustomers);
            ClearCustomerSearchCommand = new RelayCommand(ClearCustomerSearch);
            SearchRoomsCommand = new RelayCommand(SearchRooms);
            ClearRoomSearchCommand = new RelayCommand(ClearRoomSearch);
            EditProfileCommand = new RelayCommand(EditProfile);
            ChangePasswordCommand = new RelayCommand(ChangePassword);
            AddBookingCommand = new RelayCommand(AddBooking);
            EditBookingCommand = new RelayCommand(EditBooking, () => SelectedBooking != null);
            CancelBookingCommand = new RelayCommand(CancelBooking, () => SelectedBooking != null);

            // Start realtime data service
            _realtimeDataService.StartRealtimeUpdates();
            
            // Load initial data
            _ = LoadDataAsync();
            
            // Debug booking search
            _ = Task.Run(async () => 
            {
                await Task.Delay(2000); // Wait for data to load
                DebugBookingSearch();
            });
        }

        private async Task LoadDataAsync()
        {
            try
            {
                if (CurrentUser.IsAdmin)
                {
                    var customers = await _customerService.GetActiveCustomersAsync();
                    Customers = new ObservableCollection<Customer>(customers);
                    FilteredCustomers = new ObservableCollection<Customer>(customers);

                    var rooms = await _roomService.GetActiveRoomsAsync();
                    Rooms = new ObservableCollection<RoomInformation>(rooms);
                    FilteredRooms = new ObservableCollection<RoomInformation>(rooms);

                    var roomTypes = await _roomService.GetAllRoomTypesAsync();
                    RoomTypes = new ObservableCollection<RoomType>(roomTypes);
                    
                    // Load all bookings for admin (chỉ hiển thị booking đã Booked)
                    var allBookings = await _bookingService.GetAllBookingsAsync();
                    System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Total bookings from service: {allBookings.Count}");
                    
                    var bookingDisplayModels = allBookings
                        // Tất cả booking hiển thị đều đã được booked
                        .Select(booking => 
                        {
                            var customer = Customers.FirstOrDefault(c => c.CustomerID == booking.CustomerID);
                            var room = Rooms.FirstOrDefault(r => r.RoomID == booking.RoomID);
                            return BookingDisplayModel.FromBooking(
                                booking, 
                                customer?.CustomerFullName ?? "Unknown Customer",
                                room?.RoomNumber ?? "Unknown Room"
                            );
                        }).ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Booked bookings count: {bookingDisplayModels.Count}");
                    Bookings = new ObservableCollection<BookingDisplayModel>(bookingDisplayModels);
                }
                else
                {
                    // Load rooms data for regular users to display room names in bookings
                    var rooms = await _roomService.GetActiveRoomsAsync();
                    Rooms = new ObservableCollection<RoomInformation>(rooms);
                    FilteredRooms = new ObservableCollection<RoomInformation>(rooms);
                    
                    // Load only customer's bookings for regular users (chỉ hiển thị booking đã Booked)
                    var bookings = await _bookingService.GetBookingsByCustomerIdAsync(CurrentUser.CustomerID);
                    System.Diagnostics.Debug.WriteLine($"LoadDataAsync (Regular User): Total bookings for customer {CurrentUser.CustomerID}: {bookings.Count}");
                    
                    var bookingDisplayModels = bookings
                        // Tất cả booking hiển thị đều đã được booked
                        .Select(booking => 
                        {
                            System.Diagnostics.Debug.WriteLine($"LoadDataAsync (Regular User): Processing booking ID: {booking.BookingID}, Status: {booking.BookingStatus}, CustomerID: {booking.CustomerID}");
                            var room = Rooms.FirstOrDefault(r => r.RoomID == booking.RoomID);
                            return BookingDisplayModel.FromBooking(
                                booking, 
                                CurrentUser.CustomerFullName,
                                room?.RoomNumber ?? "Unknown Room"
                            );
                        }).ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"LoadDataAsync (Regular User): Booked bookings count: {bookingDisplayModels.Count}");
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
                var dialog = new Views.CustomerDialog();
                
                // Don't set Owner to avoid the "Cannot set Owner property to itself" error
                // The dialog will be modal by default
                
                if (dialog.ShowDialog() == true)
                {
                    var newCustomer = dialog.GetCustomer();
                    if (newCustomer != null)
                    {
                       await _customerService.AddCustomerAsync(newCustomer);
                       await LoadDataAsync();
                       FilterCustomers(); // Refresh filtered customers
                       await _realtimeDataService.ForceUpdateAsync();
                        MessageBox.Show("Customer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditCustomer()
        {
            if (SelectedCustomer == null) 
            {
                System.Diagnostics.Debug.WriteLine("EditCustomer: No customer selected");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"EditCustomer: Opening dialog for customer {SelectedCustomer.CustomerFullName}");
                var dialog = new Views.CustomerDialog(SelectedCustomer);
                
                // Don't set Owner to avoid the "Cannot set Owner property to itself" error
                // The dialog will be modal by default
                
                System.Diagnostics.Debug.WriteLine("EditCustomer: Showing dialog...");
                var result = dialog.ShowDialog();
                System.Diagnostics.Debug.WriteLine($"EditCustomer: Dialog result: {result}");
                
                if (result == true)
                {
                    var updatedCustomer = dialog.GetCustomer();
                    if (updatedCustomer != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"EditCustomer: Updating customer {updatedCustomer.CustomerFullName}");
                        await _customerService.UpdateCustomerAsync(updatedCustomer);
                        await LoadDataAsync();
                        FilterCustomers(); // Refresh filtered customers
                        await _realtimeDataService.ForceUpdateAsync();
                        MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("EditCustomer: Updated customer is null");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("EditCustomer: Dialog was cancelled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EditCustomer: Exception - {ex.Message}");
                MessageBox.Show($"Error updating customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteCustomer()
        {
            if (SelectedCustomer == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {SelectedCustomer.CustomerFullName}?\n\nThis action cannot be undone.", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _customerService.DeleteCustomerAsync(SelectedCustomer.CustomerID);
                    await LoadDataAsync();
                    FilterCustomers(); // Refresh filtered customers
                    await _realtimeDataService.ForceUpdateAsync();
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
                var dialog = new Views.RoomDialog();
                
                // Don't set Owner to avoid the "Cannot set Owner property to itself" error
                // The dialog will be modal by default
                
                if (dialog.ShowDialog() == true)
                {
                    var newRoom = dialog.GetRoom();
                    if (newRoom != null)
                    {
                        await _roomService.AddRoomAsync(newRoom);
                        await LoadDataAsync();
                        FilterRooms(); // Refresh filtered rooms
                        await _realtimeDataService.ForceUpdateAsync();
                        MessageBox.Show("Room added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
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
                var dialog = new Views.RoomDialog(SelectedRoom);
                
                // Don't set Owner to avoid the "Cannot set Owner property to itself" error
                // The dialog will be modal by default
                
                if (dialog.ShowDialog() == true)
                {
                    var updatedRoom = dialog.GetRoom();
                    if (updatedRoom != null)
                    {
                        await _roomService.UpdateRoomAsync(updatedRoom);
                        await LoadDataAsync();
                        FilterRooms(); // Refresh filtered rooms
                        await _realtimeDataService.ForceUpdateAsync();
                        MessageBox.Show("Room updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteRoom()
        {
            if (SelectedRoom == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete room {SelectedRoom.RoomNumber}?\n\nThis action cannot be undone.", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _roomService.DeleteRoomAsync(SelectedRoom.RoomID);
                    await LoadDataAsync();
                    FilterRooms(); // Refresh filtered rooms
                    await _realtimeDataService.ForceUpdateAsync();
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
                    System.Diagnostics.Debug.WriteLine($"ExportPDF: Starting PDF export to {saveFileDialog.FileName}");
                    System.Diagnostics.Debug.WriteLine($"ExportPDF: Bookings count: {bookingsList.Count}");
                    System.Diagnostics.Debug.WriteLine($"ExportPDF: Customers count: {customersList.Count}");
                    System.Diagnostics.Debug.WriteLine($"ExportPDF: Rooms count: {roomsList.Count}");
                    
                    var bookingsForExport = bookingsList.Select(b => b.ToBooking()).ToList();
                    System.Diagnostics.Debug.WriteLine($"ExportPDF: Converted bookings count: {bookingsForExport.Count}");
                    
                    var filePath = await _pdfExportService.ExportToPDFAsync(
                        bookingsForExport, 
                        customersList, 
                        roomsList, 
                        "Comprehensive Business Report",
                        saveFileDialog.FileName
                    );
                    
                    System.Diagnostics.Debug.WriteLine($"ExportPDF: Export completed, file path: {filePath}");
                    
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
                    
                    MessageBox.Show($"📋 Excel file exported successfully!\n\nFile saved to: {filePath}\n\nThis is a CSV file that can be opened in Excel or Google Sheets.", 
                        "Excel Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting Excel: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Search Methods

        private void FilterCustomers()
        {
            if (string.IsNullOrWhiteSpace(CustomerSearchText))
            {
                FilteredCustomers = new ObservableCollection<Customer>(Customers);
            }
            else
            {
                var searchTerm = CustomerSearchText.ToLower();
                var filtered = Customers.Where(c => 
                    c.CustomerFullName.ToLower().Contains(searchTerm) ||
                    c.EmailAddress.ToLower().Contains(searchTerm) ||
                    c.Telephone.Contains(searchTerm)
                ).ToList();
                
                FilteredCustomers = new ObservableCollection<Customer>(filtered);
            }
        }

        private void FilterRooms()
        {
            if (string.IsNullOrWhiteSpace(RoomSearchText))
            {
                FilteredRooms = new ObservableCollection<RoomInformation>(Rooms);
            }
            else
            {
                var searchTerm = RoomSearchText.ToLower();
                var filtered = Rooms.Where(r => 
                    r.RoomNumber.ToLower().Contains(searchTerm) ||
                    r.RoomDescription.ToLower().Contains(searchTerm) ||
                    r.RoomMaxCapacity.ToString().Contains(searchTerm) ||
                    r.RoomPricePerDate.ToString().Contains(searchTerm)
                ).ToList();
                
                FilteredRooms = new ObservableCollection<RoomInformation>(filtered);
            }
        }

        private void SearchCustomers()
        {
            FilterCustomers();
            MessageBox.Show($"Found {FilteredCustomers.Count} customer(s) matching '{CustomerSearchText}'", 
                "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearCustomerSearch()
        {
            CustomerSearchText = string.Empty;
            FilteredCustomers = new ObservableCollection<Customer>(Customers);
            MessageBox.Show("Customer search cleared. Showing all customers.", 
                "Search Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchRooms()
        {
            FilterRooms();
            MessageBox.Show($"Found {FilteredRooms.Count} room(s) matching '{RoomSearchText}'", 
                "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearRoomSearch()
        {
            RoomSearchText = string.Empty;
            FilteredRooms = new ObservableCollection<RoomInformation>(Rooms);
            MessageBox.Show("Room search cleared. Showing all rooms.", 
                "Search Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Profile Management

        private async void EditProfile()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EditProfile: Opening dialog for customer {CurrentUser.CustomerFullName}");
                var dialog = new Views.CustomerDialog(CurrentUser);
                
                System.Diagnostics.Debug.WriteLine("EditProfile: Showing dialog...");
                var result = dialog.ShowDialog();
                System.Diagnostics.Debug.WriteLine($"EditProfile: Dialog result: {result}");
                
                if (result == true)
                {
                    var updatedCustomer = dialog.GetCustomer();
                    if (updatedCustomer != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"EditProfile: Updating customer {updatedCustomer.CustomerFullName}");
                        await _customerService.UpdateCustomerAsync(updatedCustomer);
                        
                        // Update current user
                        CurrentUser = updatedCustomer;
                        
                        // Reload data to refresh the UI
                        await LoadDataAsync();
                        await _realtimeDataService.ForceUpdateAsync();
                        
                        MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("EditProfile: Updated customer is null");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("EditProfile: Dialog was cancelled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EditProfile: Exception - {ex.Message}");
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePassword()
        {
            try
            {
                // Simple password change dialog
                var newPassword = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter new password:", 
                    "Change Password", 
                    "", 
                    -1, 
                    -1);
                
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    // Update password in current user
                    CurrentUser.Password = newPassword;
                    
                    // Update in database
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _customerService.UpdateCustomerAsync(CurrentUser);
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show("Password changed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"Error changing password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                        }
                    });
                }
                else
                {
                    MessageBox.Show("Password cannot be empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChangePassword: Exception - {ex.Message}");
                MessageBox.Show($"Error changing password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Booking Management

        private async void AddBooking()
        {
            try
            {
                // Create booking dialog with current customer ID
                var dialog = new Views.BookingDialog(CurrentUser.CustomerID);
                
                if (dialog.ShowDialog() == true)
                {
                    // Booking is automatically created and saved by the ViewModel
                    // Just reload data and show success message
                    await LoadDataAsync();
                    await _realtimeDataService.ForceUpdateAsync();
                    
                    // Navigate to booking history view after successful booking
                    CurrentView = "Bookings";
                    
                    MessageBox.Show("Đặt phòng thành công!\n\nBạn có thể xem lịch sử đặt phòng trong tab 'Bookings'.", 
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditBooking()
        {
            if (SelectedBooking == null) 
            {
                MessageBox.Show("Vui lòng chọn đặt phòng để chỉnh sửa.", "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"EditBooking: Looking for booking with ID: {SelectedBooking.BookingID}");
                
                // Get the original booking
                var originalBooking = await _bookingService.GetBookingByIdAsync(SelectedBooking.BookingID);
                if (originalBooking == null)
                {
                    System.Diagnostics.Debug.WriteLine($"EditBooking: No booking found with ID: {SelectedBooking.BookingID}");
                    MessageBox.Show($"Không tìm thấy đặt phòng với ID: {SelectedBooking.BookingID}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"EditBooking: Found booking - CustomerID: {originalBooking.CustomerID}, CurrentUser: {CurrentUser.CustomerID}");

                // Admin không thể edit booking
                if (CurrentUser.IsAdmin)
                {
                    MessageBox.Show("Admin chỉ có thể xem thông tin booking, không thể chỉnh sửa.", "Không có quyền", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Check if booking belongs to current user
                if (originalBooking.CustomerID != CurrentUser.CustomerID)
                {
                    MessageBox.Show("Bạn chỉ có thể chỉnh sửa đặt phòng của chính mình.", "Không có quyền", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new Views.BookingDialog(originalBooking);
                
                if (dialog.ShowDialog() == true)
                {
                    var updatedBooking = dialog.GetBooking();
                    if (updatedBooking != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"EditBooking: Updating booking ID: {updatedBooking.BookingID}");
                        System.Diagnostics.Debug.WriteLine($"EditBooking: Original booking ID: {originalBooking.BookingID}");
                        System.Diagnostics.Debug.WriteLine($"EditBooking: Updated booking ID: {updatedBooking.BookingID}");
                        
                        await _bookingService.UpdateBookingAsync(updatedBooking);
                        System.Diagnostics.Debug.WriteLine($"EditBooking: Booking updated successfully, refreshing data...");
                        
                        // Clear current selection to avoid issues
                        SelectedBooking = null;
                        
                        await LoadDataAsync();
                        await _realtimeDataService.ForceUpdateAsync();
                        
                        System.Diagnostics.Debug.WriteLine($"EditBooking: Data refreshed, current bookings count: {Bookings.Count}");
                        MessageBox.Show("Chỉnh sửa đặt phòng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chỉnh sửa đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CancelBooking()
        {
            if (SelectedBooking == null) 
            {
                MessageBox.Show("Vui lòng chọn đặt phòng để hủy.", "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"CancelBooking: Looking for booking with ID: {SelectedBooking.BookingID}");
                
                // Get the original booking
                var originalBooking = await _bookingService.GetBookingByIdAsync(SelectedBooking.BookingID);
                if (originalBooking == null)
                {
                    System.Diagnostics.Debug.WriteLine($"CancelBooking: No booking found with ID: {SelectedBooking.BookingID}");
                    MessageBox.Show($"Không tìm thấy đặt phòng với ID: {SelectedBooking.BookingID}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"CancelBooking: Found booking - CustomerID: {originalBooking.CustomerID}, CurrentUser: {CurrentUser.CustomerID}");

                // Admin không thể cancel booking
                if (CurrentUser.IsAdmin)
                {
                    MessageBox.Show("Admin chỉ có thể xem thông tin booking, không thể hủy.", "Không có quyền", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Check if booking belongs to current user
                if (originalBooking.CustomerID != CurrentUser.CustomerID)
                {
                    MessageBox.Show("Bạn chỉ có thể hủy đặt phòng của chính mình.", "Không có quyền", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc chắn muốn hủy đặt phòng #{SelectedBooking.BookingID}?\n\nHành động này không thể hoàn tác.", 
                    "Xác nhận hủy", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await _bookingService.CancelBookingAsync(SelectedBooking.BookingID);
                    await LoadDataAsync();
                    await _realtimeDataService.ForceUpdateAsync();
                    MessageBox.Show("Hủy đặt phòng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hủy đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Debug Methods

        private async void DebugBookingSearch()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== DEBUG BOOKING SEARCH ===");
                
                // Get all bookings
                var allBookings = await _bookingService.GetAllBookingsAsync();
                System.Diagnostics.Debug.WriteLine($"Total bookings in database: {allBookings.Count}");
                
                foreach (var booking in allBookings)
                {
                    System.Diagnostics.Debug.WriteLine($"Booking ID: {booking.BookingID}, Customer ID: {booking.CustomerID}, Room ID: {booking.RoomID}");
                }
                
                // Test GetBookingByIdAsync for each booking
                foreach (var booking in allBookings)
                {
                    var foundBooking = await _bookingService.GetBookingByIdAsync(booking.BookingID);
                    if (foundBooking != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✓ Found booking {booking.BookingID}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ Could not find booking {booking.BookingID}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("=== END DEBUG ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Debug error: {ex.Message}");
            }
        }

        #endregion

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
}
