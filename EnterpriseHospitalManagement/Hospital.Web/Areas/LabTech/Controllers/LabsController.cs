using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Hospital.Web.Areas.LabTech.Controllers
{
    [Area("LabTech")]
    [Authorize(Roles = WebSiteRoles.Website_LabTech)]
    public class LabsController : Controller
    {
        private readonly ILabService _svc;
        private readonly IApplicationUserService _users;
        public LabsController(ILabService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(LabViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            vm.TechnicianId = UserId;
            if (vm.Status == Hospital.Models.Enums.LabTestStatus.Completed)
                vm.CompletedDate = DateTime.Now;
            _svc.Update(vm);
            TempData["success"] = "Lab test updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
