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
            // 1) Läs in konfiguration och anslutningssträng
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("json.json", optional: false, reloadOnChange: true)
                .Build();
            string cs = configuration.GetConnectionString("DefaultConnection");

            // 2) Bygg DbContextOptions
            var options = new DbContextOptionsBuilder<HotelContext>()
                .UseSqlServer(cs)
                .Options;

            // 3) Initiera service
            hotelService = new HotelService(options);

            // 4) Applicera migrationer
            using (var ctx = new HotelContext(options))
                ctx.Database.Migrate();

            // 5) Startmeddelande
            Console.WriteLine("Databasen är redo. Tryck valfri tangent...");
            Console.ReadKey();

            // 6) Meny
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
                Console.WriteLine("14. Visa betalningar");
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
                    case "14": ListPayments(); break;
                    case "0": exit = true; break;
                    default:
                        Console.WriteLine("Ogiltigt val, försök igen.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // ——— RUM ———
        static void AddRoom()
        {
            var num = ConsoleHelper.ReadString("Ange rumsnummer (ex: 101): ");
            var type = ConsoleHelper.ReadString("Rumstyp (Enkelrum/Dubbelrum): ")
                       .Equals("Dubbelrum", StringComparison.OrdinalIgnoreCase)
                ? RoomType.Dubbelrum
                : RoomType.Enkelrum;

            int? beds = null;
            if (type == RoomType.Dubbelrum)
                beds = ConsoleHelper.ReadInt("Antal extrasängar (max 2): ");

            hotelService.AddRoom(new Room { RoomNumber = num, RoomType = type, ExtraBeds = beds });
            Console.WriteLine("Rum tillagt!");
            Console.ReadKey();
        }

        static void ListRooms()
        {
            foreach (var r in hotelService.GetRooms())
                Console.WriteLine($"{r.RoomId}: {r.RoomNumber} ({r.RoomType}), Extrasängar: {r.ExtraBeds}");
            Console.ReadKey();
        }

        static void UpdateRoom()
        {
            int id = ConsoleHelper.ReadInt("Ange ID för rummet att uppdatera: ");
            var r = hotelService.GetRoomById(id);
            if (r != null)
            {
                r.RoomNumber = ConsoleHelper.ReadString("Nytt rumsnummer (ex: 202): ");
                hotelService.UpdateRoom(r);
                Console.WriteLine("Rum uppdaterat!");
            }
            else Console.WriteLine("Rum hittades inte.");
            Console.ReadKey();
        }

        static void DeleteRoom()
        {
            int id = ConsoleHelper.ReadInt("Ange ID för rummet att radera: ");
            var r = hotelService.GetRoomById(id);
            if (r != null)
            {
                hotelService.DeleteRoom(r);
                Console.WriteLine("Rum raderat!");
            }
            else Console.WriteLine("Rum hittades inte.");
            Console.ReadKey();
        }

        // ——— KUND ———
        static void AddCustomer()
        {
            var name = ConsoleHelper.ReadString("Ange kundens namn: ");
            var email = ConsoleHelper.ReadString("Ange kundens e-post (ex: anna@example.com): ");
            var phone = ConsoleHelper.ReadString("Ange kundens telefonnummer (ex: 070 123 45 67): ");
            hotelService.AddCustomer(new Customer
            {
                Name = name,
                CustomerEmail = email,
                CustomerPhoneNumber = phone
            });
            Console.WriteLine("Kund tillagd!");
            Console.ReadKey();
        }

        static void ListCustomers()
        {
            foreach (var c in hotelService.GetCustomers())
                Console.WriteLine($"{c.CustomerId}: {c.Name}, {c.CustomerEmail}, {c.CustomerPhoneNumber}");
            Console.ReadKey();
        }

        static void UpdateCustomer()
        {
            int id = ConsoleHelper.ReadInt("Ange ID för kunden att uppdatera: ");
            var c = hotelService.GetCustomerById(id);
            if (c != null)
            {
                c.Name = ConsoleHelper.ReadString("Nytt namn: ");
                c.CustomerEmail = ConsoleHelper.ReadString("Ny e-post (ex: anders@example.com): ");
                c.CustomerPhoneNumber = ConsoleHelper.ReadString("Nytt telefonnummer (ex: 070 987 65 43): ");
                hotelService.UpdateCustomer(c);
                Console.WriteLine("Kund uppdaterad!");
            }
            else Console.WriteLine("Kund hittades inte.");
            Console.ReadKey();
        }

        static void DeleteCustomer()
        {
            int id = ConsoleHelper.ReadInt("Ange ID för kunden att radera: ");
            var c = hotelService.GetCustomerById(id);
            if (c != null && (c.Bookings?.Any() ?? false))
                Console.WriteLine("Kunden har bokningar och kan inte raderas.");
            else if (c != null)
            {
                hotelService.DeleteCustomer(c);
                Console.WriteLine("Kund raderad!");
            }
            else Console.WriteLine("Kund hittades inte.");
            Console.ReadKey();
        }

        // ——— BOKNING ———
        static void CreateBooking()
        {
            Console.WriteLine("----- KUNDER -----");
            foreach (var c in hotelService.GetCustomers())
                Console.WriteLine($"ID {c.CustomerId}: {c.Name}");
            Console.WriteLine("------------------");

            Console.WriteLine("----- RUM -----");
            foreach (var r in hotelService.GetRooms())
                Console.WriteLine($"ID {r.RoomId}: Rum {r.RoomNumber} ({r.RoomType})");
            Console.WriteLine("---------------");

            int custId;
            Customer cust;
            while (true)
            {
                custId = ConsoleHelper.ReadInt("Ange kund-ID: ");
                cust = hotelService.GetCustomerById(custId);
                if (cust != null) break;
                Console.WriteLine("Ingen kund med det ID:t. Försök igen.");
            }

            int roomId;
            Room room;
            while (true)
            {
                roomId = ConsoleHelper.ReadInt("Ange rum-ID: ");
                room = hotelService.GetRoomById(roomId);
                if (room != null) break;
                Console.WriteLine("Inget rum med det ID:t. Försök igen.");
            }

            DateTime checkIn, checkOut;
            while (true)
            {
                checkIn = ConsoleHelper.ReadDate("Incheckningsdatum (yyyy-MM-dd): ");
                checkOut = ConsoleHelper.ReadDate("Utcheckningsdatum (yyyy-MM-dd): ");

                if (checkIn.Date < DateTime.Today)
                    Console.WriteLine("Incheckningsdatum kan inte vara i det förflutna.");
                else if (checkOut <= checkIn)
                    Console.WriteLine("Utcheckningsdatum måste vara efter incheckningsdatum.");
                else if (hotelService.IsRoomBooked(roomId, checkIn, checkOut))
                    Console.WriteLine("Rummet är redan bokat under vald period.");
                else
                    break;

                Console.WriteLine("Försök igen.");
            }

            var booking = new Booking
            {
                CustomerId = custId,
                RoomId = roomId,
                BookingDate = DateTime.Now,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                CheckedIn = false,
                CheckedOut = false
            };
            hotelService.AddBooking(booking);

            var amount = hotelService.CalculateAmount(roomId, checkIn, checkOut);
            hotelService.AddPayment(new Payment
            {
                BookingId = booking.BookingId,
                Amount = amount,
                IsPaid = false
            });

            Console.WriteLine($"Bokning skapad för {cust.Name}, Rum {room.RoomNumber}, {checkIn:yyyy-MM-dd}→{checkOut:yyyy-MM-dd}, Belopp {amount} kr");
            Console.ReadKey();
        }

        static void ListBookings()
        {
            foreach (var b in hotelService.GetBookings())
                Console.WriteLine(
                    $"{b.BookingId}: Kund={b.Customer.Name}, Rum={b.Room.RoomNumber}, " +
                    $"In={b.CheckInDate:yyyy-MM-dd}, Ut={b.CheckOutDate:yyyy-MM-dd}, Betald={b.Payment?.IsPaid}");
            Console.ReadKey();
        }

        static void CancelBooking()
        {
            int id = ConsoleHelper.ReadInt("Ange boknings-ID att avboka: ");
            var b = hotelService.GetBookingById(id);
            if (b != null)
            {
                hotelService.DeleteBooking(b);
                Console.WriteLine("Bokning avbokad!");
            }
            else Console.WriteLine("Ingen sådan bokning.");
            Console.ReadKey();
        }

        // ——— BETALNING ———
        static void RegisterPayment()
        {
            Console.WriteLine("----- BEFINSTIGA BOKNINGAR -----");
            foreach (var b in hotelService.GetBookings())
            {
                Console.WriteLine(
                    $"ID {b.BookingId}: Kund={b.Customer.Name}, Rum={b.Room.RoomNumber}, " +
                    $"In={b.CheckInDate:yyyy-MM-dd}, Ut={b.CheckOutDate:yyyy-MM-dd}, Betald={b.Payment?.IsPaid}");
            }
            Console.WriteLine("---------------------------------");

            int bookingId = ConsoleHelper.ReadInt("Ange boknings-ID för betalning: ");
            var booking = hotelService.GetBookingById(bookingId);

            if (booking == null)
                Console.WriteLine("Ingen bokning med det ID:t.");
            else if (booking.Payment == null)
                Console.WriteLine("Ingen faktura finns kopplad.");
            else if (booking.Payment.IsPaid)
                Console.WriteLine("Betalningen är redan registrerad.");
            else
            {
                hotelService.MarkPaymentAsPaid(booking.Payment.PaymentId);
                Console.WriteLine("Betalning registrerad!");
            }
            Console.ReadKey();
        }

        static void CancelOverdueBookings()
        {
            var n = hotelService.CancelOverdueBookings();
            Console.WriteLine($"{n} bokning(ar) annullerade pga utebliven betalning.");
            Console.ReadKey();
        }

        static void ListPayments()
        {
            foreach (var p in hotelService.GetPayments())
            {
                Console.WriteLine(
                    $"PayId:{p.PaymentId}, BoknId:{p.BookingId}, Belopp:{p.Amount}, " +
                    $"Betald:{p.IsPaid}, Kund:{p.Booking.Customer.Name}, Rum:{p.Booking.Room.RoomNumber}");
            }
            Console.ReadKey();
        }
    }
}
