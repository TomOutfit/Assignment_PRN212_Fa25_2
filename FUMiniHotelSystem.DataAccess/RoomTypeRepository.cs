using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniHotelSystem.DataAccess
{
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly FUMiniHotelDbContext _context;

        public RoomTypeRepository()
        {
            _context = DbContextFactory.CreateDbContext();
        }

        public async Task<List<RoomType>> GetAllAsync()
        {
            return await _context.RoomTypes.ToListAsync();
        }

        public async Task<RoomType?> GetByIdAsync(int id)
        {
            return await _context.RoomTypes.FindAsync(id);
        }

        public async Task<RoomType> AddAsync(RoomType entity)
        {
            _context.RoomTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(RoomType entity)
        {
            _context.RoomTypes.Update(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType != null)
            {
                _context.RoomTypes.Remove(roomType);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<RoomType>> GetAllRoomTypesAsync()
        {
            return await GetAllAsync();
        }
    }
}
