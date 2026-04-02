using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class AiAssistantController : Controller
    {
        [HttpGet]
        public IActionResult Index() => RedirectToActionPermanent("Index", "Ai", new { area = "Doctor" });
    }
}
