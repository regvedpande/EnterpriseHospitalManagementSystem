using Hospital.Models.Enums;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Hospital.Web.Areas.LabTech.Controllers
{
    [Area("LabTech")]
    [Authorize(Roles = WebSiteRoles.Website_LabTech)]
    public class HomeController : Controller
    {
        private readonly ILabService _labs;
        public HomeController(ILabService l) => _labs = l;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            var uid     = UserId;
            var allLabs = _labs.GetAll(1, 1000).Items;

            ViewBag.TotalTests  = allLabs.Count;
            ViewBag.MyTests     = allLabs.Count(l => l.TechnicianId == uid);

            // Status breakdown
            ViewBag.OrderedCount    = allLabs.Count(l => l.Status == LabTestStatus.Ordered);
            ViewBag.SampleCount     = allLabs.Count(l => l.Status == LabTestStatus.SampleCollected);
            ViewBag.InProgressCount = allLabs.Count(l => l.Status == LabTestStatus.InProgress);
            ViewBag.CompletedCount  = allLabs.Count(l => l.Status == LabTestStatus.Completed);
            ViewBag.CancelledCount  = allLabs.Count(l => l.Status == LabTestStatus.Cancelled);

            ViewBag.StatusLabels = JsonSerializer.Serialize(new[]
                { "Ordered", "Sample Collected", "In Progress", "Completed", "Cancelled" });
            ViewBag.StatusData   = JsonSerializer.Serialize(new[]
            {
                allLabs.Count(l => l.Status == LabTestStatus.Ordered),
                allLabs.Count(l => l.Status == LabTestStatus.SampleCollected),
                allLabs.Count(l => l.Status == LabTestStatus.InProgress),
                allLabs.Count(l => l.Status == LabTestStatus.Completed),
                allLabs.Count(l => l.Status == LabTestStatus.Cancelled)
            });

            // My recent tests
            ViewBag.RecentLabs = allLabs
                .Where(l => l.TechnicianId == uid || l.TechnicianId == null)
                .Take(8).ToList();

            return View();
        }
    }
}
