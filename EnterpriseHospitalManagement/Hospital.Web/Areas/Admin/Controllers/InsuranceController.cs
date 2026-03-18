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
    public class InsuranceController : Controller
    {
        private readonly IInsuranceService _svc;
        private readonly IApplicationUserService _users;
        public InsuranceController(IInsuranceService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Patients = new SelectList(_users.GetAllPatients(1, 200).Items, "Id", "Name");
            return View(new InsuranceViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(InsuranceViewModel vm)
        {
            if (!ModelState.IsValid) { ViewBag.Patients = new SelectList(_users.GetAllPatients(1, 200).Items, "Id", "Name"); return View(vm); }
            _svc.Insert(vm);
            TempData["success"] = "Insurance policy created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null) return NotFound();
            ViewBag.Patients = new SelectList(_users.GetAllPatients(1, 200).Items, "Id", "Name");
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(InsuranceViewModel vm)
        {
            if (!ModelState.IsValid) { ViewBag.Patients = new SelectList(_users.GetAllPatients(1, 200).Items, "Id", "Name"); return View(vm); }
            _svc.Update(vm);
            TempData["success"] = "Insurance policy updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.Delete(id);
            TempData["success"] = "Insurance policy deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
