using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            return View(_doctorService.GetAllTimings(pageNumber, pageSize));
        }

        [HttpGet]
        public IActionResult AddTiming()
        {
            var morningShiftStart = new List<SelectListItem>();
            var morningShiftEnd = new List<SelectListItem>();
            var afternoonShiftStart = new List<SelectListItem>();
            var afternoonShiftEnd = new List<SelectListItem>();

            for (int i = 9; i <= 12; i++)
            {
                morningShiftStart.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                morningShiftEnd.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            for (int i = 13; i <= 18; i++)
            {
                afternoonShiftStart.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                afternoonShiftEnd.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            ViewBag.MorningShiftStart = new SelectList(morningShiftStart, "Value", "Text");
            ViewBag.MorningShiftEnd = new SelectList(morningShiftEnd, "Value", "Text");
            ViewBag.AfternoonShiftStart = new SelectList(afternoonShiftStart, "Value", "Text");
            ViewBag.AfternoonShiftEnd = new SelectList(afternoonShiftEnd, "Value", "Text");

            var vm = new TimingViewModel
            {
                ScheduleDate = DateTime.Now.AddDays(1)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTiming(TimingViewModel vm)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                vm.DoctorId = claim.Value;
            }

            if (ModelState.IsValid)
            {
                _doctorService.AddTiming(vm);
                return RedirectToAction("Index");
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _doctorService.GetTimingById(id);

            var morningShiftStart = new List<SelectListItem>();
            var morningShiftEnd = new List<SelectListItem>();
            var afternoonShiftStart = new List<SelectListItem>();
            var afternoonShiftEnd = new List<SelectListItem>();

            for (int i = 9; i <= 12; i++)
            {
                morningShiftStart.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                morningShiftEnd.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            for (int i = 13; i <= 18; i++)
            {
                afternoonShiftStart.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                afternoonShiftEnd.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            ViewBag.MorningShiftStart = new SelectList(morningShiftStart, "Value", "Text");
            ViewBag.MorningShiftEnd = new SelectList(morningShiftEnd, "Value", "Text");
            ViewBag.AfternoonShiftStart = new SelectList(afternoonShiftStart, "Value", "Text");
            ViewBag.AfternoonShiftEnd = new SelectList(afternoonShiftEnd, "Value", "Text");

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TimingViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _doctorService.UpdateTiming(vm);
                return RedirectToAction("Index");
            }

            return View(vm);
        }

        public IActionResult Delete(int id)
        {
            _doctorService.DeleteTiming(id);
            return RedirectToAction("Index");
        }
    }
}
