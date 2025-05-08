using System;

namespace HotelApp.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        // FK mot Room
        public int RoomId { get; set; }
        public Room Room { get; set; }

        // FK mot Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public DateTime BookingDate { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public bool CheckedIn { get; set; }
        public bool CheckedOut { get; set; }

        // En-till-en-relation mot Payment
        public Payment Payment { get; set; }
    }
}
