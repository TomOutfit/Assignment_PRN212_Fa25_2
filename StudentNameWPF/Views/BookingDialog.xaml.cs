using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FUMiniHotelSystem.Models;
using StudentNameWPF.ViewModels;

namespace StudentNameWPF.Views
{
    public partial class BookingDialog : Window
    {
        private BookingDialogViewModel _viewModel;
        private Booking? _originalBooking;

        public BookingDialog()
        {
            InitializeComponent();
            _viewModel = new BookingDialogViewModel();
            DataContext = _viewModel;
            
            // Subscribe to booking confirmed event
            _viewModel.BookingConfirmed += OnBookingConfirmed;
            
            InitializeDialog();
        }

        public BookingDialog(Booking booking)
        {
            InitializeComponent();
            _viewModel = new BookingDialogViewModel();
            DataContext = _viewModel;
            
            // Subscribe to booking confirmed event
            _viewModel.BookingConfirmed += OnBookingConfirmed;
            
            _originalBooking = booking;
            _viewModel.LoadBooking(booking);
            DialogTitle.Text = "Edit Booking";
            SaveButton.Content = "Update Booking";
            InitializeDialog();
        }

        public BookingDialog(int customerId)
        {
            InitializeComponent();
            _viewModel = new BookingDialogViewModel();
            DataContext = _viewModel;
            
            // Subscribe to booking confirmed event
            _viewModel.BookingConfirmed += OnBookingConfirmed;
            
            // Load customer information
            _ = LoadCustomerAsync(customerId);
            
            InitializeDialog();
        }

        private async void InitializeDialog()
        {
            // Set minimum date to today
            CheckInDatePicker.DisplayDateStart = DateTime.Today;
            CheckOutDatePicker.DisplayDateStart = DateTime.Today.AddDays(1);
            
            // Bind event handlers
            CheckInDatePicker.SelectedDateChanged += (sender, e) => OnDateChanged();
            CheckOutDatePicker.SelectedDateChanged += (sender, e) => OnDateChanged();
            
            // Load rooms
            await LoadRooms();
            
            // Set default dates
            if (_originalBooking == null)
            {
                CheckInDatePicker.SelectedDate = DateTime.Today;
                CheckOutDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            }
            else
            {
                CheckInDatePicker.SelectedDate = _originalBooking.CheckInDate;
                CheckOutDatePicker.SelectedDate = _originalBooking.CheckOutDate;
                NotesTextBox.Text = _originalBooking.Notes;
            }
            
            UpdatePriceCalculation();
            
            // Trigger command update for edit mode
            if (_originalBooking != null)
            {
                ((RelayCommand)_viewModel.ConfirmBookingCommand).RaiseCanExecuteChanged();
            }
        }

