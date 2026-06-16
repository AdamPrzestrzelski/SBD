using Microsoft.AspNetCore.Mvc;
using SBD.Models;
using SBD.Repositories;
using SBD.Database;
using System;
using System.Collections.Generic;
using System.Data;

namespace SBD.Controllers
{
    public class CarsController : Controller
    {
        private readonly CarRepository _carRepo = new CarRepository();
        private readonly BranchRepository _branchRepo = new BranchRepository();

        public IActionResult Index(int? categoryId, int? branchId, string status)
        {
            List<Car> cars;

            if (categoryId.HasValue)
                cars = _carRepo.SearchByCategory(categoryId.Value);
            else if (branchId.HasValue)
                cars = _carRepo.SearchByBranch(branchId.Value);
            else
                cars = _carRepo.GetAll();

            if (!string.IsNullOrEmpty(status))
            {
                cars = cars.FindAll(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.Categories = GetCategoriesList();
            ViewBag.Branches = _branchRepo.GetAll();
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedBranch = branchId;
            ViewBag.SelectedStatus = status;

            return View(cars);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = GetCategoriesList();
            ViewBag.Branches = _branchRepo.GetAll();
            return View(new Car());
        }

        [HttpPost]
        public IActionResult Create(Car car)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(car.Brand) || string.IsNullOrWhiteSpace(car.Model) || string.IsNullOrWhiteSpace(car.PlateNumber) || string.IsNullOrWhiteSpace(car.Vin))
                {
                    ModelState.AddModelError("", "Wszystkie wymagane pola muszą być uzupełnione.");
                }

                if (ModelState.IsValid)
                {
                    _carRepo.Insert(car);
                    TempData["SuccessMessage"] = "Samochód został pomyślnie dodany do floty!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd podczas dodawania samochodu: {ex.Message}");
            }

            ViewBag.Categories = GetCategoriesList();
            ViewBag.Branches = _branchRepo.GetAll();
            return View(car);
        }

        public IActionResult Edit(int id)
        {
            var car = _carRepo.GetById(id);
            if (car == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono samochodu o podanym ID.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = GetCategoriesList();
            ViewBag.Branches = _branchRepo.GetAll();
            return View(car);
        }

        [HttpPost]
        public IActionResult Edit(int id, Car car)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _carRepo.Update(car);
                    TempData["SuccessMessage"] = $"Pomyślnie zaktualizowano samochód {car.Brand} {car.Model}!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Błąd zapisu zmian: {ex.Message}");
            }

            ViewBag.Categories = GetCategoriesList();
            ViewBag.Branches = _branchRepo.GetAll();
            return View(car);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                _carRepo.Delete(id);
                TempData["SuccessMessage"] = "Samochód został usunięty z floty.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Nie można usunąć samochodu: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private List<CarCategory> GetCategoriesList()
        {
            var list = new List<CarCategory>();
            var dt = DbConnection.Instance.ExecuteQuery("SELECT CATEGORY_ID, NAME, DESCRIPTION FROM CAR_CATEGORIES ORDER BY CATEGORY_ID");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new CarCategory
                {
                    CategoryId = Convert.ToInt32(row["CATEGORY_ID"]),
                    Name = row["NAME"].ToString(),
                    Description = row["DESCRIPTION"]?.ToString()
                });
            }
            return list;
        }
    }
}
