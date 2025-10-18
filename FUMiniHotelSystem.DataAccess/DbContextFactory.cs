using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FUMiniHotelSystem.DataAccess
{
    public static class DbContextFactory
    {
        public static FUMiniHotelDbContext CreateDbContext()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            var optionsBuilder = new DbContextOptionsBuilder<FUMiniHotelDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FUMiniHotelDbContext(optionsBuilder.Options);
        }
    }
}
