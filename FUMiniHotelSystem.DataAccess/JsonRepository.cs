using System.Text.Json;
using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess
{
    public class JsonRepository<T> : IRepository<T> where T : class
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private List<T> _entities;

        public JsonRepository(string fileName)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _filePath = Path.Combine(baseDirectory, "Data", fileName);
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _entities = new List<T>();
            
            EnsureDataDirectoryExists();
        }

        private void EnsureDataDirectoryExists()
        {
            var dataDir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir!);
            }
        }

        private async Task LoadEntitiesAsync()
        {
            if (!File.Exists(_filePath))
            {
                _entities = new List<T>();
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _entities = new List<T>();
                }
                else
                {
                    _entities = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
                }
            }
            catch
            {
                _entities = new List<T>();
            }
        }

        private async Task SaveEntitiesAsync()
        {
            var json = JsonSerializer.Serialize(_entities, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<T>> GetAllAsync()
        {
            await LoadEntitiesAsync();
            return _entities.ToList();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            await LoadEntitiesAsync();
            return _entities.FirstOrDefault(x => GetId(x) == id);
        }

        public async Task<T> AddAsync(T entity)
        {
            await LoadEntitiesAsync();
            _entities.Add(entity);
            await SaveEntitiesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            await LoadEntitiesAsync();
            var index = _entities.FindIndex(x => GetId(x) == GetId(entity));
            if (index >= 0)
            {
                _entities[index] = entity;
                await SaveEntitiesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await LoadEntitiesAsync();
            var entity = _entities.FirstOrDefault(x => GetId(x) == id);
            if (entity != null)
            {
                _entities.Remove(entity);
                await SaveEntitiesAsync();
                return true;
            }
            return false;
        }

        public async Task SaveChangesAsync()
        {
            await SaveEntitiesAsync();
        }


        private int GetId(T entity)
        {
            var idProperty = typeof(T).GetProperty("CustomerID") ?? 
                           typeof(T).GetProperty("RoomID") ?? 
                           typeof(T).GetProperty("RoomTypeID") ?? 
                           typeof(T).GetProperty("BookingID");
            
            return (int)(idProperty?.GetValue(entity) ?? 0);
        }
    }
}
