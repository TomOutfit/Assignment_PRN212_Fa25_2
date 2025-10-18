using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniHotelSystem.DataAccess
{
    public class BookingRepository : IBookingRepository
    {
        private readonly FUMiniHotelDbContext _context;

        public BookingRepository()
        {
            _context = DbContextFactory.CreateDbContext();
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.RoomInformation)
                .ThenInclude(r => r.RoomType)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.RoomInformation)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(b => b.BookingID == id);
        }

        public async Task<Booking> AddAsync(Booking entity)
        {
            _context.Bookings.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(Booking entity)
        {
            _context.Bookings.Update(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.RoomInformation)
                .ThenInclude(r => r.RoomType)
                .Where(b => b.CustomerID == customerId)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.RoomInformation)
                .ThenInclude(r => r.RoomType)
                .Where(b => b.CheckInDate >= startDate && b.CheckOutDate <= endDate)
                .ToListAsync();
        }
    }
}
