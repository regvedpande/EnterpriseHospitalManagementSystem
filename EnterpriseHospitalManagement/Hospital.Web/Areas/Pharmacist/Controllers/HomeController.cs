using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;

namespace Hospital.Web.Areas.Pharmacist.Controllers
{
    [Area("Pharmacist")]
    [Authorize(Roles = WebSiteRoles.Website_Pharmacist)]
    public class HomeController : Controller
    {
        private readonly IMedicineService      _meds;
        private readonly IPatientReportService _reports;
        public HomeController(IMedicineService m, IPatientReportService r)
        { _meds = m; _reports = r; }

        public IActionResult Index()
        {
            var allMeds    = _meds.GetAll(1, 1000).Items;
            var allReports = _reports.GetAll(1, 1000).Items;

            ViewBag.MedicineCount     = allMeds.Count;
            ViewBag.PrescriptionCount = allReports.Count;

            // Medicine by type for bar chart
            var byType = allMeds
                .GroupBy(m => string.IsNullOrEmpty(m.Type) ? "Other" : m.Type)
                .OrderByDescending(g => g.Count())
                .Take(8)
                .ToList();

            ViewBag.TypeLabels = JsonSerializer.Serialize(byType.Select(g => g.Key).ToArray());
            ViewBag.TypeCounts = JsonSerializer.Serialize(byType.Select(g => g.Count()).ToArray());

            // Low stock alert (cost < 5 as proxy for low-priced / flagged)
            ViewBag.LowStockCount = allMeds.Count(m => m.Cost < 5);

            // Recent reports (prescriptions)
            ViewBag.RecentReports   = allReports.Take(5).ToList();
            ViewBag.RecentMedicines = allMeds.Take(5).ToList();

            return View();
        }
    }
}
