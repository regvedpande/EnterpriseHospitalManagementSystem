using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.Web.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = WebSiteRoles.Website_Patient)]
    public class HomeController : Controller
    {
        private readonly IHospitalInfoService   _hospitals;
        private readonly IDoctorService         _doctors;
        private readonly IAppointmentService    _appts;
        private readonly IBillService           _bills;
        private readonly ILabService            _labs;
        private readonly IPatientReportService  _reports;

        public HomeController(
            IHospitalInfoService h, IDoctorService d,
            IAppointmentService a, IBillService b,
            ILabService l, IPatientReportService r)
        {
            _hospitals = h; _doctors = d;
            _appts = a; _bills = b;
            _labs = l; _reports = r;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            var uid = UserId;

            ViewBag.HospitalCount    = _hospitals.GetAll(1, 1).TotalCount;
            ViewBag.DoctorCount      = _doctors.GetAll(1, 1).TotalCount;
            ViewBag.AppointmentCount = _appts.GetByPatient(uid, 1, 1).TotalCount;
            ViewBag.BillCount        = _bills.GetByPatient(uid, 1, 1).TotalCount;
            ViewBag.LabCount         = _labs.GetByPatient(uid, 1, 1).TotalCount;
            ViewBag.ReportCount      = _reports.GetByPatient(uid, 1, 1).TotalCount;

            ViewBag.RecentAppointments = _appts.GetByPatient(uid, 1, 5).Items;
            ViewBag.RecentBills        = _bills.GetByPatient(uid, 1, 4).Items;
            ViewBag.Doctors            = _doctors.GetAll(1, 6).Items;
            ViewBag.Hospitals          = _hospitals.GetAll(1, 4).Items;

            return View();
        }
    }
}
