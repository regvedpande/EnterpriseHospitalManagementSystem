using Hospital.Models.Enums;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;

namespace Hospital.Web.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = WebSiteRoles.Website_Receptionist)]
    public class HomeController : Controller
    {
        private readonly IAppointmentService    _appts;
        private readonly IApplicationUserService _users;
        public HomeController(IAppointmentService a, IApplicationUserService u)
        { _appts = a; _users = u; }

        public IActionResult Index()
        {
            var allAppts = _appts.GetAll(1, 1000).Items;

            ViewBag.AppointmentCount = allAppts.Count;
            ViewBag.PatientCount     = _users.GetAllPatients(1, 1).TotalCount;
            ViewBag.DoctorCount      = _users.GetAllDoctors(1, 1).TotalCount;

            // Status breakdown
            ViewBag.ScheduledCount  = allAppts.Count(a => a.Status == AppointmentStatus.Scheduled);
            ViewBag.ConfirmedCount  = allAppts.Count(a => a.Status == AppointmentStatus.Confirmed);
            ViewBag.CompletedCount  = allAppts.Count(a => a.Status == AppointmentStatus.Completed);
            ViewBag.CancelledCount  = allAppts.Count(a => a.Status == AppointmentStatus.Cancelled);

            ViewBag.StatusLabels = JsonSerializer.Serialize(new[]
                { "Scheduled", "Confirmed", "Completed", "Cancelled" });
            ViewBag.StatusData   = JsonSerializer.Serialize(new[]
            {
                allAppts.Count(a => a.Status == AppointmentStatus.Scheduled),
                allAppts.Count(a => a.Status == AppointmentStatus.Confirmed),
                allAppts.Count(a => a.Status == AppointmentStatus.Completed),
                allAppts.Count(a => a.Status == AppointmentStatus.Cancelled)
            });

            ViewBag.RecentAppointments = allAppts.Take(8).ToList();

            return View();
        }
    }
}
