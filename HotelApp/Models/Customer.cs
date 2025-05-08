using System.Collections.Generic;

namespace HotelApp.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }

        // Navigationsegenskap
        public ICollection<Booking> Bookings { get; set; }
    }
}
