using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FUMiniHotelSystem.DataAccess
{
    public class RoomTypeRepository : SqlRepository<RoomType>, IRoomTypeRepository
    {
        public RoomTypeRepository(string connectionString) : base(connectionString) { }

        public override async Task<List<RoomType>> GetAllAsync()
        {
            var roomTypes = new List<RoomType>();
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("SELECT * FROM RoomTypes", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                roomTypes.Add(MapRoomTypeFromReader(reader));
            }
            
            return roomTypes;
        }

        public override async Task<RoomType?> GetByIdAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("SELECT * FROM RoomTypes WHERE RoomTypeID = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapRoomTypeFromReader(reader);
            }
            
            return null;
        }

        public override async Task<RoomType> AddAsync(RoomType entity)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                INSERT INTO RoomTypes (RoomTypeName, TypeDescription, TypeNote)
                VALUES (@roomTypeName, @typeDescription, @typeNote);
                SELECT SCOPE_IDENTITY();", connection);
            
            command.Parameters.AddWithValue("@roomTypeName", entity.RoomTypeName);
            command.Parameters.AddWithValue("@typeDescription", entity.TypeDescription ?? string.Empty);
            command.Parameters.AddWithValue("@typeNote", entity.TypeNote ?? string.Empty);
            
            var id = await command.ExecuteScalarAsync();
            entity.RoomTypeID = Convert.ToInt32(id);
            return entity;
        }

        public override async Task<bool> UpdateAsync(RoomType entity)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                UPDATE RoomTypes 
                SET RoomTypeName = @roomTypeName, TypeDescription = @typeDescription, TypeNote = @typeNote
                WHERE RoomTypeID = @id", connection);
            
            command.Parameters.AddWithValue("@id", entity.RoomTypeID);
            command.Parameters.AddWithValue("@roomTypeName", entity.RoomTypeName);
            command.Parameters.AddWithValue("@typeDescription", entity.TypeDescription ?? string.Empty);
            command.Parameters.AddWithValue("@typeNote", entity.TypeNote ?? string.Empty);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("DELETE FROM RoomTypes WHERE RoomTypeID = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<RoomType>> GetAllRoomTypesAsync()
        {
            return await GetAllAsync();
        }

        private static RoomType MapRoomTypeFromReader(SqlDataReader reader)
        {
            return new RoomType
            {
                RoomTypeID = reader.GetInt32("RoomTypeID"),
                RoomTypeName = reader.GetString("RoomTypeName"),
                TypeDescription = reader.IsDBNull("TypeDescription") ? string.Empty : reader.GetString("TypeDescription"),
                TypeNote = reader.IsDBNull("TypeNote") ? string.Empty : reader.GetString("TypeNote")
            };
        }
    }
}
