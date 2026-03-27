using Hospital.Models.Enums;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class HomeController : Controller
    {
        private readonly IAppointmentService    _appts;
        private readonly IApplicationUserService _users;
        private readonly IPatientReportService  _reports;

        public HomeController(
            IAppointmentService appts,
            IApplicationUserService users,
            IPatientReportService reports)
        {
            _appts   = appts;
            _users   = users;
            _reports = reports;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            var uid = UserId;

            // Load all appointments for this doctor (up to 500)
            var allAppts = _appts.GetByDoctor(uid, 1, 500).Items;

            ViewBag.TotalAppointments = allAppts.Count;
            ViewBag.PatientCount      = _users.GetAllPatients(1, 1).TotalCount;
            ViewBag.ReportCount       = _reports.GetByDoctor(uid, 1, 1).TotalCount;

            // Today's appointments
            var today = DateTime.Today;
            ViewBag.TodayCount = allAppts.Count(a => a.AppointmentDate.Date == today);

            // Appointment status breakdown for doughnut chart
            var scheduled   = allAppts.Count(a => a.Status == AppointmentStatus.Scheduled);
            var confirmed   = allAppts.Count(a => a.Status == AppointmentStatus.Confirmed);
            var inProgress  = allAppts.Count(a => a.Status == AppointmentStatus.InProgress);
            var completed   = allAppts.Count(a => a.Status == AppointmentStatus.Completed);
            var cancelled   = allAppts.Count(a => a.Status == AppointmentStatus.Cancelled);
            var noShow      = allAppts.Count(a => a.Status == AppointmentStatus.NoShow);

            ViewBag.ScheduledCount  = scheduled;
            ViewBag.ConfirmedCount  = confirmed;
            ViewBag.InProgressCount = inProgress;
            ViewBag.CompletedCount  = completed;
            ViewBag.CancelledCount  = cancelled;
            ViewBag.NoShowCount     = noShow;

            ViewBag.StatusLabels = JsonSerializer.Serialize(new[] { "Scheduled", "Confirmed", "In Progress", "Completed", "Cancelled", "No Show" });
            ViewBag.StatusCounts = JsonSerializer.Serialize(new[] { scheduled, confirmed, inProgress, completed, cancelled, noShow });

            // Monthly appointment trend (last 6 months)
            var months = new List<string>();
            var monthlyCounts = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var m = DateTime.Now.AddMonths(-i);
                months.Add(m.ToString("MMM"));
                monthlyCounts.Add(allAppts.Count(a =>
                    a.AppointmentDate.Year == m.Year &&
                    a.AppointmentDate.Month == m.Month));
            }
            ViewBag.MonthLabels  = JsonSerializer.Serialize(months);
            ViewBag.MonthlyAppts = JsonSerializer.Serialize(monthlyCounts);

            // Upcoming appointments (next 7 days)
            ViewBag.UpcomingAppointments = allAppts
                .Where(a => a.AppointmentDate >= DateTime.Now
                    && a.AppointmentDate <= DateTime.Now.AddDays(7)
                    && a.Status != AppointmentStatus.Cancelled)
                .OrderBy(a => a.AppointmentDate)
                .Take(8)
                .ToList();

            // Recent reports
            ViewBag.RecentReports = _reports.GetByDoctor(uid, 1, 5).Items;

            return View();
        }
    }
}
