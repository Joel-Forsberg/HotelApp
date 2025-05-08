using System;

namespace HotelApp.Helpers
{
    public static class ConsoleHelper
    {
        public static int ReadInt(string prompt)
        {
            Console.Write(prompt);
            int value;
            while (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.Write("Felanpassning, försök igen: ");
            }
            return value;
        }

        public static string ReadString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public static DateTime ReadDate(string prompt)
        {
            Console.Write(prompt);
            DateTime date;
            while (!DateTime.TryParse(Console.ReadLine(), out date))
            {
                Console.Write("Felaktigt datumformat, försök igen (yyyy-MM-dd): ");
            }
            return date;
        }
    }
}
