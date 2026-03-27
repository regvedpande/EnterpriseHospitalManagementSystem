using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Security.Claims;

namespace Hospital.Web.Areas.LabTech.Controllers
{
    [Area("LabTech")]
    [Authorize(Roles = WebSiteRoles.Website_LabTech)]
    public class LabsController : Controller
    {
        private readonly ILabService             _svc;
        private readonly IApplicationUserService _users;
        public LabsController(ILabService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new LabViewModel
            {
                LabNumber   = "LAB-" + DateTime.Now.Ticks.ToString()[^6..],
                TechnicianId = UserId,
                CreatedDate  = DateTime.Now
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(LabViewModel vm)
        {
            vm.TechnicianId = UserId;
            if (!ModelState.IsValid) { PopulateDropdowns(); return View(vm); }
            _svc.Insert(vm);
            TempData["success"] = "Lab order created.";
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

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.Delete(id);
            TempData["success"] = "Lab order deleted.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns()
        {
            var patients = _users.GetAllPatients(1, 200).Items;
            var doctors  = _users.GetAllDoctors(1, 200).Items;
            ViewBag.Patients = new SelectList(patients, "Id", "Name");
            ViewBag.Doctors  = new SelectList(doctors, "Id", "Name");
        }
    }
}
