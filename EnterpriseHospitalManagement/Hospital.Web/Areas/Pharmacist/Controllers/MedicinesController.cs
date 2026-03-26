using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Pharmacist.Controllers
{
    [Area("Pharmacist")]
    [Authorize(Roles = WebSiteRoles.Website_Pharmacist)]
    public class MedicinesController : Controller
    {
        private readonly IMedicineService _svc;
        public MedicinesController(IMedicineService svc) => _svc = svc;

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet] public IActionResult Create() => View(new MedicineViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(MedicineViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.Insert(vm);
            TempData["success"] = "Medicine added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(MedicineViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.Update(vm);
            TempData["success"] = "Medicine updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.Delete(id);
            TempData["success"] = "Medicine deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
