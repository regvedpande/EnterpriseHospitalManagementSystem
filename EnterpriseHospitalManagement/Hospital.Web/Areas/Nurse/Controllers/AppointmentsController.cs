using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    [Authorize(Roles = WebSiteRoles.Website_Nurse)]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _svc;
        public AppointmentsController(IAppointmentService svc) => _svc = svc;

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(AppointmentViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.Update(vm);
            TempData["success"] = "Appointment status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
