using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.BusinessLogic
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly ICustomerRepository _customerRepository;

        public BookingService(IBookingRepository bookingRepository, IRoomRepository roomRepository, ICustomerRepository customerRepository)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _customerRepository = customerRepository;
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            return await _bookingRepository.GetBookingsByCustomerIdAsync(customerId);
        }

        public async Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _bookingRepository.GetBookingsByDateRangeAsync(startDate, endDate);
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _bookingRepository.GetByIdAsync(id);
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            // Validate room availability
            var room = await _roomRepository.GetByIdAsync(booking.RoomID);
            if (room == null || room.RoomStatus != 1)
            {
                throw new InvalidOperationException("Room not available");
            }

            // Validate customer
            var customer = await _customerRepository.GetByIdAsync(booking.CustomerID);
            if (customer == null || customer.CustomerStatus != 1)
            {
                throw new InvalidOperationException("Customer not found or inactive");
            }

            // Generate new ID
            var bookings = await _bookingRepository.GetAllAsync();
            booking.BookingID = bookings.Count > 0 ? bookings.Max(b => b.BookingID) + 1 : 1;
            booking.CreatedDate = DateTime.Now;
            booking.BookingStatus = 1; // Pending

            return await _bookingRepository.AddAsync(booking);
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            return await _bookingRepository.UpdateAsync(booking);
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking != null)
            {
                booking.BookingStatus = 3; // Cancelled
                return await _bookingRepository.UpdateAsync(booking);
            }
            return false;
        }

        public async Task<bool> ConfirmBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking != null)
            {
                booking.BookingStatus = 2; // Confirmed
                return await _bookingRepository.UpdateAsync(booking);
            }
            return false;
        }
    }
}
