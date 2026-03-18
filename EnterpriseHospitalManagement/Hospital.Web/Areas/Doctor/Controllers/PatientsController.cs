using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class PatientsController : Controller
    {
        private readonly IPatientReportService _reports;
        private readonly IApplicationUserService _users;
        private readonly IMedicineService _meds;
        public PatientsController(IPatientReportService r, IApplicationUserService u, IMedicineService m)
        { _reports = r; _users = u; _meds = m; }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index(int page = 1, int size = 10) =>
            View(_reports.GetByDoctor(UserId, page, size));

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Patients = new SelectList(_users.GetAllPatients(1, 200).Items, "Id", "Name");
            ViewBag.Medicines = new SelectList(_meds.GetAll(1, 200).Items, "Id", "Name");
            return View(new PatientReportViewModel { DoctorId = UserId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(PatientReportViewModel vm)
        {
            vm.DoctorId = UserId;
            if (!ModelState.IsValid)
            {
                ViewBag.Patients = new SelectList(_users.GetAllPatients(1, 200).Items, "Id", "Name");
                ViewBag.Medicines = new SelectList(_meds.GetAll(1, 200).Items, "Id", "Name");
                return View(vm);
            }
            _reports.Insert(vm);
            TempData["success"] = "Patient report created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var vm = _reports.GetById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }
    }
}
