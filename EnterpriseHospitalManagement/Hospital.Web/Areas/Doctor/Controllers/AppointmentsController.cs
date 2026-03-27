using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Security.Claims;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService    _svc;
        private readonly IApplicationUserService _users;
        public AppointmentsController(IAppointmentService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index(int page = 1, int size = 10) =>
            View(_svc.GetByDoctor(UserId, page, size));

        [HttpGet]
        public IActionResult Create()
        {
            var uid = UserId;
            PopulatePatients();
            return View(new AppointmentViewModel
            {
                Number   = "APT-" + DateTime.Now.Ticks.ToString()[^6..],
                DoctorId = uid
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(AppointmentViewModel vm)
        {
            vm.DoctorId = UserId;
            if (!ModelState.IsValid) { PopulatePatients(); return View(vm); }
            _svc.Insert(vm);
            TempData["success"] = "Appointment created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null || vm.DoctorId != UserId) return NotFound();
            PopulatePatients();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(AppointmentViewModel vm)
        {
            if (!ModelState.IsValid) { PopulatePatients(); return View(vm); }
            _svc.Update(vm);
            TempData["success"] = "Appointment updated.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulatePatients()
        {
            var patients = _users.GetAllPatients(1, 200).Items;
            ViewBag.Patients = new SelectList(patients, "Id", "Name");
        }
    }
}
