using System;
using System.Data;
using SBD.Repositories;

namespace SBD.Services
{
    /// <summary>
    /// Serwis generujący raporty biznesowe.
    /// </summary>
    public class ReportService
    {
        private readonly ReportRepository _reportRepo = new ReportRepository();

        /// <summary>
        /// Wyświetla podsumowanie systemu.
        /// </summary>
        public void ShowSystemSummary()
        {
            var dt = _reportRepo.GetSystemSummary();
            if (dt.Rows.Count == 0) return;

            var row = dt.Rows[0];
            Console.WriteLine("\n╔══════════════════════════════════════════╗");
            Console.WriteLine("║        PODSUMOWANIE SYSTEMU CarRent     ║");
            Console.WriteLine("╠══════════════════════════════════════════╣");
            Console.WriteLine($"║  Klienci:            {row["TOTAL_CLIENTS"],15}  ║");
            Console.WriteLine($"║  Samochody:          {row["TOTAL_CARS"],15}  ║");
            Console.WriteLine($"║  Dostępne auta:      {row["AVAILABLE_CARS"],15}  ║");
            Console.WriteLine($"║  Aktywne wypożycz.:  {row["ACTIVE_RENTALS"],15}  ║");
            Console.WriteLine($"║  Aktywne rezerwacje: {row["ACTIVE_RESERVATIONS"],15}  ║");
            Console.WriteLine($"║  Przychód łączny:    {Convert.ToDecimal(row["TOTAL_REVENUE"]),12:N2} PLN ║");
            Console.WriteLine($"║  Kary łącznie:       {row["TOTAL_PENALTIES"],15}  ║");
            Console.WriteLine($"║  Zablokowani klienci:{row["BLOCKED_CLIENTS"],15}  ║");
            Console.WriteLine($"║  Oddziały:           {row["TOTAL_BRANCHES"],15}  ║");
            Console.WriteLine($"║  Pracownicy:         {row["TOTAL_EMPLOYEES"],15}  ║");
            Console.WriteLine("╚══════════════════════════════════════════╝");
        }

        /// <summary>
        /// Wyświetla raport przychodów miesięcznych.
        /// </summary>
        public void ShowMonthlyRevenue(int? month = null, int? year = null)
        {
            DataTable dt;
            if (month.HasValue && year.HasValue)
                dt = _reportRepo.GetMonthlyRevenue(month.Value, year.Value);
            else
                dt = _reportRepo.GetMonthlyRevenue();

            Console.WriteLine("\n┌────────────┬────────────┬──────────────┬──────────────┐");
            Console.WriteLine("│  Miesiąc   │ Wypożycz.  │   Przychód   │  Śr. płatn.  │");
            Console.WriteLine("├────────────┼────────────┼──────────────┼──────────────┤");

            foreach (DataRow row in dt.Rows)
            {
                Console.WriteLine($"│  {row["REVENUE_MONTH"],2}/{row["REVENUE_YEAR"]}  │ {row["TOTAL_RENTALS"],10} │ {Convert.ToDecimal(row["TOTAL_REVENUE"]),9:N2} PLN│ {Convert.ToDecimal(row["AVG_PAYMENT"]),9:N2} PLN│");
            }

            Console.WriteLine("└────────────┴────────────┴──────────────┴──────────────┘");

            if (dt.Rows.Count == 0)
                Console.WriteLine("  Brak danych do wyświetlenia.");
        }

        /// <summary>
        /// Wyświetla raport wykorzystania floty.
        /// </summary>
        public void ShowCarUtilization()
        {
            var dt = _reportRepo.GetCarUtilization();

            Console.WriteLine("\n┌───────────────────────────┬────────────┬──────────┬───────────┬──────────────┐");
            Console.WriteLine("│       Samochód            │  Kategoria │  Status  │ Dni wypoż.│   Przychód   │");
            Console.WriteLine("├───────────────────────────┼────────────┼──────────┼───────────┼──────────────┤");

            foreach (DataRow row in dt.Rows)
            {
                string carName = row["CAR_NAME"].ToString();
                if (carName.Length > 23) carName = carName.Substring(0, 20) + "...";

                Console.WriteLine($"│ {carName,-25} │ {row["CATEGORY_NAME"],-10} │ {row["CURRENT_STATUS"],-8} │ {row["TOTAL_RENTAL_DAYS"],9} │ {Convert.ToDecimal(row["TOTAL_REVENUE"]),9:N2} PLN│");
            }

            Console.WriteLine("└───────────────────────────┴────────────┴──────────┴───────────┴──────────────┘");
        }

        /// <summary>
        /// Wyświetla raport klientów wysokiego ryzyka.
        /// </summary>
        public void ShowHighRiskClients()
        {
            var dt = _reportRepo.GetHighRiskClients();

            Console.WriteLine("\n┌─────────────────────────┬──────────┬──────────────┬───────────┬──────────┐");
            Console.WriteLine("│       Klient            │  Kary    │  Kwota kar   │  Mnożnik  │ Zablok.  │");
            Console.WriteLine("├─────────────────────────┼──────────┼──────────────┼───────────┼──────────┤");

            foreach (DataRow row in dt.Rows)
            {
                string clientName = row["CLIENT_NAME"].ToString();
                if (clientName.Length > 21) clientName = clientName.Substring(0, 18) + "...";
                string blocked = Convert.ToInt32(row["IS_BLOCKED"]) == 1 ? "TAK" : "NIE";

                Console.WriteLine($"│ {clientName,-23} │ {row["PENALTY_COUNT"],8} │ {Convert.ToDecimal(row["TOTAL_PENALTY_AMOUNT"]),9:N2} PLN│ {Convert.ToDecimal(row["PENALTY_MULTIPLIER"]),9:F2} │ {blocked,-8} │");
            }

            Console.WriteLine("└─────────────────────────┴──────────┴──────────────┴───────────┴──────────┘");

            if (dt.Rows.Count == 0)
                Console.WriteLine("  Brak klientów wysokiego ryzyka.");
        }

        /// <summary>
        /// Wyświetla historię zmian.
        /// </summary>
        public void ShowChangeHistory(string tableName = null, int topN = 20)
        {
            var dt = _reportRepo.GetChangeHistory(tableName, null, topN);

            Console.WriteLine($"\n--- Historia zmian{(tableName != null ? $" ({tableName})" : "")} (ostatnie {topN}) ---");
            Console.WriteLine($"{"Data",-20} {"Tabela",-15} {"ID",-8} {"Operacja",-10} {"Użytkownik",-15}");
            Console.WriteLine(new string('-', 70));

            foreach (DataRow row in dt.Rows)
            {
                Console.WriteLine($"{Convert.ToDateTime(row["CHANGE_DATE"]),-20:yyyy-MM-dd HH:mm} " +
                                  $"{row["TABLE_NAME"],-15} {row["RECORD_ID"],-8} " +
                                  $"{row["OPERATION_TYPE"],-10} {row["CHANGED_BY"],-15}");
            }

            if (dt.Rows.Count == 0)
                Console.WriteLine("  Brak wpisów w historii zmian.");
        }
    }
}
