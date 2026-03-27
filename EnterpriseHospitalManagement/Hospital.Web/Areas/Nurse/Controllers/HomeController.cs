using Hospital.Models.Enums;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;

namespace Hospital.Web.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    [Authorize(Roles = WebSiteRoles.Website_Nurse)]
    public class HomeController : Controller
    {
        private readonly IAppointmentService    _appts;
        private readonly IApplicationUserService _users;
        private readonly IRoomService            _rooms;
        public HomeController(IAppointmentService a, IApplicationUserService u, IRoomService r)
        { _appts = a; _users = u; _rooms = r; }

        public IActionResult Index()
        {
            var allAppts = _appts.GetAll(1, 1000).Items;

            ViewBag.AppointmentCount = allAppts.Count;
            ViewBag.PatientCount     = _users.GetAllPatients(1, 1).TotalCount;
            ViewBag.RoomCount        = _rooms.GetAll(1, 1).TotalCount;

            // Status counts
            ViewBag.ScheduledCount  = allAppts.Count(a => a.Status == AppointmentStatus.Scheduled);
            ViewBag.ConfirmedCount  = allAppts.Count(a => a.Status == AppointmentStatus.Confirmed);
            ViewBag.InProgressCount = allAppts.Count(a => a.Status == AppointmentStatus.InProgress);
            ViewBag.CompletedCount  = allAppts.Count(a => a.Status == AppointmentStatus.Completed);
            ViewBag.CancelledCount  = allAppts.Count(a => a.Status == AppointmentStatus.Cancelled);
            ViewBag.NoShowCount     = allAppts.Count(a => a.Status == AppointmentStatus.NoShow);

            ViewBag.StatusLabels = JsonSerializer.Serialize(new[]
                { "Scheduled", "Confirmed", "In Progress", "Completed", "Cancelled", "No Show" });
            ViewBag.StatusData   = JsonSerializer.Serialize(new[]
            {
                allAppts.Count(a => a.Status == AppointmentStatus.Scheduled),
                allAppts.Count(a => a.Status == AppointmentStatus.Confirmed),
                allAppts.Count(a => a.Status == AppointmentStatus.InProgress),
                allAppts.Count(a => a.Status == AppointmentStatus.Completed),
                allAppts.Count(a => a.Status == AppointmentStatus.Cancelled),
                allAppts.Count(a => a.Status == AppointmentStatus.NoShow)
            });

            ViewBag.RecentAppointments = allAppts.Take(8).ToList();

            return View();
        }
    }
}
