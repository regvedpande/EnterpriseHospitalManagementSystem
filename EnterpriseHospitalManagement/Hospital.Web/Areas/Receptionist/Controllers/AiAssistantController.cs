using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = WebSiteRoles.Website_Receptionist)]
    public class AiAssistantController : Controller
    {
        [HttpGet]
        public IActionResult Index() => RedirectToActionPermanent("Index", "Ai", new { area = "Receptionist" });
    }
}
