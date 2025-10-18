using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniHotelSystem.DataAccess
{
    public class RoomRepository : IRoomRepository
    {
        private readonly FUMiniHotelDbContext _context;

        public RoomRepository()
        {
            _context = DbContextFactory.CreateDbContext();
        }

        public async Task<List<RoomInformation>> GetAllAsync()
        {
            return await _context.RoomInformation
                .Include(r => r.RoomType)
                .ToListAsync();
        }

        public async Task<RoomInformation?> GetByIdAsync(int id)
        {
            return await _context.RoomInformation
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomID == id);
        }

        public async Task<RoomInformation> AddAsync(RoomInformation entity)
        {
            _context.RoomInformation.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(RoomInformation entity)
        {
            _context.RoomInformation.Update(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var room = await _context.RoomInformation.FindAsync(id);
            if (room != null)
            {
                _context.RoomInformation.Remove(room);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<RoomInformation>> GetActiveRoomsAsync()
        {
            return await _context.RoomInformation
                .Include(r => r.RoomType)
                .Where(r => r.RoomStatus == 1)
                .ToListAsync();
        }

        public async Task<List<RoomInformation>> GetRoomsByTypeAsync(int roomTypeId)
        {
            return await _context.RoomInformation
                .Include(r => r.RoomType)
                .Where(r => r.RoomTypeID == roomTypeId && r.RoomStatus == 1)
                .ToListAsync();
        }
    }
}
