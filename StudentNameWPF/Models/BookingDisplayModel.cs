using FUMiniHotelSystem.Models;

namespace StudentNameWPF.Models
{
    public class BookingDisplayModel
    {
        public int BookingID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int BookingStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Original IDs for reference
        public int CustomerID { get; set; }
        public int RoomID { get; set; }
        
        public static BookingDisplayModel FromBooking(Booking booking, string customerName, string roomName)
        {
            return new BookingDisplayModel
            {
                BookingID = booking.BookingID,
                CustomerName = customerName,
                RoomName = roomName,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalAmount = booking.TotalAmount,
                BookingStatus = booking.BookingStatus,
                CreatedDate = booking.CreatedDate,
                Notes = booking.Notes,
                CustomerID = booking.CustomerID,
                RoomID = booking.RoomID
            };
        }
        
        public Booking ToBooking()
        {
            return new Booking
            {
                BookingID = BookingID,
                CustomerID = CustomerID,
                RoomID = RoomID,
                CheckInDate = CheckInDate,
                CheckOutDate = CheckOutDate,
                TotalAmount = TotalAmount,
                BookingStatus = BookingStatus,
                CreatedDate = CreatedDate,
                Notes = Notes
            };
        }
    }
}
