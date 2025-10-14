using Microsoft.EntityFrameworkCore;
using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess
{
    public class CustomerRepository : SqlRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(HotelDbContext context) : base(context) { }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.EmailAddress.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet.Where(c => c.CustomerStatus == 1).ToListAsync();
        }
    }
}
