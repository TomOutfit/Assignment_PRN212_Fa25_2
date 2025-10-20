using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FUMiniHotelSystem.DataAccess;
using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.BusinessLogic;
using FUMiniHotelSystem.Models;

namespace StudentNameWPF.Services
{
    public static class ServiceContainer
    {
        private static IServiceProvider? _serviceProvider;

        public static void Initialize()
        {
            var services = new ServiceCollection();

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                "Server=(localdb)\\mssqllocaldb;Database=FUMiniHotelManagement;Trusted_Connection=true;MultipleActiveResultSets=true";

            // Register configuration
            var appSettings = new AppSettings
            {
                AdminEmail = configuration["AdminEmail"] ?? "admin@FUMiniHotelSystem.com",
                AdminPassword = configuration["AdminPassword"] ?? "@@abc123@@",
                ConnectionString = connectionString
            };
            services.AddSingleton(appSettings);

            // Register services
            services.AddDataAccessServices(connectionString);

            // Register business logic services
            services.AddScoped<AuthenticationService>();
            services.AddScoped<CustomerService>();
            services.AddScoped<RoomService>();
            services.AddScoped<BookingService>();

            // Register other services
            services.AddScoped<ReportExportService>();
            services.AddScoped<ChartExportService>();
            services.AddScoped<PDFExportService>();
            services.AddScoped<ExcelExportService>();
            services.AddScoped<RealtimeDataService>();

            _serviceProvider = services.BuildServiceProvider();
        }

        public static T GetService<T>() where T : notnull
        {
            if (_serviceProvider == null)
            {
                Initialize();
            }
            return _serviceProvider!.GetRequiredService<T>();
        }

        public static void Dispose()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _serviceProvider = null;
        }
    }
}
