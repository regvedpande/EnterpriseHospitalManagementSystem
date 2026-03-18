using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.Web.Areas.LabTech.Controllers
{
    [Area("LabTech")]
    [Authorize(Roles = WebSiteRoles.Website_LabTech)]
    public class HomeController : Controller
    {
        private readonly ILabService _labs;
        public HomeController(ILabService l) => _labs = l;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            ViewBag.TotalTests = _labs.GetAll(1, 1).TotalCount;
            ViewBag.MyTests = _labs.GetByTechnician(UserId, 1, 1).TotalCount;
            return View();
        }
    }
}
