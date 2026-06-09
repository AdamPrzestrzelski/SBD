using System;
using System.Collections.Generic;
using SBD.Models;
using SBD.Repositories;

namespace SBD.Services
{
    /// <summary>
    /// Serwis zarządzający płatnościami.
    /// </summary>
    public class PaymentService
    {
        private readonly PaymentRepository _paymentRepo = new PaymentRepository();
        private readonly RentalRepository _rentalRepo = new RentalRepository();

        /// <summary>
        /// Przetwarza jednorazową płatność za wypożyczenie.
        /// </summary>
        public void ProcessPayment(int rentalId, decimal amount, string paymentMethod = "CARD")
        {
            var rental = _rentalRepo.GetById(rentalId);
            if (rental == null)
                throw new Exception("Nie znaleziono wypożyczenia o podanym ID.");

            if (amount <= 0)
                throw new Exception("Kwota płatności musi być dodatnia.");

            _paymentRepo.ProcessPayment(rentalId, amount, paymentMethod);
            Console.WriteLine($"Płatność {amount:F2} PLN za wypożyczenie #{rentalId} przetworzona ({paymentMethod}).");
        }

        /// <summary>
        /// Tworzy plan ratalny dla wypożyczenia.
        /// </summary>
        public void CreateInstallmentPlan(int rentalId, int numberOfInstallments)
        {
            var rental = _rentalRepo.GetById(rentalId);
            if (rental == null)
                throw new Exception("Nie znaleziono wypożyczenia o podanym ID.");

            if (numberOfInstallments < 2 || numberOfInstallments > 12)
                throw new Exception("Liczba rat musi być od 2 do 12.");

            _paymentRepo.CreateInstallmentPlan(rentalId, numberOfInstallments);
            Console.WriteLine($"Plan ratalny ({numberOfInstallments} rat po ~{(rental.TotalPrice / numberOfInstallments):F2} PLN) " +
                              $"utworzony dla wypożyczenia #{rentalId}.");
        }

        /// <summary>
        /// Opłaca oczekującą ratę.
        /// </summary>
        public void PayInstallment(int paymentId)
        {
            _paymentRepo.MarkAsPaid(paymentId);
            Console.WriteLine($"Rata #{paymentId} została opłacona.");
        }

        /// <summary>
        /// Wyświetla oczekujące płatności.
        /// </summary>
        public List<Payment> GetPendingPayments()
        {
            return _paymentRepo.GetPending();
        }
    }
}
