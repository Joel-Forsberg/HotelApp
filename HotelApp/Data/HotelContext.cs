using Microsoft.EntityFrameworkCore;
using HotelApp.Models;

namespace HotelApp.Data
{
    public class HotelContext : DbContext
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options) { }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationer
            modelBuilder.Entity<Room>()
                .HasMany(r => r.Bookings)
                .WithOne(b => b.Room)
                .HasForeignKey(b => b.RoomId);
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Bookings)
                .WithOne(b => b.Customer)
                .HasForeignKey(b => b.CustomerId);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId);

            // Seed-data
            modelBuilder.Entity<Room>().HasData(
                new Room { RoomId = 1, RoomNumber = "101", RoomType = RoomType.Enkelrum },
                new Room { RoomId = 2, RoomNumber = "102", RoomType = RoomType.Dubbelrum, ExtraBeds = 1 },
                new Room { RoomId = 3, RoomNumber = "103", RoomType = RoomType.Dubbelrum, ExtraBeds = 2 },
                new Room { RoomId = 4, RoomNumber = "104", RoomType = RoomType.Enkelrum }
            );
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, Name = "Anna Andersson", CustomerEmail = "anna@example.com", CustomerPhoneNumber = "0701234567" },
                new Customer { CustomerId = 2, Name = "Bertil Bengtsson", CustomerEmail = "bertil@example.com", CustomerPhoneNumber = "0708901234" },
                new Customer { CustomerId = 3, Name = "Cecilia Carlsson", CustomerEmail = "cecilia@example.com", CustomerPhoneNumber = "0705678901" },
                new Customer { CustomerId = 4, Name = "David Davidsson", CustomerEmail = "david@example.com", CustomerPhoneNumber = "0702345678" }
            );
        }
    }
}
