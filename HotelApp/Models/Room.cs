using System.Collections.Generic;

namespace HotelApp.Models
{
    public enum RoomType
    {
        Enkelrum,
        Dubbelrum
    }

    public class Room
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public RoomType RoomType { get; set; }
        // Endast relevant för dubbelrum
        public int? ExtraBeds { get; set; }

        // Navigationsegenskap
        public ICollection<Booking> Bookings { get; set; }
    }
}
