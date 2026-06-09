using System;
using System.Collections.Generic;
using SBD.Models;
using SBD.Repositories;

namespace SBD.UI
{
    /// <summary>
    /// Menu zarządzania samochodami.
    /// </summary>
    public class CarMenu
    {
        private readonly CarRepository _carRepo = new CarRepository();
        private readonly BranchRepository _branchRepo = new BranchRepository();

        public void Show()
        {
            bool running = true;
            while (running)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n═══════════ SAMOCHODY ═══════════");
                Console.ResetColor();
                Console.WriteLine("  1. Lista wszystkich samochodów");
                Console.WriteLine("  2. Dostępne samochody");
                Console.WriteLine("  3. Szukaj wg kategorii");
                Console.WriteLine("  4. Szukaj wg oddziału");
                Console.WriteLine("  5. Szczegóły samochodu");
                Console.WriteLine("  6. Dodaj samochód");
                Console.WriteLine("  7. Edytuj samochód");
                Console.WriteLine("  8. Usuń samochód");
                Console.WriteLine("  9. Kategorie samochodów");
                Console.WriteLine("  10. Oddziały");
                Console.WriteLine("  0. Powrót");
                Console.WriteLine("═════════════════════════════════");

                string choice = ConsoleMenu.ReadChoice();

                try
                {
                    switch (choice)
                    {
                        case "1": ListAll(); break;
                        case "2": ListAvailable(); break;
                        case "3": SearchByCategory(); break;
                        case "4": SearchByBranch(); break;
                        case "5": ShowDetails(); break;
                        case "6": Add(); break;
                        case "7": Edit(); break;
                        case "8": Delete(); break;
                        case "9": ShowCategories(); break;
                        case "10": ShowBranches(); break;
                        case "0": running = false; break;
                        default: Console.WriteLine("Nieprawidłowy wybór."); break;
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

        private void ListAll()
        {
            var cars = _carRepo.GetAll();
            Console.WriteLine($"\n--- Wszystkie samochody ({cars.Count}) ---");
            foreach (var c in cars)
                Console.WriteLine($"  {c}");
        }

        private void ListAvailable()
        {
            var cars = _carRepo.GetAvailable();
            Console.WriteLine($"\n--- Dostępne samochody ({cars.Count}) ---");
            foreach (var c in cars)
                Console.WriteLine($"  {c}");
        }

        private void SearchByCategory()
        {
            ShowCategories();
            int catId = ConsoleMenu.ReadInt("ID kategorii");
            var cars = _carRepo.SearchByCategory(catId);
            Console.WriteLine($"\n--- Samochody w kategorii ({cars.Count}) ---");
            foreach (var c in cars)
                Console.WriteLine($"  {c}");
        }

        private void SearchByBranch()
        {
            ShowBranches();
            int branchId = ConsoleMenu.ReadInt("ID oddziału");
            var cars = _carRepo.SearchByBranch(branchId);
            Console.WriteLine($"\n--- Samochody w oddziale ({cars.Count}) ---");
            foreach (var c in cars)
                Console.WriteLine($"  {c}");
        }

        private void ShowDetails()
        {
            int id = ConsoleMenu.ReadInt("ID samochodu");
            var car = _carRepo.GetById(id);
            if (car == null)
            {
                Console.WriteLine("Nie znaleziono samochodu.");
                return;
            }

            Console.WriteLine($"\n╔══════════════════════════════════╗");
            Console.WriteLine($"  {car.FullName}");
            Console.WriteLine($"  Rejestracja: {car.PlateNumber}");
            Console.WriteLine($"  VIN:         {car.Vin}");
            Console.WriteLine($"  Kolor:       {car.Color}");
            Console.WriteLine($"  Miejsca:     {car.Seats}");
            Console.WriteLine($"  Przebieg:    {car.Mileage:N0} km");
            Console.WriteLine($"  Stawka:      {car.DailyRate:F2} PLN/dzień");
            Console.WriteLine($"  Kategoria:   {car.CategoryName}");
            Console.WriteLine($"  Oddział:     {car.BranchName}");
            Console.WriteLine($"  Status:      {car.Status}");
            Console.WriteLine($"╚══════════════════════════════════╝");
        }

        private void Add()
        {
            Console.WriteLine("\n--- Dodaj nowy samochód ---");
            ShowCategories();
            ShowBranches();

            var car = new Car
            {
                Brand = ConsoleMenu.ReadInput("Marka"),
                Model = ConsoleMenu.ReadInput("Model"),
                Year = ConsoleMenu.ReadInt("Rok produkcji"),
                PlateNumber = ConsoleMenu.ReadInput("Nr rejestracyjny"),
                Vin = ConsoleMenu.ReadInput("VIN (17 znaków)"),
                DailyRate = ConsoleMenu.ReadDecimal("Stawka dzienna (PLN)"),
                CategoryId = ConsoleMenu.ReadInt("ID kategorii"),
                BranchId = ConsoleMenu.ReadInt("ID oddziału"),
                Color = ConsoleMenu.ReadInput("Kolor"),
                Seats = ConsoleMenu.ReadInt("Liczba miejsc"),
                Mileage = ConsoleMenu.ReadInt("Przebieg (km)")
            };

            _carRepo.Insert(car);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✔ Samochód {car.FullName} dodany pomyślnie.");
            Console.ResetColor();
        }

        private void Edit()
        {
            int id = ConsoleMenu.ReadInt("ID samochodu do edycji");
            var car = _carRepo.GetById(id);
            if (car == null)
            {
                Console.WriteLine("Nie znaleziono samochodu.");
                return;
            }

            Console.WriteLine($"\nEdycja: {car}");
            Console.WriteLine("(wciśnij Enter, aby zachować obecną wartość)");

            string val;
            val = ConsoleMenu.ReadInput($"Marka [{car.Brand}]");
            if (!string.IsNullOrEmpty(val)) car.Brand = val;

            val = ConsoleMenu.ReadInput($"Model [{car.Model}]");
            if (!string.IsNullOrEmpty(val)) car.Model = val;

            val = ConsoleMenu.ReadInput($"Stawka dzienna [{car.DailyRate:F2}]");
            if (!string.IsNullOrEmpty(val)) car.DailyRate = decimal.Parse(val);

            val = ConsoleMenu.ReadInput($"Status [{car.Status}] (AVAILABLE/RENTED/IN_SERVICE/UNAVAILABLE)");
            if (!string.IsNullOrEmpty(val)) car.Status = val.ToUpper();

            val = ConsoleMenu.ReadInput($"Przebieg [{car.Mileage}]");
            if (!string.IsNullOrEmpty(val)) car.Mileage = int.Parse(val);

            _carRepo.Update(car);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✔ Samochód {car.FullName} zaktualizowany.");
            Console.ResetColor();
        }

        private void Delete()
        {
            int id = ConsoleMenu.ReadInt("ID samochodu do usunięcia");
            var car = _carRepo.GetById(id);
            if (car == null)
            {
                Console.WriteLine("Nie znaleziono samochodu.");
                return;
            }

            Console.Write($"Czy na pewno usunąć {car.FullName}? (t/n): ");
            if (Console.ReadLine()?.Trim().ToLower() == "t")
            {
                _carRepo.Delete(id);
                Console.WriteLine("Samochód usunięty.");
            }
        }

        private void ShowCategories()
        {
            var categories = _carRepo.GetCategories();
            Console.WriteLine("\n  Kategorie:");
            foreach (var c in categories)
                Console.WriteLine($"    {c}");
        }

        private void ShowBranches()
        {
            var branches = _branchRepo.GetAll();
            Console.WriteLine("\n  Oddziały:");
            foreach (var b in branches)
                Console.WriteLine($"    {b}");
        }
    }
}
