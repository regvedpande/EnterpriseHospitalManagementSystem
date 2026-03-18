using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    [Authorize(Roles = WebSiteRoles.Website_Nurse)]
    public class HomeController : Controller
    {
        private readonly IAppointmentService _appts;
        private readonly IApplicationUserService _users;
        private readonly IRoomService _rooms;
        public HomeController(IAppointmentService a, IApplicationUserService u, IRoomService r)
        { _appts = a; _users = u; _rooms = r; }

        public IActionResult Index()
        {
            ViewBag.AppointmentCount = _appts.GetAll(1, 1).TotalCount;
            ViewBag.PatientCount = _users.GetAllPatients(1, 1).TotalCount;
            ViewBag.RoomCount = _rooms.GetAll(1, 1).TotalCount;
            return View();
        }
    }
}
