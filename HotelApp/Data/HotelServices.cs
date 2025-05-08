using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HotelApp.Models;

namespace HotelApp.Data
{
    public class HotelService
    {
        private readonly DbContextOptions<HotelContext> _options;
        public HotelService(DbContextOptions<HotelContext> options) => _options = options;

        // ——— Rum ———
        public void AddRoom(Room room)
        {
            using var ctx = new HotelContext(_options);
            ctx.Rooms.Add(room);
            ctx.SaveChanges();
        }

        public List<Room> GetRooms()
        {
            using var ctx = new HotelContext(_options);
            return ctx.Rooms.ToList();
        }

        public Room GetRoomById(int id)
        {
            using var ctx = new HotelContext(_options);
            return ctx.Rooms.Find(id);
        }

        public void UpdateRoom(Room room)
        {
            using var ctx = new HotelContext(_options);
            ctx.Rooms.Update(room);
            ctx.SaveChanges();
        }

        public void DeleteRoom(Room room)
        {
            using var ctx = new HotelContext(_options);
            ctx.Rooms.Remove(room);
            ctx.SaveChanges();
        }

        // ——— Kund ———
        public void AddCustomer(Customer customer)
        {
            using var ctx = new HotelContext(_options);
            ctx.Customers.Add(customer);
            ctx.SaveChanges();
        }

        public List<Customer> GetCustomers()
        {
            using var ctx = new HotelContext(_options);
            return ctx.Customers.ToList();
        }

        public Customer GetCustomerById(int id)
        {
            using var ctx = new HotelContext(_options);
            return ctx.Customers
                      .Include(c => c.Bookings)
                      .FirstOrDefault(c => c.CustomerId == id);
        }

        public void UpdateCustomer(Customer customer)
        {
            using var ctx = new HotelContext(_options);
            ctx.Customers.Update(customer);
            ctx.SaveChanges();
        }

        public void DeleteCustomer(Customer customer)
        {
            using var ctx = new HotelContext(_options);
            ctx.Customers.Remove(customer);
            ctx.SaveChanges();
        }

        // ——— Bokning ———
        public void AddBooking(Booking booking)
        {
            using var ctx = new HotelContext(_options);
            ctx.Bookings.Add(booking);
            ctx.SaveChanges();
        }

        public List<Booking> GetBookings()
        {
            using var ctx = new HotelContext(_options);
            return ctx.Bookings
                      .Include(b => b.Customer)
                      .Include(b => b.Room)
                      .Include(b => b.Payment)
                      .ToList();              // ← make sure you call ToList()
        }

        public Booking GetBookingById(int id)
        {
            using var ctx = new HotelContext(_options);
            return ctx.Bookings
                      .Include(b => b.Customer)
                      .Include(b => b.Room)
                      .Include(b => b.Payment)
                      .FirstOrDefault(b => b.BookingId == id);
        }

        public void DeleteBooking(Booking booking)
        {
            using var ctx = new HotelContext(_options);
            ctx.Bookings.Remove(booking);
            ctx.SaveChanges();
        }

        public bool IsRoomBooked(int roomId, DateTime checkIn, DateTime checkOut)
        {
            using var ctx = new HotelContext(_options);
            return ctx.Bookings.Any(b =>
                b.RoomId == roomId &&
                ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                 (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate)));
        }

        public decimal CalculateAmount(int roomId, DateTime checkIn, DateTime checkOut)
        {
            using var ctx = new HotelContext(_options);
            var room = ctx.Rooms.Find(roomId);
            int nights = (checkOut - checkIn).Days;
            return nights * (room.RoomType == RoomType.Enkelrum ? 500 : 800);
        }

        // ——— Betalning ———
        public void AddPayment(Payment payment)
        {
            using var ctx = new HotelContext(_options);
            ctx.Payments.Add(payment);
            ctx.SaveChanges();
        }

        public void MarkPaymentAsPaid(int paymentId)
        {
            using var ctx = new HotelContext(_options);
            var pay = ctx.Payments.Find(paymentId);
            if (pay == null) throw new InvalidOperationException("Betalning hittades inte.");
            pay.IsPaid = true;
            pay.PaymentDate = DateTime.Now;
            ctx.SaveChanges();
        }

        public int CancelOverdueBookings()
        {
            using var ctx = new HotelContext(_options);
            var cutoff = DateTime.Now.AddDays(-10);
            var overdue = ctx.Bookings
                .Include(b => b.Payment)
                .Where(b => b.BookingDate <= cutoff && (b.Payment == null || !b.Payment.IsPaid))
                .ToList();

            ctx.Payments.RemoveRange(overdue.Where(b => b.Payment != null).Select(b => b.Payment));
            ctx.Bookings.RemoveRange(overdue);
            ctx.SaveChanges();
            return overdue.Count;
        }
    }
}
