using System;
using SBD.UI;

namespace SBD
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var menu = new ConsoleMenu();
                menu.Run();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nKrytyczny błąd aplikacji: {ex.Message}");
                Console.WriteLine($"Szczegóły: {ex.StackTrace}");
                Console.ResetColor();
                Console.WriteLine("\nNaciśnij dowolny klawisz, aby zakończyć...");
                Console.ReadKey();
            }
        }
    }
}
