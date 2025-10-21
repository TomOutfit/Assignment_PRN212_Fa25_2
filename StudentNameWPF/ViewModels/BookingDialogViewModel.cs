using System.Collections.ObjectModel;
using System.Linq;
using FUMiniHotelSystem.BusinessLogic;
using FUMiniHotelSystem.DataAccess;
using FUMiniHotelSystem.Models;
using StudentNameWPF.ViewModels;
using System.Windows.Input;
using System.Windows;
using System.IO;

namespace StudentNameWPF.ViewModels
{
    public class BookingDialogViewModel : BaseViewModel
    {
        private readonly RoomService _roomService;
        private readonly BookingService _bookingService;
        private readonly CustomerService _customerService;
        private ObservableCollection<RoomInformation> _availableRooms = new();
        private RoomInformation? _selectedRoom;
        private decimal _totalAmount;
        private int _customerID;
        private DateTime _checkInDate = DateTime.Today;
        private DateTime _checkOutDate = DateTime.Today.AddDays(1);
        private string _notes = string.Empty;
        private Customer? _currentCustomer;
        private Booking? _currentBooking;

        public BookingDialogViewModel()
        {
            var connectionString = GetConnectionString();
            
            var roomRepository = new RoomRepository(connectionString);
            var roomTypeRepository = new RoomTypeRepository(connectionString);
            var bookingRepository = new BookingRepository(connectionString);
            var customerRepository = new CustomerRepository(connectionString);
            
            _roomService = new RoomService(roomRepository, roomTypeRepository);
            _bookingService = new BookingService(bookingRepository, roomRepository, customerRepository);
            _customerService = new CustomerService(customerRepository);
            
            // Initialize commands
            ConfirmBookingCommand = new RelayCommand(ConfirmBooking, CanConfirmBooking);
            SearchRoomsCommand = new RelayCommand(SearchRooms, CanSearchRooms);
        }

        public ObservableCollection<RoomInformation> AvailableRooms
        {
            get => _availableRooms;
            set => SetProperty(ref _availableRooms, value);
        }

