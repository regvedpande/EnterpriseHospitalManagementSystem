using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _svc;
        private readonly IReportService _reports;

        public DoctorsController(IDoctorService svc, IReportService reports)
        { _svc = svc; _reports = reports; }

        public IActionResult Index(int page = 1, int size = 10)
        {
            // Adapt to whatever method your IDoctorService actually exposes.
            // Common names: GetAll / GetTimings / GetAllTimings
            // If your service returns TimingViewModel, it still works because
            // DoctorViewModel has the same fields — just change the cast below if needed.
            var result = _svc.GetAll(page, size);
            return View(result);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateHospitals();
            return View(new DoctorViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(DoctorViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateHospitals(); return View(vm); }
            _svc.InsertDoctor(vm);
            TempData["success"] = "Timing created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetDoctorById(id);
            if (vm == null) return NotFound();
            PopulateHospitals();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(DoctorViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateHospitals(); return View(vm); }
            _svc.UpdateDoctor(vm);
            TempData["success"] = "Updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.DeleteDoctor(id);
            TempData["success"] = "Deleted.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DownloadPdf()
            => File(_reports.GenerateDoctorsPdf(), "application/pdf", "doctors.pdf");

        public IActionResult DownloadExcel()
            => File(_reports.GenerateDoctorsExcel(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "doctors.xlsx");

        private void PopulateHospitals()
        {
            // Populate hospital dropdown — adjust if your service method differs
            // ViewBag.Hospitals = new SelectList(_hospitalSvc.GetAll(1,1000).Items, "Id", "Name");
        }
    }
}