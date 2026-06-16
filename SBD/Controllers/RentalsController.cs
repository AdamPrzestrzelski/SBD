using Microsoft.AspNetCore.Mvc;
using SBD.Models;
using SBD.Repositories;
using SBD.Services;
using System;
using System.Collections.Generic;

namespace SBD.Controllers
{
    public class RentalsController : Controller
    {
        private readonly RentalRepository _rentalRepo = new RentalRepository();
        private readonly CarRepository _carRepo = new CarRepository();
        private readonly ClientRepository _clientRepo = new ClientRepository();
        private readonly RentalService _rentalService = new RentalService();

        public IActionResult Index(bool showAll = false)
        {
            List<Rental> rentals = showAll ? _rentalRepo.GetAll() : _rentalRepo.GetActive();
            ViewBag.ShowAll = showAll;
            return View(rentals);
        }

        public IActionResult Create()
        {
            ViewBag.Clients = _clientRepo.GetAll();
            ViewBag.Cars = _carRepo.GetAvailable();
            return View();
        }

        [HttpPost]
        public IActionResult Create(int clientId, int carId, DateTime startDate, DateTime endDate)
        {
            try
            {
                int rentalId = _rentalService.CreateRental(clientId, carId, startDate, endDate);
                TempData["SuccessMessage"] = $"Wypożyczenie #{rentalId} zostało utworzone pomyślnie!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Nie udało się utworzyć wypożyczenia: {ex.Message}");
            }

            ViewBag.Clients = _clientRepo.GetAll();
            ViewBag.Cars = _carRepo.GetAvailable();
            return View();
        }

        [HttpPost]
        public IActionResult Extend(int rentalId, DateTime newEndDate)
        {
            try
            {
                _rentalService.ExtendRental(rentalId, newEndDate);
                TempData["SuccessMessage"] = $"Wypożyczenie #{rentalId} zostało przedłużone do {newEndDate:yyyy-MM-dd}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Błąd przedłużenia wypożyczenia: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Complete(int id)
        {
            try
            {
                // Przed zatwierdzeniem pobierzmy dane, żeby sprawdzić ewentualne spóźnienie
                var rental = _rentalRepo.GetById(id);
                _rentalService.CompleteRental(id);
                
                string msg = $"Wypożyczenie #{id} zostało pomyślnie zakończone. Samochód zwrócony.";
                if (rental != null && DateTime.Today > rental.EndDate)
                {
                    int daysLate = (DateTime.Today - rental.EndDate).Days;
                    msg += $" Uwaga: Zwrot opóźniony o {daysLate} dni! Naliczono karę za opóźnienie w bazie danych.";
                }

                TempData["SuccessMessage"] = msg;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Błąd podczas zwrotu samochodu: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            try
            {
                _rentalService.CancelRental(id);
                TempData["SuccessMessage"] = $"Wypożyczenie #{id} zostało anulowane.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Błąd podczas anulowania: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
