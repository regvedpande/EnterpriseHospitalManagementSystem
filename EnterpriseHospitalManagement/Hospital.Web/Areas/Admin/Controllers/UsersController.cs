// Hospital.Web/Areas/Admin/Controllers/UsersController.cs
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class UsersController : Controller
    {
        private readonly IApplicationUserService _userService;

        public UsersController(IApplicationUserService userService)
        {
            _userService = userService;
        }

        // FIX: use GetAll() not GetAllUsers() — matches actual interface
        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            var model = _userService.GetAll(pageNumber, pageSize);
            return View(model);
        }

        // FIX: GetAllDoctors takes (pageNumber, pageSize) — pass both params explicitly
        public IActionResult AllDoctors(int pageNumber = 1, int pageSize = 10)
        {
            var model = _userService.GetAllDoctors(pageNumber, pageSize);
            return View(model);
        }

        // FIX: GetAllPatients takes (pageNumber, pageSize) — pass both params explicitly
        public IActionResult AllPatients(int pageNumber = 1, int pageSize = 10)
        {
            var model = _userService.GetAllPatients(pageNumber, pageSize);
            return View(model);
        }
    }
}