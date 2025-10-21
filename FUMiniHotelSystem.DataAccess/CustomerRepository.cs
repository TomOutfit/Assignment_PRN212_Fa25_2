using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FUMiniHotelSystem.DataAccess
{
    public class CustomerRepository : SqlRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(string connectionString) : base(connectionString) { }

        public override async Task<List<Customer>> GetAllAsync()
        {
            var customers = new List<Customer>();
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("SELECT * FROM Customers WHERE CustomerStatus = 1", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                customers.Add(MapCustomerFromReader(reader));
            }
            
            return customers;
        }

        public override async Task<Customer?> GetByIdAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("SELECT * FROM Customers WHERE CustomerID = @id AND CustomerStatus = 1", connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapCustomerFromReader(reader);
            }
            
            return null;
        }

        public override async Task<Customer> AddAsync(Customer entity)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                INSERT INTO Customers (CustomerFullName, Telephone, EmailAddress, CustomerBirthday, CustomerStatus, Password)
                VALUES (@fullName, @telephone, @email, @birthday, @status, @password);
                SELECT SCOPE_IDENTITY();", connection);
            
            command.Parameters.AddWithValue("@fullName", entity.CustomerFullName);
            command.Parameters.AddWithValue("@telephone", entity.Telephone);
            command.Parameters.AddWithValue("@email", entity.EmailAddress);
            command.Parameters.AddWithValue("@birthday", entity.CustomerBirthday);
            command.Parameters.AddWithValue("@status", entity.CustomerStatus);
            command.Parameters.AddWithValue("@password", entity.Password);
            
            var id = await command.ExecuteScalarAsync();
            entity.CustomerID = Convert.ToInt32(id);
            return entity;
        }

        public override async Task<bool> UpdateAsync(Customer entity)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(@"
                UPDATE Customers 
                SET CustomerFullName = @fullName, Telephone = @telephone, EmailAddress = @email, 
                    CustomerBirthday = @birthday, CustomerStatus = @status, Password = @password
                WHERE CustomerID = @id", connection);
            
            command.Parameters.AddWithValue("@id", entity.CustomerID);
            command.Parameters.AddWithValue("@fullName", entity.CustomerFullName);
            command.Parameters.AddWithValue("@telephone", entity.Telephone);
            command.Parameters.AddWithValue("@email", entity.EmailAddress);
            command.Parameters.AddWithValue("@birthday", entity.CustomerBirthday);
            command.Parameters.AddWithValue("@status", entity.CustomerStatus);
            command.Parameters.AddWithValue("@password", entity.Password);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("UPDATE Customers SET CustomerStatus = 2 WHERE CustomerID = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand("SELECT * FROM Customers WHERE EmailAddress = @email AND CustomerStatus = 1", connection);
            command.Parameters.AddWithValue("@email", email);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapCustomerFromReader(reader);
            }
            
            return null;
        }

        public async Task<List<Customer>> GetActiveCustomersAsync()
        {
            return await GetAllAsync();
        }

        private static Customer MapCustomerFromReader(SqlDataReader reader)
        {
            return new Customer
            {
                CustomerID = reader.GetInt32("CustomerID"),
                CustomerFullName = reader.GetString("CustomerFullName"),
                Telephone = reader.GetString("Telephone"),
                EmailAddress = reader.GetString("EmailAddress"),
                CustomerBirthday = reader.GetDateTime("CustomerBirthday"),
                CustomerStatus = reader.GetInt32("CustomerStatus"),
                Password = reader.GetString("Password")
            };
        }
    }
}
