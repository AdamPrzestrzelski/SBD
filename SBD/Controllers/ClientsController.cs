using Microsoft.AspNetCore.Mvc;
using SBD.Models;
using SBD.Repositories;
using SBD.Services;
using System;
using System.Collections.Generic;

namespace SBD.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ClientRepository _clientRepo = new ClientRepository();
        private readonly PenaltyService _penaltyService = new PenaltyService();

        public IActionResult Index(string search)
        {
            List<Client> clients;
            if (!string.IsNullOrEmpty(search))
            {
                clients = _clientRepo.Search(search);
            }
            else
            {
                clients = _clientRepo.GetAll();
            }

            ViewBag.SearchTerm = search;
            return View(clients);
        }

        public IActionResult Create()
        {
            return View(new Client());
        }

        [HttpPost]
        public IActionResult Create(Client client)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(client.FirstName) || string.IsNullOrWhiteSpace(client.LastName) || string.IsNullOrWhiteSpace(client.Email))
                {
                    ModelState.AddModelError("", "Imię, nazwisko oraz e-mail są wymagane.");
                }

                if (ModelState.IsValid)
                {
                    int id = _clientRepo.Insert(client);
                    TempData["SuccessMessage"] = $"Klient został zarejestrowany pomyślnie z ID: #{id}!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd rejestracji klienta: {ex.Message}");
            }

            return View(client);
        }

        public IActionResult Edit(int id)
        {
            var client = _clientRepo.GetById(id);
            if (client == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono klienta o podanym ID.";
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        [HttpPost]
        public IActionResult Edit(int id, Client client)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _clientRepo.Update(client);
                    TempData["SuccessMessage"] = "Dane klienta zostały zaktualizowane.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd zapisu zmian: {ex.Message}");
            }
            return View(client);
        }

        [HttpPost]
        public IActionResult Block(int id)
        {
            try
            {
                _penaltyService.BlockClient(id);
                TempData["SuccessMessage"] = "Klient został zablokowany.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Błąd podczas blokowania klienta: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Unblock(int id)
        {
            try
            {
                _penaltyService.UnblockClient(id);
                TempData["SuccessMessage"] = "Klient został odblokowany. Mnożnik kar zresetowany.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Błąd podczas odblokowywania klienta: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                _clientRepo.Delete(id);
                TempData["SuccessMessage"] = "Konto klienta zostało usunięte.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Nie można usunąć klienta: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
