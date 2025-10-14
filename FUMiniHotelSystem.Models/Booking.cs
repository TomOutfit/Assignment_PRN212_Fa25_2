using System.ComponentModel.DataAnnotations;

namespace FUMiniHotelSystem.Models
{
    public class Booking
    {
        public int BookingID { get; set; }
        
        public int CustomerID { get; set; }
        
        public int RoomID { get; set; }
        
        public DateTime CheckInDate { get; set; }
        
        public DateTime CheckOutDate { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        public int BookingStatus { get; set; } = 1; // 1: Pending, 2: Confirmed, 3: Cancelled, 4: Completed
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public Customer? Customer { get; set; }
        public RoomInformation? Room { get; set; }
    }
}