        public RoomInformation? SelectedRoom
        {
            get => _selectedRoom;
            set 
            { 
                SetProperty(ref _selectedRoom, value);
                UpdateTotalAmount();
                ((RelayCommand)ConfirmBookingCommand).RaiseCanExecuteChanged();
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public int CustomerID
        {
            get => _customerID;
            set => SetProperty(ref _customerID, value);
        }

        public DateTime CheckInDate
        {
            get => _checkInDate;
            set 
            { 
                SetProperty(ref _checkInDate, value);
                UpdateTotalAmount();
                ((RelayCommand)SearchRoomsCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ConfirmBookingCommand).RaiseCanExecuteChanged();
            }
        }

        public DateTime CheckOutDate
        {
            get => _checkOutDate;
            set 
            { 
                SetProperty(ref _checkOutDate, value);
                UpdateTotalAmount();
                ((RelayCommand)SearchRoomsCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ConfirmBookingCommand).RaiseCanExecuteChanged();
            }
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public Customer? CurrentCustomer
        {
            get => _currentCustomer;
            set => SetProperty(ref _currentCustomer, value);
        }

        public Booking? CurrentBooking
        {
            get => _currentBooking;
            set => SetProperty(ref _currentBooking, value);
        }

        public int DurationDays
        {
            get
            {
                if (CheckInDate < CheckOutDate)
                {
                    return (CheckOutDate - CheckInDate).Days;
                }
                return 0;
            }
        }

        // Commands
        public ICommand ConfirmBookingCommand { get; }
        public ICommand SearchRoomsCommand { get; }

        public async Task LoadRoomsAsync()
        {
            try
            {
                var rooms = await _roomService.GetActiveRoomsAsync();
                AvailableRooms = new ObservableCollection<RoomInformation>(rooms);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading rooms: {ex.Message}", ex);
            }
        }

        public async Task LoadAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
        {
            try
            {
                var availableRooms = await _bookingService.GetAvailableRoomsAsync(checkIn, checkOut);
                AvailableRooms = new ObservableCollection<RoomInformation>(availableRooms);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading available rooms: {ex.Message}", ex);
            }
        }

        public async void LoadBooking(Booking booking)
        {
            CurrentBooking = booking;
            CustomerID = booking.CustomerID;
            TotalAmount = booking.TotalAmount;
            CheckInDate = booking.CheckInDate;
            CheckOutDate = booking.CheckOutDate;
            Notes = booking.Notes ?? string.Empty;
            
            // Load customer information for edit mode
            try
            {
                await LoadCustomerAsync(booking.CustomerID);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customer for edit: {ex.Message}");
            }
            
            // Load room information for edit mode
            try
            {
                await LoadRoomForEditAsync(booking.RoomID);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading room for edit: {ex.Message}");
            }
        }

        public async Task LoadCustomerAsync(int customerId)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                CurrentCustomer = customer;
                CustomerID = customerId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading customer: {ex.Message}", ex);
            }
        }

        public async Task LoadRoomForEditAsync(int roomId)
        {
            try
            {
                // Load all rooms first
                await LoadRoomsAsync();
                
                // Find and select the specific room
                var room = AvailableRooms.FirstOrDefault(r => r.RoomID == roomId);
                if (room != null)
                {
                    SelectedRoom = room;
                }
                else
                {
                    // If room not found in available rooms, load it separately
                    var connectionString = GetConnectionString();
                    var roomService = new RoomService(new RoomRepository(connectionString), new RoomTypeRepository(connectionString));
                    var specificRoom = await roomService.GetRoomByIdAsync(roomId);
                    if (specificRoom != null)
                    {
                        // Add the room to available rooms if not already there
                        if (!AvailableRooms.Any(r => r.RoomID == roomId))
                        {
                            AvailableRooms.Add(specificRoom);
                        }
                        SelectedRoom = specificRoom;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading room for edit: {ex.Message}", ex);
            }
        }

        private void UpdateTotalAmount()
        {
            if (SelectedRoom != null && CheckInDate < CheckOutDate)
            {
                var duration = (CheckOutDate - CheckInDate).Days;
                TotalAmount = duration * SelectedRoom.RoomPricePerDate;
            }
            else
            {
                TotalAmount = 0;
            }
            
            // Notify property changes for calculated properties
            OnPropertyChanged(nameof(DurationDays));
        }

        private async void SearchRooms()
        {
            try
            {
                await LoadAvailableRoomsAsync(CheckInDate, CheckOutDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching rooms: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSearchRooms()
        {
            return CheckInDate < CheckOutDate && CheckInDate >= DateTime.Today;
        }

        private async void ConfirmBooking()
        {
            try
            {
                if (SelectedRoom == null)
                {
                    MessageBox.Show("Please select a room.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CustomerID <= 0)
                {
                    MessageBox.Show("Customer information is missing.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if this is edit mode (booking already exists)
                if (CurrentBooking != null)
                {
                    // Edit mode - update existing booking
                    CurrentBooking.CustomerID = CustomerID;
                    CurrentBooking.RoomID = SelectedRoom.RoomID;
                    CurrentBooking.CheckInDate = CheckInDate;
                    CurrentBooking.CheckOutDate = CheckOutDate;
                    CurrentBooking.TotalAmount = TotalAmount;
                    CurrentBooking.Notes = Notes;
                    // Keep original BookingStatus and CreatedDate
                    
                    await _bookingService.UpdateBookingAsync(CurrentBooking);
                }
                else
                {
                    // Create mode - create new booking
                    var booking = new Booking
                    {
                        CustomerID = CustomerID,
                        RoomID = SelectedRoom.RoomID,
                        CheckInDate = CheckInDate,
                        CheckOutDate = CheckOutDate,
                        TotalAmount = TotalAmount,
                        Notes = Notes,
                        BookingStatus = 1, // Booked (trạng thái mặc định cho booking mới)
                        CreatedDate = DateTime.Now
                    };

                    await _bookingService.CreateBookingAsync(booking);
                }

                // Show success message
                var customerName = CurrentCustomer?.CustomerFullName ?? "Customer";
                MessageBox.Show($"Booking confirmed successfully!\n\nBooking Details:\n" +
                              $"Customer: {customerName}\n" +
                              $"Room: {SelectedRoom.RoomNumber}\n" +
                              $"Check-in: {CheckInDate:yyyy-MM-dd}\n" +
                              $"Check-out: {CheckOutDate:yyyy-MM-dd}\n" +
                              $"Total Amount: ${TotalAmount:F2}", 
                              "Booking Confirmed", MessageBoxButton.OK, MessageBoxImage.Information);

                // Trigger booking confirmed event
                BookingConfirmed?.Invoke(CurrentBooking ?? new Booking
                {
                    CustomerID = CustomerID,
                    RoomID = SelectedRoom.RoomID,
                    CheckInDate = CheckInDate,
                    CheckOutDate = CheckOutDate,
                    TotalAmount = TotalAmount,
                    Notes = Notes,
                    BookingStatus = 1,
                    CreatedDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error confirming booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanConfirmBooking()
        {
            // For edit mode, we don't need to check CurrentCustomer if CustomerID is set
            var hasValidCustomer = CurrentCustomer != null || CustomerID > 0;
            
            return SelectedRoom != null && 
                   CheckInDate < CheckOutDate && 
                   CheckInDate >= DateTime.Today &&
                   hasValidCustomer;
        }

        // Event for booking confirmation
        public event Action<Booking>? BookingConfirmed;

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
