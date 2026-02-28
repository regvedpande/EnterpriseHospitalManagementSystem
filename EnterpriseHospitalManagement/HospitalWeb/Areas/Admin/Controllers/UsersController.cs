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
        public UsersController(IApplicationUserService userService) => _userService = userService;

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
            => View(_userService.GetAll(pageNumber, pageSize));

        public IActionResult AllDoctors(int pageNumber = 1, int pageSize = 10)
            => View(_userService.GetAllDoctors(pageNumber, pageSize));

        public IActionResult AllPatients(int pageNumber = 1, int pageSize = 10)
            => View(_userService.GetAllPatients(pageNumber, pageSize));
    }
}
