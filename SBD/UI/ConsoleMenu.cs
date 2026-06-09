using System;

namespace SBD.UI
{
    /// <summary>
    /// Menu główne aplikacji CarRent DB.
    /// </summary>
    public class ConsoleMenu
    {
        private readonly ClientMenu _clientMenu = new ClientMenu();
        private readonly CarMenu _carMenu = new CarMenu();
        private readonly RentalMenu _rentalMenu = new RentalMenu();
        private readonly ReportMenu _reportMenu = new ReportMenu();

        public void Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ShowWelcome();

            bool running = true;
            while (running)
            {
                ShowMainMenu();
                string choice = ReadChoice();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            _clientMenu.Show();
                            break;
                        case "2":
                            _carMenu.Show();
                            break;
                        case "3":
                            _rentalMenu.Show();
                            break;
                        case "4":
                            _reportMenu.Show();
                            break;
                        case "5":
                            TestConnection();
                            break;
                        case "0":
                            running = false;
                            Console.WriteLine("\nDo widzenia!");
                            break;
                        default:
                            Console.WriteLine("Nieprawidłowy wybór. Spróbuj ponownie.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n✖ Błąd: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        private void ShowWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ╔═══════════════════════════════════════════════════════╗
  ║                                                       ║
  ║          🚗  CarRent DB - System Wypożyczalni  🚗     ║
  ║                                                       ║
  ║     Systemy Baz Danych                                ║
  ║     Adam Przestrzelski, Paweł Sławiński               ║
  ║                                                       ║
  ╚═══════════════════════════════════════════════════════╝
");
            Console.ResetColor();
        }

        private void ShowMainMenu()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n═══════════ MENU GŁÓWNE ═══════════");
            Console.ResetColor();
            Console.WriteLine("  1. Klienci");
            Console.WriteLine("  2. Samochody");
            Console.WriteLine("  3. Wypożyczenia i Rezerwacje");
            Console.WriteLine("  4. Raporty i Analityka");
            Console.WriteLine("  5. Test połączenia z bazą");
            Console.WriteLine("  0. Wyjście");
            Console.WriteLine("═══════════════════════════════════");
        }

        private void TestConnection()
        {
            Console.WriteLine("\nTestowanie połączenia z bazą danych...");
            Database.DbConnection.Instance.TestConnection();
        }

        public static string ReadChoice()
        {
            Console.Write("\n>> Wybierz opcję: ");
            return Console.ReadLine()?.Trim();
        }

        public static string ReadInput(string prompt)
        {
            Console.Write($"  {prompt}: ");
            return Console.ReadLine()?.Trim();
        }

        public static int ReadInt(string prompt)
        {
            while (true)
            {
                string input = ReadInput(prompt);
                if (int.TryParse(input, out int value))
                    return value;
                Console.WriteLine("  Wprowadź prawidłową liczbę.");
            }
        }

        public static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                string input = ReadInput(prompt);
                if (decimal.TryParse(input, out decimal value))
                    return value;
                Console.WriteLine("  Wprowadź prawidłową kwotę.");
            }
        }

        public static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                string input = ReadInput($"{prompt} (RRRR-MM-DD)");
                if (DateTime.TryParse(input, out DateTime value))
                    return value;
                Console.WriteLine("  Wprowadź prawidłową datę (format: RRRR-MM-DD).");
            }
        }

        public static void Pause()
        {
            Console.WriteLine("\n  Naciśnij dowolny klawisz, aby kontynuować...");
            Console.ReadKey(true);
        }
    }
}
