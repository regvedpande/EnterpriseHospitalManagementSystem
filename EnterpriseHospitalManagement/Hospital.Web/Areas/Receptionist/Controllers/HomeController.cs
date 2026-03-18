using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = WebSiteRoles.Website_Receptionist)]
    public class HomeController : Controller
    {
        private readonly IAppointmentService _appts;
        private readonly IApplicationUserService _users;
        public HomeController(IAppointmentService a, IApplicationUserService u)
        { _appts = a; _users = u; }

        public IActionResult Index()
        {
            ViewBag.AppointmentCount = _appts.GetAll(1, 1).TotalCount;
            ViewBag.PatientCount = _users.GetAllPatients(1, 1).TotalCount;
            ViewBag.DoctorCount = _users.GetAllDoctors(1, 1).TotalCount;
            return View();
        }
    }
}
