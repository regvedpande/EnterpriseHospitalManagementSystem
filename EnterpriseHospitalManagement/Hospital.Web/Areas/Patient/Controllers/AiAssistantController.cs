using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = WebSiteRoles.Website_Patient)]
    public class AiAssistantController : Controller
    {
        [HttpGet]
        public IActionResult Index() => RedirectToActionPermanent("Index", "Ai", new { area = "Patient" });
    }
}
