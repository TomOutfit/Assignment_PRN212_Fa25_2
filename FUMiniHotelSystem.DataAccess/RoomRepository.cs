using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FUMiniHotelSystem.DataAccess
{
    public class RoomRepository : SqlRepository<RoomInformation>, IRoomRepository
    {
        public RoomRepository(string connectionString) : base(connectionString) { }

        public override async Task<List<RoomInformation>> GetAllAsync()
        {
            var rooms = new List<RoomInformation>();
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT r.*, rt.RoomTypeName, rt.TypeDescription, rt.TypeNote
                FROM RoomInformation r
                LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE r.RoomStatus = 1", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                rooms.Add(MapRoomFromReader(reader));
            }
            
            return rooms;
        }

        public override async Task<RoomInformation?> GetByIdAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT r.*, rt.RoomTypeName, rt.TypeDescription, rt.TypeNote
                FROM RoomInformation r
                LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE r.RoomID = @id AND r.RoomStatus = 1", connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapRoomFromReader(reader);
            }
            
            return null;
        }

        public override async Task<RoomInformation> AddAsync(RoomInformation entity)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                INSERT INTO RoomInformation (RoomNumber, RoomDescription, RoomMaxCapacity, RoomStatus, RoomPricePerDate, RoomTypeID)
                VALUES (@roomNumber, @roomDescription, @roomMaxCapacity, @roomStatus, @roomPricePerDate, @roomTypeId);
                SELECT SCOPE_IDENTITY();", connection);
            
            command.Parameters.AddWithValue("@roomNumber", entity.RoomNumber);
            command.Parameters.AddWithValue("@roomDescription", entity.RoomDescription ?? string.Empty);
            command.Parameters.AddWithValue("@roomMaxCapacity", entity.RoomMaxCapacity);
            command.Parameters.AddWithValue("@roomStatus", entity.RoomStatus);
            command.Parameters.AddWithValue("@roomPricePerDate", entity.RoomPricePerDate);
            command.Parameters.AddWithValue("@roomTypeId", entity.RoomTypeID);
            
            var id = await command.ExecuteScalarAsync();
            entity.RoomID = Convert.ToInt32(id);
            return entity;
        }

        public override async Task<bool> UpdateAsync(RoomInformation entity)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                UPDATE RoomInformation 
                SET RoomNumber = @roomNumber, RoomDescription = @roomDescription, 
                    RoomMaxCapacity = @roomMaxCapacity, RoomStatus = @roomStatus, 
                    RoomPricePerDate = @roomPricePerDate, RoomTypeID = @roomTypeId
                WHERE RoomID = @id", connection);
            
            command.Parameters.AddWithValue("@id", entity.RoomID);
            command.Parameters.AddWithValue("@roomNumber", entity.RoomNumber);
            command.Parameters.AddWithValue("@roomDescription", entity.RoomDescription ?? string.Empty);
            command.Parameters.AddWithValue("@roomMaxCapacity", entity.RoomMaxCapacity);
            command.Parameters.AddWithValue("@roomStatus", entity.RoomStatus);
            command.Parameters.AddWithValue("@roomPricePerDate", entity.RoomPricePerDate);
            command.Parameters.AddWithValue("@roomTypeId", entity.RoomTypeID);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("UPDATE RoomInformation SET RoomStatus = 2 WHERE RoomID = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<RoomInformation>> GetActiveRoomsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<List<RoomInformation>> GetRoomsByTypeAsync(int roomTypeId)
        {
            var rooms = new List<RoomInformation>();
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT r.*, rt.RoomTypeName, rt.TypeDescription, rt.TypeNote
                FROM RoomInformation r
                LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE r.RoomTypeID = @roomTypeId AND r.RoomStatus = 1", connection);
            command.Parameters.AddWithValue("@roomTypeId", roomTypeId);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                rooms.Add(MapRoomFromReader(reader));
            }
            
            return rooms;
        }

        private static RoomInformation MapRoomFromReader(SqlDataReader reader)
        {
            var room = new RoomInformation
            {
                RoomID = reader.GetInt32("RoomID"),
                RoomNumber = reader.GetString("RoomNumber"),
                RoomDescription = reader.IsDBNull("RoomDescription") ? string.Empty : reader.GetString("RoomDescription"),
                RoomMaxCapacity = reader.GetInt32("RoomMaxCapacity"),
                RoomStatus = reader.GetInt32("RoomStatus"),
                RoomPricePerDate = reader.GetDecimal("RoomPricePerDate"),
                RoomTypeID = reader.GetInt32("RoomTypeID")
            };

            // Map room type if available
            if (!reader.IsDBNull("RoomTypeName"))
            {
                room.RoomType = new RoomType
                {
                    RoomTypeID = room.RoomTypeID,
                    RoomTypeName = reader.GetString("RoomTypeName"),
                    TypeDescription = reader.IsDBNull("TypeDescription") ? string.Empty : reader.GetString("TypeDescription"),
                    TypeNote = reader.IsDBNull("TypeNote") ? string.Empty : reader.GetString("TypeNote")
                };
            }

            return room;
        }
    }
}
