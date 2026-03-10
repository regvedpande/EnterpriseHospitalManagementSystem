using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = WebSiteRoles.Website_Patient)]
    public class HomeController : Controller
    {
        private readonly IHospitalInfoService _hospitals;
        private readonly IDoctorService       _doctors;
        private readonly IRoomService         _rooms;

        public HomeController(IHospitalInfoService h, IDoctorService d, IRoomService r)
        { _hospitals = h; _doctors = d; _rooms = r; }

        public IActionResult Index()
        {
            ViewBag.HospitalCount = _hospitals.GetAll(1, 1000).TotalCount;
            ViewBag.DoctorCount   = _doctors.GetAll(1, 1000).TotalCount;
            ViewBag.RoomCount     = _rooms.GetAll(1, 1000).TotalCount;
            ViewBag.Doctors       = _doctors.GetAll(1, 6).Items;
            ViewBag.Hospitals     = _hospitals.GetAll(1, 6).Items;
            return View();
        }
    }
}
