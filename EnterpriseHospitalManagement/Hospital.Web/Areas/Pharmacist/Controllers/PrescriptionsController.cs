using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Pharmacist.Controllers
{
    [Area("Pharmacist")]
    [Authorize(Roles = WebSiteRoles.Website_Pharmacist)]
    public class PrescriptionsController : Controller
    {
        private readonly IPatientReportService _svc;
        public PrescriptionsController(IPatientReportService svc) => _svc = svc;

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        public IActionResult Details(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }
    }
}
