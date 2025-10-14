using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.BusinessLogic
{
    public class CustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<Customer>> GetActiveCustomersAsync()
        {
            return await _customerRepository.GetActiveCustomersAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            // Generate new ID
            var customers = await _customerRepository.GetAllAsync();
            customer.CustomerID = customers.Count > 0 ? customers.Max(c => c.CustomerID) + 1 : 1;
            customer.CustomerStatus = 1;
            
            return await _customerRepository.AddAsync(customer);
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            return await _customerRepository.UpdateAsync(customer);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer != null)
            {
                customer.CustomerStatus = 2; // Mark as deleted
                return await _customerRepository.UpdateAsync(customer);
            }
            return false;
        }

        public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
        {
            var customers = await _customerRepository.GetActiveCustomersAsync();
            return customers.Where(c => 
                c.CustomerFullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.EmailAddress.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Telephone.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
    }
}
