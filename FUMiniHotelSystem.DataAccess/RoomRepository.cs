using Microsoft.EntityFrameworkCore;
using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess
{
    public class RoomRepository : SqlRepository<RoomInformation>, IRoomRepository
    {
        public RoomRepository(HotelDbContext context) : base(context) { }

        public async Task<List<RoomInformation>> GetActiveRoomsAsync()
        {
            return await _dbSet.Where(r => r.RoomStatus == 1).ToListAsync();
        }

        public async Task<List<RoomInformation>> GetRoomsByTypeAsync(int roomTypeId)
        {
            return await _dbSet.Where(r => r.RoomTypeID == roomTypeId && r.RoomStatus == 1).ToListAsync();
        }
    }
}
