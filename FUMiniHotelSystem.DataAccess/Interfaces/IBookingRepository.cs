using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId);
        Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
