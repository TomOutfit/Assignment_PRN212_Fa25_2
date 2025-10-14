using System;

namespace StudentNameWPF.Views.Charts
{
    public class MonthlyRevenueData
    {
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
    
    public class CustomerDistributionData
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
    
    public class RoomTypePerformanceData
    {
        public int RoomTypeID { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public int RoomCount { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
