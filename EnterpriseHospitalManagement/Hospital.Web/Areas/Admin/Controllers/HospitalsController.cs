using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class HospitalsController : Controller
    {
        private readonly IHospitalInfoService _svc;
        private readonly IReportService _reports;
        public HospitalsController(IHospitalInfoService svc, IReportService reports) { _svc = svc; _reports = reports; }

        public IActionResult Index(int page = 1, int size = 10)
            => View(_svc.GetAll(page, size));

        [HttpGet] public IActionResult Create() => View(new HospitalInfoViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(HospitalInfoViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.InsertHospitalInfo(vm);
            TempData["success"] = "Hospital created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet] public IActionResult Edit(int id)
        {
            var vm = _svc.GetHospitalById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(HospitalInfoViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.UpdateHospitalInfo(vm);
            TempData["success"] = "Hospital updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.DeleteHospitalInfo(id);
            TempData["success"] = "Hospital deleted.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DownloadPdf()   => File(_reports.GenerateHospitalsPdf(),   "application/pdf",  "hospitals.pdf");
        public IActionResult DownloadExcel() => File(_reports.GenerateHospitalsExcel(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "hospitals.xlsx");
    }
}
