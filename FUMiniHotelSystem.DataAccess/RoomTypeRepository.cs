using Microsoft.EntityFrameworkCore;
using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess
{
    public class RoomTypeRepository : SqlRepository<RoomType>, IRoomTypeRepository
    {
        public RoomTypeRepository(HotelDbContext context) : base(context) { }

        public async Task<List<RoomType>> GetAllRoomTypesAsync()
        {
            return await GetAllAsync();
        }
    }
}
