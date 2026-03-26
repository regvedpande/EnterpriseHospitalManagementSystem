using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class HomeController : Controller
    {
        private readonly IApplicationUserService _users;
        private readonly IAppointmentService _appts;
        private readonly IBillService _bills;
        private readonly ILabService _labs;
        private readonly IHospitalInfoService _hospitals;
        private readonly IRoomService _rooms;
        private readonly IMedicineService _medicines;
        private readonly IPayrollService _payrolls;

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
            _users = users;
            _appts = appts;
            _bills = bills;
            _labs = labs;
            _hospitals = hospitals;
            _rooms = rooms;
            _medicines = medicines;
            _payrolls = payrolls;
        }

        public IActionResult Index()
        {
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
            return View();
        }
    }
}
