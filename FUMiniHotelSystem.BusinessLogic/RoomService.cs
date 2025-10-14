using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.BusinessLogic
{
    public class RoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IRoomTypeRepository _roomTypeRepository;

        public RoomService(IRoomRepository roomRepository, IRoomTypeRepository roomTypeRepository)
        {
            _roomRepository = roomRepository;
            _roomTypeRepository = roomTypeRepository;
        }

        public async Task<List<RoomInformation>> GetActiveRoomsAsync()
        {
            return await _roomRepository.GetActiveRoomsAsync();
        }

        public async Task<List<RoomType>> GetAllRoomTypesAsync()
        {
            return await _roomTypeRepository.GetAllRoomTypesAsync();
        }

        public async Task<RoomInformation?> GetRoomByIdAsync(int id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        public async Task<RoomInformation> AddRoomAsync(RoomInformation room)
        {
            // Generate new ID
            var rooms = await _roomRepository.GetAllAsync();
            room.RoomID = rooms.Count > 0 ? rooms.Max(r => r.RoomID) + 1 : 1;
            room.RoomStatus = 1;
            
            return await _roomRepository.AddAsync(room);
        }

        public async Task<bool> UpdateRoomAsync(RoomInformation room)
        {
            return await _roomRepository.UpdateAsync(room);
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room != null)
            {
                room.RoomStatus = 2; // Mark as deleted
                return await _roomRepository.UpdateAsync(room);
            }
            return false;
        }

        public async Task<List<RoomInformation>> SearchRoomsAsync(string searchTerm)
        {
            var rooms = await _roomRepository.GetActiveRoomsAsync();
            return rooms.Where(r => 
                r.RoomNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.RoomDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        public async Task<List<RoomInformation>> GetRoomsByTypeAsync(int roomTypeId)
        {
            return await _roomRepository.GetRoomsByTypeAsync(roomTypeId);
        }
    }
}
