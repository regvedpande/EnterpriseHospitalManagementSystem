using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.Web.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = WebSiteRoles.Website_Patient)]
    public class ReportsController : Controller
    {
        private readonly IPatientReportService _svc;
        public ReportsController(IPatientReportService svc) => _svc = svc;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index(int page = 1, int size = 10) =>
            View(_svc.GetByPatient(UserId, page, size));

        public IActionResult Details(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null || vm.PatientId != UserId) return NotFound();
            return View(vm);
        }
    }
}
