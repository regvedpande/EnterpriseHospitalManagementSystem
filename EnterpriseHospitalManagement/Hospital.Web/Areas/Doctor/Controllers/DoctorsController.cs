using System;
using System.Collections.Generic;
using System.Security.Claims;
using Hospital.Models.Enums;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Hospital.Utilities;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            var model = _doctorService.GetAllTimings(pageNumber, pageSize);
            return View(model);
        }

        [HttpGet]
        public IActionResult AddTiming()
        {
            var vm = new TimingViewModel
            {
                ScheduleDate = DateTime.Now.AddDays(1),
                Duration = 30,
                Status = Status.Available
            };

            PopulateShiftDropdowns();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTiming(TimingViewModel vm)
        {
            PopulateShiftDropdowns();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                vm.DoctorId = claim.Value;
            }

            if (ModelState.IsValid)
            {
                _doctorService.AddTiming(vm);
                TempData["success"] = "Timing created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _doctorService.GetTimingById(id);
            if (vm == null)
            {
                return NotFound();
            }

            PopulateShiftDropdowns();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TimingViewModel vm)
        {
            PopulateShiftDropdowns();

            if (ModelState.IsValid)
            {
                _doctorService.UpdateTiming(vm);
                TempData["success"] = "Timing updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _doctorService.DeleteTiming(id);
            TempData["success"] = "Timing deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateShiftDropdowns()
        {
            var morningShiftStart = new List<SelectListItem>();
            var morningShiftEnd = new List<SelectListItem>();
            var afternoonShiftStart = new List<SelectListItem>();
            var afternoonShiftEnd = new List<SelectListItem>();

            for (int i = 9; i <= 12; i++)
            {
                morningShiftStart.Add(new SelectListItem { Text = $"{i}:00", Value = i.ToString() });
                morningShiftEnd.Add(new SelectListItem { Text = $"{i}:00", Value = i.ToString() });
            }

            for (int i = 13; i <= 18; i++)
            {
                afternoonShiftStart.Add(new SelectListItem { Text = $"{i}:00", Value = i.ToString() });
                afternoonShiftEnd.Add(new SelectListItem { Text = $"{i}:00", Value = i.ToString() });
            }

            ViewBag.MorningShiftStart = new SelectList(morningShiftStart, "Value", "Text");
            ViewBag.MorningShiftEnd = new SelectList(morningShiftEnd, "Value", "Text");
            ViewBag.AfternoonShiftStart = new SelectList(afternoonShiftStart, "Value", "Text");
            ViewBag.AfternoonShiftEnd = new SelectList(afternoonShiftEnd, "Value", "Text");
        }
    }
}
