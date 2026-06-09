using System;
using SBD.Services;

namespace SBD.UI
{
    /// <summary>
    /// Menu raportów i analityki.
    /// </summary>
    public class ReportMenu
    {
        private readonly ReportService _reportService = new ReportService();

        public void Show()
        {
            bool running = true;
            while (running)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("\n═══════════ RAPORTY ═══════════");
                Console.ResetColor();
                Console.WriteLine("  1. Podsumowanie systemu");
                Console.WriteLine("  2. Przychody miesięczne (wszystkie)");
                Console.WriteLine("  3. Przychody za miesiąc");
                Console.WriteLine("  4. Wykorzystanie floty");
                Console.WriteLine("  5. Klienci wysokiego ryzyka");
                Console.WriteLine("  6. Historia zmian");
                Console.WriteLine("  7. Historia zmian (tabela)");
                Console.WriteLine("  0. Powrót");
                Console.WriteLine("═══════════════════════════════");

                string choice = ConsoleMenu.ReadChoice();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            _reportService.ShowSystemSummary();
                            break;
                        case "2":
                            _reportService.ShowMonthlyRevenue();
                            break;
                        case "3":
                            int month = ConsoleMenu.ReadInt("Miesiąc (1-12)");
                            int year = ConsoleMenu.ReadInt("Rok");
                            _reportService.ShowMonthlyRevenue(month, year);
                            break;
                        case "4":
                            _reportService.ShowCarUtilization();
                            break;
                        case "5":
                            _reportService.ShowHighRiskClients();
                            break;
                        case "6":
                            int topN = ConsoleMenu.ReadInt("Ile ostatnich wpisów");
                            _reportService.ShowChangeHistory(null, topN);
                            break;
                        case "7":
                            string tableName = ConsoleMenu.ReadInput("Nazwa tabeli (RENTALS/PAYMENTS/CLIENTS/CARS)");
                            _reportService.ShowChangeHistory(tableName.ToUpper());
                            break;
                        case "0":
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Nieprawidłowy wybór.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n✖ Błąd: {ex.Message}");
                    Console.ResetColor();
                }

                if (running && choice != "0")
                    ConsoleMenu.Pause();
            }
        }
    }
}
