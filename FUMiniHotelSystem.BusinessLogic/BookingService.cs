using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            // Basic validation
            if (booking.CheckInDate >= booking.CheckOutDate)
            {
                throw new InvalidOperationException("Ngày check-out phải sau ngày check-in");
            }

            if (booking.CheckInDate < DateTime.Today)
            {
                throw new InvalidOperationException("Ngày check-in không thể trong quá khứ");
            }

            // Check room availability
            var room = await _roomRepository.GetByIdAsync(booking.RoomID);
            if (room == null || room.RoomStatus != 1)
            {
                throw new InvalidOperationException("Phòng không khả dụng");
            }

            // Check for conflicts (đơn giản hóa - chỉ kiểm tra booking còn tồn tại)
            var allBookings = await _bookingRepository.GetAllAsync();
            var hasConflict = allBookings.Any(b => 
                b.RoomID == booking.RoomID && 
                b.BookingStatus == 1 && // Chỉ kiểm tra booking đã booked
                ((b.CheckInDate < booking.CheckOutDate && b.CheckOutDate > booking.CheckInDate))
            );

            if (hasConflict)
            {
                throw new InvalidOperationException("Phòng đã được đặt trong khoảng thời gian này");
            }

            // Generate new ID and set defaults
            var bookings = await _bookingRepository.GetAllAsync();
            booking.BookingID = bookings.Count > 0 ? bookings.Max(b => b.BookingID) + 1 : 1;
            booking.CreatedDate = DateTime.Now;
            booking.BookingStatus = 1; // Booked (booking được tạo là đã booked)

            return await _bookingRepository.AddAsync(booking);
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            // Basic validation
            if (booking.CheckInDate >= booking.CheckOutDate)
            {
                throw new InvalidOperationException("Ngày check-out phải sau ngày check-in");
            }

            if (booking.CheckInDate < DateTime.Today)
            {
                throw new InvalidOperationException("Ngày check-in không thể trong quá khứ");
            }

            // Check room availability
            var room = await _roomRepository.GetByIdAsync(booking.RoomID);
            if (room == null || room.RoomStatus != 1)
            {
                throw new InvalidOperationException("Phòng không khả dụng");
            }

            // Check for conflicts (excluding current booking) - sửa lỗi edit
            var allBookings = await _bookingRepository.GetAllAsync();
            var hasConflict = allBookings.Any(b => 
                b.RoomID == booking.RoomID && 
                b.BookingID != booking.BookingID && // Exclude current booking
                b.BookingStatus == 1 && // Chỉ kiểm tra booking đã booked
                ((b.CheckInDate < booking.CheckOutDate && b.CheckOutDate > booking.CheckInDate))
            );

            if (hasConflict)
            {
                throw new InvalidOperationException("Phòng đã được đặt trong khoảng thời gian này");
            }

            return await _bookingRepository.UpdateAsync(booking);
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            // Set booking status to 0 (Not Booked) thay vì xóa
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking != null)
            {
                booking.BookingStatus = 0; // Not Booked
                return await _bookingRepository.UpdateAsync(booking);
            }
            return false;
        }

        // Loại bỏ các method không cần thiết cho trạng thái đơn giản
        // Chỉ giữ lại Create, Update, Delete và Get methods

        public async Task<List<RoomInformation>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
        {
            // Get all active rooms using LINQ
            var allRooms = await _roomRepository.GetAllAsync();
            var activeRooms = allRooms.Where(r => r.RoomStatus == 1).ToList();
            
            // Get all bookings in the date range using LINQ (đơn giản hóa)
            var allBookings = await _bookingRepository.GetAllAsync();
            var conflictingBookings = allBookings.Where(b => 
                b.BookingStatus == 1 && // Chỉ kiểm tra booking đã booked
                b.CheckInDate < checkOut && 
                b.CheckOutDate > checkIn
            ).ToList();
            
            // Use LINQ to find available rooms
            var availableRooms = activeRooms.Where(room => 
                !conflictingBookings.Any(booking => booking.RoomID == room.RoomID)
            ).ToList();
            
            return availableRooms;
        }

        public async Task<List<RoomInformation>> GetAvailableRoomsByTypeAsync(DateTime checkIn, DateTime checkOut, int roomTypeId)
        {
            // Get all active rooms of specific type using LINQ
            var allRooms = await _roomRepository.GetAllAsync();
            var activeRoomsOfType = allRooms.Where(r => 
                r.RoomStatus == 1 && 
                r.RoomTypeID == roomTypeId
            ).ToList();
            
            // Get conflicting bookings using LINQ (đơn giản hóa)
            var allBookings = await _bookingRepository.GetAllAsync();
            var conflictingBookings = allBookings.Where(b => 
                b.BookingStatus == 1 && // Chỉ kiểm tra booking đã booked
                b.CheckInDate < checkOut && 
                b.CheckOutDate > checkIn
            ).ToList();
            
            // Use LINQ to find available rooms of specific type
            var availableRooms = activeRoomsOfType.Where(room => 
                !conflictingBookings.Any(booking => booking.RoomID == room.RoomID)
            ).ToList();
            
            return availableRooms;
        }

        public async Task<List<RoomInformation>> GetAvailableRoomsByCapacityAsync(DateTime checkIn, DateTime checkOut, int minCapacity)
        {
            // Get all active rooms with minimum capacity using LINQ
            var allRooms = await _roomRepository.GetAllAsync();
            var activeRoomsWithCapacity = allRooms.Where(r => 
                r.RoomStatus == 1 && 
                r.RoomMaxCapacity >= minCapacity
            ).ToList();
            
            // Get conflicting bookings using LINQ (đơn giản hóa)
            var allBookings = await _bookingRepository.GetAllAsync();
            var conflictingBookings = allBookings.Where(b => 
                b.BookingStatus == 1 && // Chỉ kiểm tra booking đã booked
                b.CheckInDate < checkOut && 
                b.CheckOutDate > checkIn
            ).ToList();
            
            // Use LINQ to find available rooms with required capacity
            var availableRooms = activeRoomsWithCapacity.Where(room => 
                !conflictingBookings.Any(booking => booking.RoomID == room.RoomID)
            ).ToList();
            
            return availableRooms;
        }
    }
}
