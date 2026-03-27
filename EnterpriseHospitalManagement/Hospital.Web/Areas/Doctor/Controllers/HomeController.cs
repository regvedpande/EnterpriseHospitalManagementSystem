using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class HomeController : Controller
    {
        private readonly IAppointmentService    _appts;
        private readonly IApplicationUserService _users;
        private readonly IPatientReportService  _reports;
        private readonly ILabService            _labs;

        public HomeController(
            IAppointmentService     appts,
            IApplicationUserService users,
            IPatientReportService   reports,
            ILabService             labs)
        {
            _appts   = appts;
            _users   = users;
            _reports = reports;
            _labs    = labs;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            var uid = UserId;
            ViewBag.AppointmentCount  = _appts.GetByDoctor(uid, 1, 1).TotalCount;
            ViewBag.PatientCount      = _users.GetAllPatients(1, 1).TotalCount;
            ViewBag.ReportCount       = _reports.GetByDoctor(uid, 1, 1).TotalCount;
            ViewBag.LabCount          = _labs.GetAll(1, 1).TotalCount;
            ViewBag.RecentAppointments = _appts.GetByDoctor(uid, 1, 8).Items;
            ViewBag.RecentReports     = _reports.GetByDoctor(uid, 1, 5).Items;
            return View();
        }
    }
}
