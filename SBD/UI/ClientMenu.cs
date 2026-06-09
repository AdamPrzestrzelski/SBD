using System;
using System.Collections.Generic;
using SBD.Models;
using SBD.Repositories;
using SBD.Services;

namespace SBD.UI
{
    /// <summary>
    /// Menu zarządzania klientami.
    /// </summary>
    public class ClientMenu
    {
        private readonly ClientRepository _clientRepo = new ClientRepository();
        private readonly PenaltyService _penaltyService = new PenaltyService();
        private readonly PenaltyRepository _penaltyRepo = new PenaltyRepository();

        public void Show()
        {
            bool running = true;
            while (running)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n═══════════ KLIENCI ═══════════");
                Console.ResetColor();
                Console.WriteLine("  1. Lista wszystkich klientów");
                Console.WriteLine("  2. Wyszukaj klienta");
                Console.WriteLine("  3. Dodaj klienta");
                Console.WriteLine("  4. Edytuj klienta");
                Console.WriteLine("  5. Usuń klienta");
                Console.WriteLine("  6. Zablokuj klienta");
                Console.WriteLine("  7. Odblokuj klienta");
                Console.WriteLine("  8. Lista zablokowanych");
                Console.WriteLine("  9. Kary klienta");
                Console.WriteLine("  10. Nalicz karę");
                Console.WriteLine("  0. Powrót");
                Console.WriteLine("═══════════════════════════════");

                string choice = ConsoleMenu.ReadChoice();

                try
                {
                    switch (choice)
                    {
                        case "1": ListAll(); break;
                        case "2": Search(); break;
                        case "3": Add(); break;
                        case "4": Edit(); break;
                        case "5": Delete(); break;
                        case "6": Block(); break;
                        case "7": Unblock(); break;
                        case "8": ListBlocked(); break;
                        case "9": ShowPenalties(); break;
                        case "10": AddPenalty(); break;
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
            var clients = _clientRepo.GetAll();
            Console.WriteLine($"\n--- Lista klientów ({clients.Count}) ---");
            foreach (var c in clients)
                Console.WriteLine($"  {c}");
        }

        private void Search()
        {
            string term = ConsoleMenu.ReadInput("Szukaj (imię/nazwisko/email)");
            var clients = _clientRepo.Search(term);
            Console.WriteLine($"\n--- Wyniki wyszukiwania ({clients.Count}) ---");
            foreach (var c in clients)
                Console.WriteLine($"  {c}");
        }

        private void Add()
        {
            Console.WriteLine("\n--- Dodaj nowego klienta ---");
            var client = new Client
            {
                FirstName = ConsoleMenu.ReadInput("Imię"),
                LastName = ConsoleMenu.ReadInput("Nazwisko"),
                Email = ConsoleMenu.ReadInput("Email"),
                Phone = ConsoleMenu.ReadInput("Telefon"),
                Pesel = ConsoleMenu.ReadInput("PESEL"),
                Address = ConsoleMenu.ReadInput("Adres"),
                City = ConsoleMenu.ReadInput("Miasto"),
                PostalCode = ConsoleMenu.ReadInput("Kod pocztowy"),
                PasswordHash = "default_hash"
            };

            int id = _clientRepo.Insert(client);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✔ Klient {client.FullName} dodany pomyślnie (ID: {id}).");
            Console.ResetColor();
        }

        private void Edit()
        {
            int id = ConsoleMenu.ReadInt("ID klienta do edycji");
            var client = _clientRepo.GetById(id);
            if (client == null)
            {
                Console.WriteLine("Nie znaleziono klienta.");
                return;
            }

            Console.WriteLine($"\nEdycja klienta: {client}");
            Console.WriteLine("(wciśnij Enter, aby zachować obecną wartość)");

            string val;
            val = ConsoleMenu.ReadInput($"Imię [{client.FirstName}]");
            if (!string.IsNullOrEmpty(val)) client.FirstName = val;

            val = ConsoleMenu.ReadInput($"Nazwisko [{client.LastName}]");
            if (!string.IsNullOrEmpty(val)) client.LastName = val;

            val = ConsoleMenu.ReadInput($"Email [{client.Email}]");
            if (!string.IsNullOrEmpty(val)) client.Email = val;

            val = ConsoleMenu.ReadInput($"Telefon [{client.Phone}]");
            if (!string.IsNullOrEmpty(val)) client.Phone = val;

            val = ConsoleMenu.ReadInput($"Adres [{client.Address}]");
            if (!string.IsNullOrEmpty(val)) client.Address = val;

            val = ConsoleMenu.ReadInput($"Miasto [{client.City}]");
            if (!string.IsNullOrEmpty(val)) client.City = val;

            _clientRepo.Update(client);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✔ Klient {client.FullName} zaktualizowany.");
            Console.ResetColor();
        }

        private void Delete()
        {
            int id = ConsoleMenu.ReadInt("ID klienta do usunięcia");
            var client = _clientRepo.GetById(id);
            if (client == null)
            {
                Console.WriteLine("Nie znaleziono klienta.");
                return;
            }

            Console.Write($"Czy na pewno usunąć klienta {client.FullName}? (t/n): ");
            if (Console.ReadLine()?.Trim().ToLower() == "t")
            {
                _clientRepo.Delete(id);
                Console.WriteLine("Klient usunięty.");
            }
        }

        private void Block()
        {
            int id = ConsoleMenu.ReadInt("ID klienta do zablokowania");
            _penaltyService.BlockClient(id);
        }

        private void Unblock()
        {
            int id = ConsoleMenu.ReadInt("ID klienta do odblokowania");
            _penaltyService.UnblockClient(id);
        }

        private void ListBlocked()
        {
            var clients = _clientRepo.GetBlockedClients();
            Console.WriteLine($"\n--- Zablokowani klienci ({clients.Count}) ---");
            foreach (var c in clients)
                Console.WriteLine($"  {c}");
        }

        private void ShowPenalties()
        {
            int id = ConsoleMenu.ReadInt("ID klienta");
            var penalties = _penaltyService.GetClientPenalties(id);
            var client = _clientRepo.GetById(id);

            Console.WriteLine($"\n--- Kary klienta {client?.FullName ?? $"#{id}"} ({penalties.Count}) ---");
            foreach (var p in penalties)
                Console.WriteLine($"  {p}");

            if (penalties.Count > 0)
            {
                decimal total = _penaltyRepo.GetTotalPenaltyAmount(id);
                Console.WriteLine($"\n  Łączna kwota kar: {total:F2} PLN");
            }
        }

        private void AddPenalty()
        {
            int clientId = ConsoleMenu.ReadInt("ID klienta");
            string rentalIdStr = ConsoleMenu.ReadInput("ID wypożyczenia (Enter = brak)");
            int? rentalId = string.IsNullOrEmpty(rentalIdStr) ? (int?)null : int.Parse(rentalIdStr);
            string reason = ConsoleMenu.ReadInput("Powód kary");
            decimal amount = ConsoleMenu.ReadDecimal("Kwota kary (PLN)");

            _penaltyService.AddPenalty(clientId, rentalId, reason, amount);
        }
    }
}
