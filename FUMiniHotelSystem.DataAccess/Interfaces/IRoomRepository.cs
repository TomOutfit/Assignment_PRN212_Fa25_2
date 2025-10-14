using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess.Interfaces
{
    public interface IRoomRepository : IRepository<RoomInformation>
    {
        Task<List<RoomInformation>> GetActiveRoomsAsync();
        Task<List<RoomInformation>> GetRoomsByTypeAsync(int roomTypeId);
    }
}
