using System;
using System.Collections.Generic;
using SBD.Models;
using SBD.Repositories;
using SBD.Services;

namespace SBD.UI
{
    /// <summary>
    /// Menu zarządzania wypożyczeniami i rezerwacjami.
    /// </summary>
    public class RentalMenu
    {
        private readonly RentalRepository _rentalRepo = new RentalRepository();
        private readonly ReservationRepository _reservationRepo = new ReservationRepository();
        private readonly PaymentRepository _paymentRepo = new PaymentRepository();
        private readonly RentalService _rentalService = new RentalService();
        private readonly PaymentService _paymentService = new PaymentService();

        public void Show()
        {
            bool running = true;
            while (running)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n═══════════ WYPOŻYCZENIA ═══════════");
                Console.ResetColor();
                Console.WriteLine("  --- Wypożyczenia ---");
                Console.WriteLine("  1. Wszystkie wypożyczenia");
                Console.WriteLine("  2. Aktywne wypożyczenia");
                Console.WriteLine("  3. Wypożyczenia klienta");
                Console.WriteLine("  4. Utwórz wypożyczenie");
                Console.WriteLine("  5. Zakończ wypożyczenie (zwrot)");
                Console.WriteLine("  6. Przedłuż wypożyczenie");
                Console.WriteLine("  7. Anuluj wypożyczenie");
                Console.WriteLine("  --- Rezerwacje ---");
                Console.WriteLine("  8. Wszystkie rezerwacje");
                Console.WriteLine("  9. Utwórz rezerwację");
                Console.WriteLine("  --- Płatności ---");
                Console.WriteLine("  10. Płatności za wypożyczenie");
                Console.WriteLine("  11. Opłać wypożyczenie");
                Console.WriteLine("  12. Utwórz plan ratalny");
                Console.WriteLine("  13. Oczekujące płatności");
                Console.WriteLine("  14. Opłać ratę");
                Console.WriteLine("  0. Powrót");
                Console.WriteLine("════════════════════════════════════");

                string choice = ConsoleMenu.ReadChoice();

                try
                {
                    switch (choice)
                    {
                        case "1": ListAllRentals(); break;
                        case "2": ListActiveRentals(); break;
                        case "3": ListClientRentals(); break;
                        case "4": CreateRental(); break;
                        case "5": CompleteRental(); break;
                        case "6": ExtendRental(); break;
                        case "7": CancelRental(); break;
                        case "8": ListReservations(); break;
                        case "9": CreateReservation(); break;
                        case "10": ShowPayments(); break;
                        case "11": ProcessPayment(); break;
                        case "12": CreateInstallmentPlan(); break;
                        case "13": ShowPendingPayments(); break;
                        case "14": PayInstallment(); break;
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

        // === WYPOŻYCZENIA ===

        private void ListAllRentals()
        {
            var rentals = _rentalRepo.GetAll();
            Console.WriteLine($"\n--- Wszystkie wypożyczenia ({rentals.Count}) ---");
            foreach (var r in rentals)
                Console.WriteLine($"  {r}");
        }

        private void ListActiveRentals()
        {
            var rentals = _rentalRepo.GetActive();
            Console.WriteLine($"\n--- Aktywne wypożyczenia ({rentals.Count}) ---");
            foreach (var r in rentals)
                Console.WriteLine($"  {r}");
        }

        private void ListClientRentals()
        {
            int clientId = ConsoleMenu.ReadInt("ID klienta");
            var rentals = _rentalRepo.GetByClientId(clientId);
            Console.WriteLine($"\n--- Wypożyczenia klienta #{clientId} ({rentals.Count}) ---");
            foreach (var r in rentals)
                Console.WriteLine($"  {r}");
        }

        private void CreateRental()
        {
            Console.WriteLine("\n--- Utwórz nowe wypożyczenie ---");
            int clientId = ConsoleMenu.ReadInt("ID klienta");
            int carId = ConsoleMenu.ReadInt("ID samochodu");
            DateTime startDate = ConsoleMenu.ReadDate("Data rozpoczęcia");
            DateTime endDate = ConsoleMenu.ReadDate("Data zakończenia");

            _rentalService.CreateRental(clientId, carId, startDate, endDate);
        }

        private void CompleteRental()
        {
            int rentalId = ConsoleMenu.ReadInt("ID wypożyczenia do zakończenia");
            _rentalService.CompleteRental(rentalId);
        }

        private void ExtendRental()
        {
            int rentalId = ConsoleMenu.ReadInt("ID wypożyczenia do przedłużenia");
            DateTime newEndDate = ConsoleMenu.ReadDate("Nowa data zakończenia");
            _rentalService.ExtendRental(rentalId, newEndDate);
        }

        private void CancelRental()
        {
            int rentalId = ConsoleMenu.ReadInt("ID wypożyczenia do anulowania");

            Console.Write("Czy na pewno anulować to wypożyczenie? (t/n): ");
            if (Console.ReadLine()?.Trim().ToLower() == "t")
            {
                _rentalService.CancelRental(rentalId);
            }
        }

        // === REZERWACJE ===

        private void ListReservations()
        {
            var reservations = _reservationRepo.GetAll();
            Console.WriteLine($"\n--- Wszystkie rezerwacje ({reservations.Count}) ---");
            foreach (var r in reservations)
                Console.WriteLine($"  {r}");
        }

        private void CreateReservation()
        {
            Console.WriteLine("\n--- Utwórz nową rezerwację ---");
            int clientId = ConsoleMenu.ReadInt("ID klienta");
            int carId = ConsoleMenu.ReadInt("ID samochodu");
            DateTime startDate = ConsoleMenu.ReadDate("Data rozpoczęcia");
            DateTime endDate = ConsoleMenu.ReadDate("Data zakończenia");
            string notes = ConsoleMenu.ReadInput("Notatki (opcjonalnie)");

            var reservation = new Reservation
            {
                ClientId = clientId,
                CarId = carId,
                StartDate = startDate,
                EndDate = endDate,
                StatusId = 1, // ACTIVE
                Notes = string.IsNullOrEmpty(notes) ? null : notes
            };

            _reservationRepo.Insert(reservation);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✔ Rezerwacja utworzona pomyślnie.");
            Console.ResetColor();
        }

        // === PŁATNOŚCI ===

        private void ShowPayments()
        {
            int rentalId = ConsoleMenu.ReadInt("ID wypożyczenia");
            var payments = _paymentRepo.GetByRentalId(rentalId);
            Console.WriteLine($"\n--- Płatności za wypożyczenie #{rentalId} ({payments.Count}) ---");
            foreach (var p in payments)
                Console.WriteLine($"  {p}");
        }

        private void ProcessPayment()
        {
            int rentalId = ConsoleMenu.ReadInt("ID wypożyczenia");
            decimal amount = ConsoleMenu.ReadDecimal("Kwota (PLN)");
            string method = ConsoleMenu.ReadInput("Metoda płatności (CARD/TRANSFER/CASH)");
            if (string.IsNullOrEmpty(method)) method = "CARD";

            _paymentService.ProcessPayment(rentalId, amount, method.ToUpper());
        }

        private void CreateInstallmentPlan()
        {
            int rentalId = ConsoleMenu.ReadInt("ID wypożyczenia");
            int installments = ConsoleMenu.ReadInt("Liczba rat (2-12)");

            _paymentService.CreateInstallmentPlan(rentalId, installments);
        }

        private void ShowPendingPayments()
        {
            var payments = _paymentService.GetPendingPayments();
            Console.WriteLine($"\n--- Oczekujące płatności ({payments.Count}) ---");
            foreach (var p in payments)
                Console.WriteLine($"  {p}");
        }

        private void PayInstallment()
        {
            int paymentId = ConsoleMenu.ReadInt("ID płatności/raty");
            _paymentService.PayInstallment(paymentId);
        }
    }
}
