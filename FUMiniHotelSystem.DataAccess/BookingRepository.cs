using Microsoft.EntityFrameworkCore;
using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess
{
    public class BookingRepository : SqlRepository<Booking>, IBookingRepository
    {
        public BookingRepository(HotelDbContext context) : base(context) { }

        public async Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            return await _dbSet.Where(b => b.CustomerID == customerId).ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet.Where(b => b.CheckInDate >= startDate && b.CheckOutDate <= endDate).ToListAsync();
        }
    }
}
