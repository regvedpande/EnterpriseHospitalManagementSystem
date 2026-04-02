using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    [Authorize(Roles = WebSiteRoles.Website_Nurse)]
    public class AiAssistantController : Controller
    {
        [HttpGet]
        public IActionResult Index() => RedirectToActionPermanent("Index", "Ai", new { area = "Nurse" });
    }
}
