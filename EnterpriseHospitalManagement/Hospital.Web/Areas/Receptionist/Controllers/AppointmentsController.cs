using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hospital.Web.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = WebSiteRoles.Website_Receptionist)]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _svc;
        private readonly IApplicationUserService _users;
        public AppointmentsController(IAppointmentService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new AppointmentViewModel { Number = "APT-" + DateTime.Now.Ticks.ToString()[^6..] });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(AppointmentViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(); return View(vm); }
            _svc.Insert(vm);
            TempData["success"] = "Appointment created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null) return NotFound();
            PopulateDropdowns();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(AppointmentViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(); return View(vm); }
            _svc.Update(vm);
            TempData["success"] = "Appointment updated.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns()
        {
            ViewBag.Doctors = new SelectList(_users.GetAllDoctors(1, 200).Items, "Id", "Name");
            ViewBag.Patients = new SelectList(_users.GetAllPatients(1, 200).Items, "Id", "Name");
        }
    }
}
