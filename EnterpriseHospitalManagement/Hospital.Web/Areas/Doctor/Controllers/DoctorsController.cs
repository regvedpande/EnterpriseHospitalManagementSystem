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
            // Set DoctorId from logged-in user BEFORE validation
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
                vm.DoctorId = claim.Value;

            PopulateShiftDropdowns();

            if (!ModelState.IsValid)
                return View(vm);

            _doctorService.AddTiming(vm);
            TempData["success"] = "Timing created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _doctorService.GetTimingById(id);
            if (vm == null) return NotFound();
            PopulateShiftDropdowns();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TimingViewModel vm)
        {
            PopulateShiftDropdowns();
            if (!ModelState.IsValid) return View(vm);
            _doctorService.UpdateTiming(vm);
            TempData["success"] = "Timing updated successfully.";
            return RedirectToAction(nameof(Index));
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
            var morningStart = new List<SelectListItem>();
            var morningEnd = new List<SelectListItem>();
            var afternoonStart = new List<SelectListItem>();
            var afternoonEnd = new List<SelectListItem>();

            for (int i = 6; i <= 12; i++)
            {
                morningStart.Add(new SelectListItem { Text = $"{i:D2}:00", Value = i.ToString() });
                morningEnd.Add(new SelectListItem { Text = $"{i:D2}:00", Value = i.ToString() });
            }
            for (int i = 12; i <= 20; i++)
            {
                afternoonStart.Add(new SelectListItem { Text = $"{i:D2}:00", Value = i.ToString() });
                afternoonEnd.Add(new SelectListItem { Text = $"{i:D2}:00", Value = i.ToString() });
            }

            ViewBag.MorningShiftStart = new SelectList(morningStart, "Value", "Text");
            ViewBag.MorningShiftEnd = new SelectList(morningEnd, "Value", "Text");
            ViewBag.AfternoonShiftStart = new SelectList(afternoonStart, "Value", "Text");
            ViewBag.AfternoonShiftEnd = new SelectList(afternoonEnd, "Value", "Text");
        }
    }
}