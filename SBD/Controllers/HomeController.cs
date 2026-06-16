using Microsoft.AspNetCore.Mvc;
using SBD.Repositories;
using System.Data;

namespace SBD.Controllers
{
    public class HomeController : Controller
    {
        private readonly ReportRepository _reportRepo = new ReportRepository();

        public IActionResult Index()
        {
            try
            {
                ViewBag.SystemSummary = _reportRepo.GetSystemSummary();
                ViewBag.MonthlyRevenue = _reportRepo.GetMonthlyRevenue();
                ViewBag.CarUtilization = _reportRepo.GetCarUtilization();
                ViewBag.HighRiskClients = _reportRepo.GetHighRiskClients();
                ViewBag.ChangeHistory = _reportRepo.GetChangeHistory(null, null, 15);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Błąd podczas ładowania raportów: {ex.Message}";
            }

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