        private async Task LoadCustomerAsync(int customerId)
        {
            try
            {
                await _viewModel.LoadCustomerAsync(customerId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnBookingConfirmed(Booking booking)
        {
            DialogResult = true;
            Close();
        }

        private async Task LoadRooms()
        {
            try
            {
                await _viewModel.LoadRoomsAsync();
                
                // Set selected room after loading if editing
                if (_originalBooking != null)
                {
                    // Wait a bit for the ComboBox to be ready
                    await Task.Delay(100);
                    var room = _viewModel.AvailableRooms.FirstOrDefault(r => r.RoomID == _originalBooking.RoomID);
                    if (room != null)
                    {
                        _viewModel.SelectedRoom = room;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rooms: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewRoomDetails_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedRoom != null)
            {
                ShowRoomDetails(_viewModel.SelectedRoom);
            }
            else
            {
                MessageBox.Show("Please select a room first.", "No Room Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowRoomDetails(RoomInformation room)
        {
            RoomDetailsPanel.Visibility = Visibility.Visible;
            RoomDetailsText.Text = $"Room Number: {room.RoomNumber}\n" +
                                 $"Description: {room.RoomDescription}\n" +
                                 $"Max Capacity: {room.RoomMaxCapacity} guests\n" +
                                 $"Price per Day: ${room.RoomPricePerDate:F2}\n" +
                                 $"Status: {(room.RoomStatus == 1 ? "Available" : "Unavailable")}";
        }

        private async void OnDateChanged()
        {
            // Update ViewModel dates
            if (CheckInDatePicker.SelectedDate.HasValue)
            {
                _viewModel.CheckInDate = CheckInDatePicker.SelectedDate.Value;
            }
            if (CheckOutDatePicker.SelectedDate.HasValue)
            {
                _viewModel.CheckOutDate = CheckOutDatePicker.SelectedDate.Value;
            }
            
            UpdatePriceCalculation();
            ValidateDates();
            
            // Trigger command update
            ((RelayCommand)_viewModel.ConfirmBookingCommand).RaiseCanExecuteChanged();
            
            // Update available rooms when dates change
            if (CheckInDatePicker.SelectedDate.HasValue && CheckOutDatePicker.SelectedDate.HasValue)
            {
                await UpdateAvailableRooms();
            }
        }

        private void UpdatePriceCalculation()
        {
            // The price calculation is now handled by the ViewModel
            // This method is kept for backward compatibility but the actual calculation
            // is done in the ViewModel's UpdateTotalAmount method
        }

        private void ValidateDates()
        {
            if (CheckInDatePicker.SelectedDate.HasValue && CheckOutDatePicker.SelectedDate.HasValue)
            {
                var checkIn = CheckInDatePicker.SelectedDate.Value;
                var checkOut = CheckOutDatePicker.SelectedDate.Value;
                
                if (checkOut <= checkIn)
                {
                    ValidationMessage.Text = "Check-out date must be after check-in date.";
                    SaveButton.IsEnabled = false;
                }
                else if (checkIn < DateTime.Today)
                {
                    ValidationMessage.Text = "Check-in date cannot be in the past.";
                    SaveButton.IsEnabled = false;
                }
                else
                {
                    ValidationMessage.Text = "";
                    SaveButton.IsEnabled = true;
                }
            }
            else
            {
                ValidationMessage.Text = "Please select both check-in and check-out dates.";
                SaveButton.IsEnabled = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateBooking())
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateBooking()
        {
            if (RoomComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a room.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!CheckInDatePicker.SelectedDate.HasValue || !CheckOutDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select both check-in and check-out dates.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var checkIn = CheckInDatePicker.SelectedDate.Value;
            var checkOut = CheckOutDatePicker.SelectedDate.Value;

            if (checkOut <= checkIn)
            {
                MessageBox.Show("Check-out date must be after check-in date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (checkIn < DateTime.Today)
            {
                MessageBox.Show("Check-in date cannot be in the past.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        public Booking? GetBooking()
        {
            if (DialogResult == true && ValidateBooking())
            {
                var booking = new Booking
                {
                    CustomerID = _viewModel.CustomerID,
                    RoomID = (int)RoomComboBox.SelectedValue,
                    CheckInDate = CheckInDatePicker.SelectedDate ?? DateTime.Today,
                    CheckOutDate = CheckOutDatePicker.SelectedDate ?? DateTime.Today.AddDays(1),
                    TotalAmount = _viewModel.TotalAmount,
                    Notes = NotesTextBox.Text,
                    BookingStatus = 1, // Pending
                    CreatedDate = DateTime.Now
                };

                if (_originalBooking != null)
                {
                    booking.BookingID = _originalBooking.BookingID;
                    booking.CreatedDate = _originalBooking.CreatedDate;
                    booking.BookingStatus = _originalBooking.BookingStatus;
                }

                return booking;
            }

            return null;
        }

        private async Task UpdateAvailableRooms()
        {
            try
            {
                if (CheckInDatePicker.SelectedDate.HasValue && CheckOutDatePicker.SelectedDate.HasValue)
                {
                    var checkIn = CheckInDatePicker.SelectedDate.Value;
                    var checkOut = CheckOutDatePicker.SelectedDate.Value;
                    
                    if (checkOut > checkIn)
                    {
                        await _viewModel.LoadAvailableRoomsAsync(checkIn, checkOut);
                        
                        // Show message if no rooms available
                        if (_viewModel.AvailableRooms.Count == 0)
                        {
                            ValidationMessage.Text = "No rooms available for the selected dates. Please choose different dates.";
                        }
                        else
                        {
                            ValidationMessage.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ValidationMessage.Text = $"Error checking room availability: {ex.Message}";
            }
        }
    }
}
