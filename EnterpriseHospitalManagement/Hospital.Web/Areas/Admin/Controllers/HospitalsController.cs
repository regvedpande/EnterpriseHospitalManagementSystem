using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HospitalsController : Controller
    {
        private readonly IHospitalInfoService _hospitalService;

        public HospitalsController(IHospitalInfoService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            return View(_hospitalService.GetAll(pageNumber, pageSize));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HospitalInfoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _hospitalService.InsertHospitalInfo(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _hospitalService.GetHospitalById(id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(HospitalInfoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _hospitalService.UpdateHospitalInfo(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        public IActionResult Delete(int id)
        {
            _hospitalService.DeleteHospitalInfo(id);
            return RedirectToAction("Index");
        }
    }
}
