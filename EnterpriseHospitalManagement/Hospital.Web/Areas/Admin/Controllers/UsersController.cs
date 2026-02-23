using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class UsersController : Controller
    {
        private readonly IApplicationUserService _service;
        public UsersController(IApplicationUserService service) => _service = service;

        public IActionResult Index()
        {
            var users = _service.GetAllUsers();
            return View(users);
        }

        public IActionResult AllDoctors()
        {
            var doctors = _service.GetAllDoctors();
            return View(doctors);
        }

        public IActionResult AllPatients()
        {
            var patients = _service.GetAllPatients();
            return View(patients);
        }
    }
}
