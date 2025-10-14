using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.Models;

namespace FUMiniHotelSystem.DataAccess
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddDataAccessServices(this IServiceCollection services, string connectionString)
        {
            // Register DbContext
            services.AddDbContext<HotelDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
            
            // Also register concrete classes for direct injection
            services.AddScoped<CustomerRepository>();
            services.AddScoped<BookingRepository>();
            services.AddScoped<RoomRepository>();
            services.AddScoped<RoomTypeRepository>();

            return services;
        }
    }
}
