using System.ComponentModel.DataAnnotations;

namespace FUMiniHotelSystem.Models
{
    public class RoomInformation
    {
        public int RoomID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RoomNumber { get; set; } = string.Empty;
        
        [StringLength(220)]
        public string RoomDescription { get; set; } = string.Empty;
        
        public int RoomMaxCapacity { get; set; }
        
        public int RoomStatus { get; set; } = 1; // 1: Active, 2: Deleted
        
        [Required]
        public decimal RoomPricePerDate { get; set; }
        
        public int RoomTypeID { get; set; }
        
        // Navigation property
        public RoomType? RoomType { get; set; }
    }
}
