using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByEmailAsync(string email);
        Task<List<Customer>> GetActiveCustomersAsync();
    }
}
