namespace FUMiniHotelSystem.Models
{
    public class AppSettings
    {
        public string AdminEmail { get; set; } = "admin@FUMiniHotelSystem.com";
        public string AdminPassword { get; set; } = "@@abc123@@";
        public string DataPath { get; set; } = "Data";
        public string ConnectionString { get; set; } = "Server=(localdb)\\mssqllocaldb;Database=FUMiniHotelManagement;Trusted_Connection=true;MultipleActiveResultSets=true";
    }
}
