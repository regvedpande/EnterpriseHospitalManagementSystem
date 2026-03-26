using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hospital.Web.Areas.Accountant.Controllers
{
    [Area("Accountant")]
    [Authorize(Roles = WebSiteRoles.Website_Accountant)]
    public class BillsController : Controller
    {
        private readonly IBillService _svc;
        private readonly IApplicationUserService _users;
        private readonly IInsuranceService _insurance;
        public BillsController(IBillService svc, IApplicationUserService u, IInsuranceService ins)
        { _svc = svc; _users = u; _insurance = ins; }

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new BillViewModel { BillNumber = new Random().Next(10000, 99999) });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(BillViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(); return View(vm); }
            _svc.Insert(vm);
            TempData["success"] = "Bill created.";
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
        public IActionResult Edit(BillViewModel vm)
        {
            if (!ModelState.IsValid) { PopulateDropdowns(); return View(vm); }
            _svc.Update(vm);
            TempData["success"] = "Bill updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.Delete(id);
            TempData["success"] = "Bill deleted.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns()
        {
            ViewBag.Patients = new SelectList(_users.GetAllPatients(1, 200).Items, "Id", "Name");
            ViewBag.Insurances = new SelectList(_insurance.GetAll(1, 200).Items, "Id", "PolicyNumber");
        }
    }
}
