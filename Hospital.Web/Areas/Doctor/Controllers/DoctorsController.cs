using Hospital.Services;
using Hospital.ViewModels;
using Hospital.Web.Areas.Doctor.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public IActionResult AddTiming()
        {
            var morningShiftStart = new List<SelectListItem>();
            var morningShiftEnd = new List<SelectListItem>();
            var AfternoonShiftStart = new List<SelectListItem>();
            var AfternoonShiftEnd = new List<SelectListItem>();

            for (int i = 9; i <= 12; i++)
            {
                morningShiftStart.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                morningShiftEnd.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            for (int i = 13; i <= 18; i++)
            {
                AfternoonShiftStart.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                AfternoonShiftEnd.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            ViewBag.morningStart = new SelectList(morningShiftStart, "Value", "Text");
            ViewBag.morningEnd = new SelectList(morningShiftEnd, "Value", "Text");
            ViewBag.evenStart = new SelectList(AfternoonShiftStart, "Value", "Text");
            ViewBag.evenEnd = new SelectList(AfternoonShiftEnd, "Value", "Text");
            TimingViewModel vm = new TimingViewModel();
            vm.ScheduleDate = DateTime.Now;
            vm.ScheduleDate = vm.ScheduleDate.AddDays(1);

            return View();
        }

        [HttpPost]
        public IActionResult AddTiming(TimingViewModel vm)
        {
            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                vm.DoctorId = claim.Value;
            }
            _doctorService.AddTiming(vm);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var viewModel = _doctorService.GetTimingById(id);
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(TimingViewModel vm)
        {
            _doctorService.UpdateTiming(vm);
            return RedirectToAction("Index");
        }
    }
}
