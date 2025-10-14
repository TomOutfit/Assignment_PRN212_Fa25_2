using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.BusinessLogic
{
    public class AuthenticationService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly AppSettings _appSettings;

        public AuthenticationService(ICustomerRepository customerRepository, AppSettings appSettings)
        {
            _customerRepository = customerRepository;
            _appSettings = appSettings;
        }

        public async Task<Customer?> AuthenticateAsync(string email, string password)
        {
            try
            {
                // Check admin credentials first
                if (email.Equals(_appSettings.AdminEmail, StringComparison.OrdinalIgnoreCase) &&
                    password == _appSettings.AdminPassword)
                {
                    return new Customer
                    {
                        CustomerID = 0,
                        CustomerFullName = "Administrator",
                        EmailAddress = _appSettings.AdminEmail,
                        CustomerStatus = 1
                    };
                }

                // Check customer credentials
                var customer = await _customerRepository.GetByEmailAsync(email);
                if (customer != null && customer.Password == password && customer.CustomerStatus == 1)
                {
                    return customer;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Authentication error: {ex.Message}", ex);
            }
        }
    }
}
