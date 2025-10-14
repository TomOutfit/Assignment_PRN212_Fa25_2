using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess.Interfaces
{
    public interface IRoomTypeRepository : IRepository<RoomType>
    {
        Task<List<RoomType>> GetAllRoomTypesAsync();
    }
}
