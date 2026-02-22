// Hospital.Web/Areas/Admin/Controllers/UsersController.cs
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]  // FIX: applies to all actions including AllDoctors
    public class UsersController : Controller
    {
        private readonly IApplicationUserService _userService;

        public UsersController(IApplicationUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            var model = _userService.GetAll(pageNumber, pageSize);
            return View(model);
        }

        // FIX: This action was missing [Authorize] — now covered by class-level attribute
        public IActionResult AllDoctors(int pageNumber = 1, int pageSize = 10)
        {
            var model = _userService.GetAllDoctors(pageNumber, pageSize);
            return View(model);
        }
    }
}