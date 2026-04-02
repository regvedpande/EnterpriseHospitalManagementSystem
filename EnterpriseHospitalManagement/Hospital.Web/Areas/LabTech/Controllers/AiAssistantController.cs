using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.LabTech.Controllers
{
    [Area("LabTech")]
    [Authorize(Roles = WebSiteRoles.Website_LabTech)]
    public class AiAssistantController : Controller
    {
        [HttpGet]
        public IActionResult Index() => RedirectToActionPermanent("Index", "Ai", new { area = "LabTech" });
    }
}
