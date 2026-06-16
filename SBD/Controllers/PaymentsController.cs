using Microsoft.AspNetCore.Mvc;
using SBD.Models;
using SBD.Repositories;
using SBD.Services;
using System;
using System.Collections.Generic;

namespace SBD.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly PaymentRepository _paymentRepo = new PaymentRepository();
        private readonly PenaltyRepository _penaltyRepo = new PenaltyRepository();
        private readonly RentalRepository _rentalRepo = new RentalRepository();
        private readonly ClientRepository _clientRepo = new ClientRepository();
        
        private readonly PaymentService _paymentService = new PaymentService();
        private readonly PenaltyService _penaltyService = new PenaltyService();

        public IActionResult Index()
        {
            ViewBag.Payments = _paymentRepo.GetAll();
            ViewBag.Penalties = _penaltyRepo.GetAll();
            ViewBag.Rentals = _rentalRepo.GetAll(); // do formularzy płatności/rat
            ViewBag.Clients = _clientRepo.GetAll(); // do formularza nakładania kar

            return View();
        }

        [HttpPost]
        public IActionResult ProcessPayment(int rentalId, decimal amount, string paymentMethod)
        {
            try
            {
                _paymentService.ProcessPayment(rentalId, amount, paymentMethod);
                TempData["SuccessMessage"] = $"Zaksięgowano wpłatę {amount:F2} PLN dla wypożyczenia #{rentalId}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Błąd procesowania płatności: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CreateInstallments(int rentalId, int installments)
        {
            try
            {
                _paymentService.CreateInstallmentPlan(rentalId, installments);
                TempData["SuccessMessage"] = $"Utworzono plan ratalny ({installments} rat) dla wypożyczenia #{rentalId}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Nie udało się utworzyć planu ratalnego: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult PayInstallment(int id)
        {
            try
            {
                _paymentService.PayInstallment(id);
                TempData["SuccessMessage"] = $"Rata #{id} została pomyślnie opłacona.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Błąd podczas opłacania raty: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AddPenalty(int clientId, int? rentalId, string reason, decimal amount)
        {
            try
            {
                _penaltyService.AddPenalty(clientId, rentalId, reason, amount);
                TempData["SuccessMessage"] = $"Kara w wysokości {amount:F2} PLN została pomyślnie naliczona.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Błąd podczas naliczania kary: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
