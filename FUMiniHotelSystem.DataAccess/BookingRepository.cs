using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FUMiniHotelSystem.DataAccess
{
    public class BookingRepository : SqlRepository<Booking>, IBookingRepository
    {
        public BookingRepository(string connectionString) : base(connectionString) { }

        public override async Task<List<Booking>> GetAllAsync()
        {
            var bookings = new List<Booking>();
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT b.*, c.CustomerFullName, c.EmailAddress, c.Telephone,
                       r.RoomNumber, r.RoomDescription, r.RoomPricePerDate,
                       rt.RoomTypeName, rt.TypeDescription
                FROM Bookings b
                LEFT JOIN Customers c ON b.CustomerID = c.CustomerID
                LEFT JOIN RoomInformation r ON b.RoomID = r.RoomID
                LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE b.BookingStatus != 3", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                bookings.Add(MapBookingFromReader(reader));
            }
            
            return bookings;
        }

        public override async Task<Booking?> GetByIdAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT b.*, c.CustomerFullName, c.EmailAddress, c.Telephone,
                       r.RoomNumber, r.RoomDescription, r.RoomPricePerDate,
                       rt.RoomTypeName, rt.TypeDescription
                FROM Bookings b
                LEFT JOIN Customers c ON b.CustomerID = c.CustomerID
                LEFT JOIN RoomInformation r ON b.RoomID = r.RoomID
                LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE b.BookingID = @id AND b.BookingStatus != 3", connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapBookingFromReader(reader);
            }
            
            return null;
        }

        public override async Task<Booking> AddAsync(Booking entity)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                INSERT INTO Bookings (CustomerID, RoomID, CheckInDate, CheckOutDate, TotalAmount, BookingStatus, CreatedDate, Notes)
                VALUES (@customerId, @roomId, @checkInDate, @checkOutDate, @totalAmount, @bookingStatus, @createdDate, @notes);
                SELECT SCOPE_IDENTITY();", connection);
            
            command.Parameters.AddWithValue("@customerId", entity.CustomerID);
            command.Parameters.AddWithValue("@roomId", entity.RoomID);
            command.Parameters.AddWithValue("@checkInDate", entity.CheckInDate);
            command.Parameters.AddWithValue("@checkOutDate", entity.CheckOutDate);
            command.Parameters.AddWithValue("@totalAmount", entity.TotalAmount);
            command.Parameters.AddWithValue("@bookingStatus", entity.BookingStatus);
            command.Parameters.AddWithValue("@createdDate", entity.CreatedDate);
            command.Parameters.AddWithValue("@notes", entity.Notes ?? string.Empty);
            
            var id = await command.ExecuteScalarAsync();
            entity.BookingID = Convert.ToInt32(id);
            return entity;
        }

        public override async Task<bool> UpdateAsync(Booking entity)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                UPDATE Bookings 
                SET CustomerID = @customerId, RoomID = @roomId, CheckInDate = @checkInDate, 
                    CheckOutDate = @checkOutDate, TotalAmount = @totalAmount, 
                    BookingStatus = @bookingStatus, Notes = @notes
                WHERE BookingID = @id", connection);
            
            command.Parameters.AddWithValue("@id", entity.BookingID);
            command.Parameters.AddWithValue("@customerId", entity.CustomerID);
            command.Parameters.AddWithValue("@roomId", entity.RoomID);
            command.Parameters.AddWithValue("@checkInDate", entity.CheckInDate);
            command.Parameters.AddWithValue("@checkOutDate", entity.CheckOutDate);
            command.Parameters.AddWithValue("@totalAmount", entity.TotalAmount);
            command.Parameters.AddWithValue("@bookingStatus", entity.BookingStatus);
            command.Parameters.AddWithValue("@notes", entity.Notes ?? string.Empty);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("UPDATE Bookings SET BookingStatus = 3 WHERE BookingID = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            var bookings = new List<Booking>();
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT b.*, c.CustomerFullName, c.EmailAddress, c.Telephone,
                       r.RoomNumber, r.RoomDescription, r.RoomPricePerDate,
                       rt.RoomTypeName, rt.TypeDescription
                FROM Bookings b
                LEFT JOIN Customers c ON b.CustomerID = c.CustomerID
                LEFT JOIN RoomInformation r ON b.RoomID = r.RoomID
                LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE b.CustomerID = @customerId AND b.BookingStatus != 3", connection);
            command.Parameters.AddWithValue("@customerId", customerId);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                bookings.Add(MapBookingFromReader(reader));
            }
            
            return bookings;
        }

        public async Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = new List<Booking>();
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT b.*, c.CustomerFullName, c.EmailAddress, c.Telephone,
                       r.RoomNumber, r.RoomDescription, r.RoomPricePerDate,
                       rt.RoomTypeName, rt.TypeDescription
                FROM Bookings b
                LEFT JOIN Customers c ON b.CustomerID = c.CustomerID
                LEFT JOIN RoomInformation r ON b.RoomID = r.RoomID
                LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                WHERE b.CheckInDate >= @startDate AND b.CheckOutDate <= @endDate AND b.BookingStatus != 3", connection);
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                bookings.Add(MapBookingFromReader(reader));
            }
            
            return bookings;
        }

        private static Booking MapBookingFromReader(SqlDataReader reader)
        {
            var booking = new Booking
            {
                BookingID = reader.GetInt32("BookingID"),
                CustomerID = reader.GetInt32("CustomerID"),
                RoomID = reader.GetInt32("RoomID"),
                CheckInDate = reader.GetDateTime("CheckInDate"),
                CheckOutDate = reader.GetDateTime("CheckOutDate"),
                TotalAmount = reader.GetDecimal("TotalAmount"),
                BookingStatus = reader.GetInt32("BookingStatus"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes")
            };

            // Map related entities if available
            if (!reader.IsDBNull("CustomerFullName"))
            {
                booking.Customer = new Customer
                {
                    CustomerID = booking.CustomerID,
                    CustomerFullName = reader.GetString("CustomerFullName"),
                    EmailAddress = reader.GetString("EmailAddress"),
                    Telephone = reader.GetString("Telephone")
                };
            }

            if (!reader.IsDBNull("RoomNumber"))
            {
                booking.Room = new RoomInformation
                {
                    RoomID = booking.RoomID,
                    RoomNumber = reader.GetString("RoomNumber"),
                    RoomDescription = reader.GetString("RoomDescription"),
                    RoomPricePerDate = reader.GetDecimal("RoomPricePerDate"),
                    RoomType = new RoomType
                    {
                        RoomTypeName = reader.GetString("RoomTypeName"),
                        TypeDescription = reader.GetString("TypeDescription")
                    }
                };
            }

            return booking;
        }
    }
}
