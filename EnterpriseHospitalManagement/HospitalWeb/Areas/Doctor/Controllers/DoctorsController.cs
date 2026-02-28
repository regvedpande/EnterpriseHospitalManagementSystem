using System.Security.Claims;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;
        public DoctorsController(IDoctorService doctorService) => _doctorService = doctorService;

        public IActionResult Index(int pageNumber = 1, int pageSize = 20)
            => View(_doctorService.GetAllTimings(pageNumber, pageSize));

        [HttpGet]
        public IActionResult AddTiming() => View(new TimingViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AddTiming(TimingViewModel vm)
        {
            vm.DoctorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            ModelState.Remove(nameof(vm.DoctorId));
            if (!ModelState.IsValid) return View(vm);
            _doctorService.AddTiming(vm);
            TempData["success"] = "Timing added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _doctorService.GetTimingById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(TimingViewModel vm)
        {
            vm.DoctorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            ModelState.Remove(nameof(vm.DoctorId));
            if (!ModelState.IsValid) return View(vm);
            _doctorService.UpdateTiming(vm);
            TempData["success"] = "Timing updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _doctorService.DeleteTiming(id);
            TempData["success"] = "Timing deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
