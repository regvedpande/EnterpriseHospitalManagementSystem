using Hospital.Models.Enums;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class HomeController : Controller
    {
        private readonly IApplicationUserService _users;
        private readonly IAppointmentService     _appts;
        private readonly IBillService            _bills;
        private readonly ILabService             _labs;
        private readonly IHospitalInfoService    _hospitals;
        private readonly IRoomService            _rooms;
        private readonly IMedicineService        _medicines;
        private readonly IPayrollService         _payrolls;

        public HomeController(
            IApplicationUserService users,
            IAppointmentService appts,
            IBillService bills,
            ILabService labs,
            IHospitalInfoService hospitals,
            IRoomService rooms,
            IMedicineService medicines,
            IPayrollService payrolls)
        {
            _users     = users;
            _appts     = appts;
            _bills     = bills;
            _labs      = labs;
            _hospitals = hospitals;
            _rooms     = rooms;
            _medicines = medicines;
            _payrolls  = payrolls;
        }

        public IActionResult Index()
        {
            // ── Totals ────────────────────────────────────────────────────────
            ViewBag.PatientCount     = _users.GetAllPatients(1, 1).TotalCount;
            ViewBag.DoctorCount      = _users.GetAllDoctors(1, 1).TotalCount;
            ViewBag.AppointmentCount = _appts.GetAll(1, 1).TotalCount;
            ViewBag.BillCount        = _bills.GetAll(1, 1).TotalCount;
            ViewBag.LabCount         = _labs.GetAll(1, 1).TotalCount;
            ViewBag.HospitalCount    = _hospitals.GetAll(1, 1).TotalCount;
            ViewBag.RoomCount        = _rooms.GetAll(1, 1).TotalCount;
            ViewBag.MedicineCount    = _medicines.GetAll(1, 1).TotalCount;
            ViewBag.PayrollCount     = _payrolls.GetAll(1, 1).TotalCount;
            ViewBag.StaffCount       = _users.GetAll(1, 1).TotalCount;

            // ── Chart data: appointments per month (last 6 months) ────────────
            var allAppts = _appts.GetAll(1, 1000).Items;
            var months   = new List<string>();
            var apptByMonth = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var m = DateTime.Now.AddMonths(-i);
                months.Add(m.ToString("MMM yy"));
                apptByMonth.Add(allAppts.Count(a =>
                    a.AppointmentDate.Year == m.Year &&
                    a.AppointmentDate.Month == m.Month));
            }
            ViewBag.MonthLabels  = JsonSerializer.Serialize(months);
            ViewBag.ApptByMonth  = JsonSerializer.Serialize(apptByMonth);

            // ── Appointment status breakdown ──────────────────────────────────
            ViewBag.ApptScheduled  = allAppts.Count(a => a.Status == AppointmentStatus.Scheduled);
            ViewBag.ApptConfirmed  = allAppts.Count(a => a.Status == AppointmentStatus.Confirmed);
            ViewBag.ApptCompleted  = allAppts.Count(a => a.Status == AppointmentStatus.Completed);
            ViewBag.ApptCancelled  = allAppts.Count(a => a.Status == AppointmentStatus.Cancelled);

            ViewBag.ApptStatusLabels = JsonSerializer.Serialize(new[] { "Scheduled", "Confirmed", "Completed", "Cancelled", "In Progress", "No Show" });
            ViewBag.ApptStatusData   = JsonSerializer.Serialize(new[] {
                allAppts.Count(a => a.Status == AppointmentStatus.Scheduled),
                allAppts.Count(a => a.Status == AppointmentStatus.Confirmed),
                allAppts.Count(a => a.Status == AppointmentStatus.Completed),
                allAppts.Count(a => a.Status == AppointmentStatus.Cancelled),
                allAppts.Count(a => a.Status == AppointmentStatus.InProgress),
                allAppts.Count(a => a.Status == AppointmentStatus.NoShow)
            });

            // ── Bill status summary ────────────────────────────────────────────
            var allBills = _bills.GetAll(1, 1000).Items;
            ViewBag.PendingBills  = allBills.Count(b => b.Status == BillStatus.Pending);
            ViewBag.PaidBills     = allBills.Count(b => b.Status == BillStatus.Paid);
            ViewBag.OverdueBills  = allBills.Count(b => b.Status == BillStatus.Overdue);
            ViewBag.TotalRevenue  = allBills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.TotalBill);
            ViewBag.PendingAmount = allBills.Where(b => b.Status == BillStatus.Pending || b.Status == BillStatus.Overdue).Sum(b => b.TotalBill);

            // ── Bill status chart ─────────────────────────────────────────────
            ViewBag.BillStatusLabels = JsonSerializer.Serialize(new[] { "Pending", "Paid", "Overdue", "Partial", "Cancelled" });
            ViewBag.BillStatusData   = JsonSerializer.Serialize(new[] {
                allBills.Count(b => b.Status == BillStatus.Pending),
                allBills.Count(b => b.Status == BillStatus.Paid),
                allBills.Count(b => b.Status == BillStatus.Overdue),
                allBills.Count(b => b.Status == BillStatus.PartiallyPaid),
                allBills.Count(b => b.Status == BillStatus.Cancelled)
            });

            // ── Recent appointments (8) ────────────────────────────────────────
            ViewBag.RecentAppointments = allAppts
                .OrderByDescending(a => a.AppointmentDate)
                .Take(8)
                .ToList();

            return View();
        }
    }
}
