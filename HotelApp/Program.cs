using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HotelApp.Data;
using HotelApp.Helpers;
using HotelApp.Models;

namespace HotelApp
{
    class Program
    {
        private static HotelService hotelService;

        static void Main(string[] args)
        {
            // 1) Load config
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("json.json", optional: false, reloadOnChange: true)
                .Build();
            string cs = configuration.GetConnectionString("DefaultConnection");

            // 2) Build DbContextOptions
            var options = new DbContextOptionsBuilder<HotelContext>()
                .UseSqlServer(cs)
                .Options;

            // 3) Init service
            hotelService = new HotelService(options);

            // 4) Apply migrations
            using (var ctx = new HotelContext(options))
                ctx.Database.Migrate();

            // 5) Press key
            Console.WriteLine("Databasen är redo. Tryck valfri tangent...");
            Console.ReadKey();

            // 6) Menu loop
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("1. Registrera nytt rum");
                Console.WriteLine("2. Visa rum");
                Console.WriteLine("3. Uppdatera rum");
                Console.WriteLine("4. Radera rum");
                Console.WriteLine("5. Registrera kund");
                Console.WriteLine("6. Visa kunder");
                Console.WriteLine("7. Uppdatera kund");
                Console.WriteLine("8. Radera kund");
                Console.WriteLine("9. Skapa bokning");
                Console.WriteLine("10. Visa bokningar");
                Console.WriteLine("11. Avboka bokning");
                Console.WriteLine("12. Registrera betalning");
                Console.WriteLine("13. Annullera förfallna");
                Console.WriteLine("0. Avsluta");
                Console.Write("Val: ");

                switch (Console.ReadLine())
                {
                    case "1": AddRoom(); break;
                    case "2": ListRooms(); break;
                    case "3": UpdateRoom(); break;
                    case "4": DeleteRoom(); break;
                    case "5": AddCustomer(); break;
                    case "6": ListCustomers(); break;
                    case "7": UpdateCustomer(); break;
                    case "8": DeleteCustomer(); break;
                    case "9": CreateBooking(); break;
                    case "10": ListBookings(); break;
                    case "11": CancelBooking(); break;
                    case "12": RegisterPayment(); break;
                    case "13": CancelOverdueBookings(); break;
                    case "0": exit = true; break;
                    default:
                        Console.WriteLine("Ogiltigt val");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void AddRoom()
        {
            var num = ConsoleHelper.ReadString("Rum #? ");
            var type = ConsoleHelper.ReadString("Typ Enkelrum/Dubbelrum? ")
                       .Equals("Dubbelrum", StringComparison.OrdinalIgnoreCase)
                       ? RoomType.Dubbelrum
                       : RoomType.Enkelrum;

            int? beds = null;
            if (type == RoomType.Dubbelrum)
                beds = ConsoleHelper.ReadInt("Extrasängar? ");

            hotelService.AddRoom(new Room { RoomNumber = num, RoomType = type, ExtraBeds = beds });
            Console.WriteLine("Rum tillagt"); Console.ReadKey();
        }

        static void ListRooms()
        {
            foreach (var r in hotelService.GetRooms())
                Console.WriteLine($"{r.RoomId}: {r.RoomNumber} ({r.RoomType})");
            Console.ReadKey();
        }

        static void UpdateRoom()
        {
            int id = ConsoleHelper.ReadInt("ID att uppdatera? ");
            var r = hotelService.GetRoomById(id);
            if (r != null)
            {
                r.RoomNumber = ConsoleHelper.ReadString("Nytt nummer? ");
                hotelService.UpdateRoom(r);
                Console.WriteLine("Uppdaterat!");
            }
            else Console.WriteLine("Ej funnet.");
            Console.ReadKey();
        }

        static void DeleteRoom()
        {
            int id = ConsoleHelper.ReadInt("ID att ta bort? ");
            var r = hotelService.GetRoomById(id);
            if (r != null)
            {
                hotelService.DeleteRoom(r);
                Console.WriteLine("Bortaget!");
            }
            else Console.WriteLine("Ej funnet.");
            Console.ReadKey();
        }

        static void AddCustomer()
        {
            hotelService.AddCustomer(new Customer
            {
                Name = ConsoleHelper.ReadString("Namn? "),
                CustomerEmail = ConsoleHelper.ReadString("E-post? "),
                CustomerPhoneNumber = ConsoleHelper.ReadString("Tel? ")
            });
            Console.WriteLine("Kund tillagd"); Console.ReadKey();
        }

        static void ListCustomers()
        {
            foreach (var c in hotelService.GetCustomers())
                Console.WriteLine($"{c.CustomerId}: {c.Name}");
            Console.ReadKey();
        }

        static void UpdateCustomer()
        {
            int id = ConsoleHelper.ReadInt("ID? ");
            var c = hotelService.GetCustomerById(id);
            if (c != null)
            {
                c.Name = ConsoleHelper.ReadString("Nytt namn? ");
                hotelService.UpdateCustomer(c);
                Console.WriteLine("Uppdaterat!");
            }
            else Console.WriteLine("Ej funnet.");
            Console.ReadKey();
        }

        static void DeleteCustomer()
        {
            int id = ConsoleHelper.ReadInt("ID? ");
            var c = hotelService.GetCustomerById(id);
            if (c != null && (c.Bookings?.Any() ?? false))
                Console.WriteLine("Har bokningar!");
            else if (c != null)
            {
                hotelService.DeleteCustomer(c);
                Console.WriteLine("Bortaget!");
            }
            else Console.WriteLine("Ej funnet.");
            Console.ReadKey();
        }

        static void CreateBooking()
        {
            int cid = ConsoleHelper.ReadInt("Kund ID? ");
            int rid = ConsoleHelper.ReadInt("Rum ID? ");
            var ci = ConsoleHelper.ReadDate("In (yyyy-MM-dd)? ");
            var co = ConsoleHelper.ReadDate("Ut (yyyy-MM-dd)? ");

            if (ci < DateTime.Today) Console.WriteLine("För sent!");
            else if (co <= ci) Console.WriteLine("Fel!");
            else if (hotelService.IsRoomBooked(rid, ci, co)) Console.WriteLine("Upptaget!");
            else
            {
                hotelService.AddBooking(new Booking
                {
                    CustomerId = cid,
                    RoomId = rid,
                    BookingDate = DateTime.Now,
                    CheckInDate = ci,
                    CheckOutDate = co,
                    CheckedIn = false,
                    CheckedOut = false
                });
                Console.WriteLine("Bokat!");
            }
            Console.ReadKey();
        }

        static void ListBookings()
        {
            foreach (var b in hotelService.GetBookings())
                Console.WriteLine($"{b.BookingId}: {b.Customer.Name} -> {b.Room.RoomNumber}");
            Console.ReadKey();
        }

        static void CancelBooking()
        {
            int id = ConsoleHelper.ReadInt("Bokn ID? ");
            var b = hotelService.GetBookingById(id);
            if (b != null)
            {
                hotelService.DeleteBooking(b);
                Console.WriteLine("Avbokat!");
            }
            else Console.WriteLine("Ej funnet.");
            Console.ReadKey();
        }

        static void RegisterPayment()
        {
            // Fråga efter boknings-ID istället för PaymentId
            int bookingId = ConsoleHelper.ReadInt("Ange boknings-ID för att markera som betald: ");
            // Hämta bokningen inklusive betalningen
            var booking = hotelService.GetBookingById(bookingId);
            if (booking == null)
            {
                Console.WriteLine("Ingen bokning med det ID:t.");
            }
            else if (booking.Payment == null)
            {
                Console.WriteLine("Inga fakturor är kopplade till denna bokning.");
            }
            else if (booking.Payment.IsPaid)
            {
                Console.WriteLine("Den här fakturan är redan betald.");
            }
            else
            {
                // Anropa befintlig service-metod som tar PaymentId
                hotelService.MarkPaymentAsPaid(booking.Payment.PaymentId);
                Console.WriteLine("Betalning registrerad!");
            }
            Console.WriteLine("Tryck på valfri tangent för att återgå.");
            Console.ReadKey();
        }


        static void CancelOverdueBookings()
        {
            var n = hotelService.CancelOverdueBookings();
            Console.WriteLine($"{n} borttagna");
            Console.ReadKey();
        }
    }
}
