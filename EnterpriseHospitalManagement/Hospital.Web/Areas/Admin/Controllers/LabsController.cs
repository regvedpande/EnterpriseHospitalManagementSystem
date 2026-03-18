using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class LabsController : Controller
    {
        private readonly ILabService _svc;
        private readonly IApplicationUserService _users;
        public LabsController(ILabService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new LabViewModel { LabNumber = "LAB-" + DateTime.Now.Ticks.ToString()[^6..] });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(LabViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(); return View(vm); }
            _svc.Insert(vm);
            TempData["success"] = "Lab test created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null) return NotFound();
            PopulateDropdowns();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(LabViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(); return View(vm); }
            _svc.Update(vm);
            TempData["success"] = "Lab test updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.Delete(id);
            TempData["success"] = "Lab test deleted.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns()
        {
            ViewBag.Patients = new SelectList(_users.GetAllPatients(1, 200).Items, "Id", "Name");
            ViewBag.Doctors = new SelectList(_users.GetAllDoctors(1, 200).Items, "Id", "Name");
        }
    }
}
