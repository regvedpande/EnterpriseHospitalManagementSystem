using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class HospitalsController : Controller
    {
        private readonly IHospitalInfoService _hospitalService;
        public HospitalsController(IHospitalInfoService hospitalService) => _hospitalService = hospitalService;

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
            => View(_hospitalService.GetAll(pageNumber, pageSize));

        [HttpGet]
        public IActionResult Create() => View(new HospitalInfoViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(HospitalInfoViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _hospitalService.InsertHospitalInfo(vm);
            TempData["success"] = "Hospital created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _hospitalService.GetHospitalById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(HospitalInfoViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _hospitalService.UpdateHospitalInfo(vm);
            TempData["success"] = "Hospital updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _hospitalService.DeleteHospitalInfo(id);
            TempData["success"] = "Hospital deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
