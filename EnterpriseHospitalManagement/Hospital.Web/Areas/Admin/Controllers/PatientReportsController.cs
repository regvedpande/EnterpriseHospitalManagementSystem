using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class PatientReportsController : Controller
    {
        private readonly IPatientReportService _svc;
        private readonly IApplicationUserService _users;
        public PatientReportsController(IPatientReportService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet]
        public IActionResult Details(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.Delete(id);
            TempData["success"] = "Report deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
