using Microsoft.Data.SqlClient;
using System.Data;
using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess
{
    public class SqlRepository<T> : IRepository<T> where T : class
    {
        protected readonly string _connectionString;

        public SqlRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected async Task<SqlConnection> GetConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            // This will be implemented by derived classes
            return new List<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            // This will be implemented by derived classes
            return null;
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            // This will be implemented by derived classes
            return entity;
        }

        public virtual async Task<bool> UpdateAsync(T entity)
        {
            // This will be implemented by derived classes
            return false;
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            // This will be implemented by derived classes
            return false;
        }

        public virtual async Task SaveChangesAsync()
        {
            // For SQL repositories, changes are committed immediately
            await Task.CompletedTask;
        }
    }
}
