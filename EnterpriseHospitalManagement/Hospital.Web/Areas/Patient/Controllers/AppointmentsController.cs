using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Hospital.Web.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = WebSiteRoles.Website_Patient)]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _svc;
        private readonly IApplicationUserService _users;
        public AppointmentsController(IAppointmentService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index(int page = 1, int size = 10) =>
            View(_svc.GetByPatient(UserId, page, size));

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Doctors = new SelectList(_users.GetAllDoctors(1, 200).Items, "Id", "Name");
            return View(new AppointmentViewModel
            {
                PatientId = UserId,
                Number = "APT-" + DateTime.Now.Ticks.ToString()[^6..]
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(AppointmentViewModel vm)
        {
            vm.PatientId = UserId;
            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = new SelectList(_users.GetAllDoctors(1, 200).Items, "Id", "Name");
                return View(vm);
            }
            _svc.Insert(vm);
            TempData["success"] = "Appointment booked successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
