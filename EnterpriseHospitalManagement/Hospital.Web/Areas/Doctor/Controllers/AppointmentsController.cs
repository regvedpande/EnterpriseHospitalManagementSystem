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
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _svc;
        private readonly IApplicationUserService _users;
        public AppointmentsController(IAppointmentService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index(int page = 1, int size = 10) =>
            View(_svc.GetByDoctor(UserId, page, size));

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null || vm.DoctorId != UserId) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(AppointmentViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.Update(vm);
            TempData["success"] = "Appointment updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
