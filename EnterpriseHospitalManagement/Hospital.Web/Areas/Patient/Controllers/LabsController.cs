using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.Web.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = WebSiteRoles.Website_Patient)]
    public class LabsController : Controller
    {
        private readonly ILabService _svc;
        public LabsController(ILabService svc) => _svc = svc;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index(int page = 1, int size = 10) =>
            View(_svc.GetByPatient(UserId, page, size));
    }
}
