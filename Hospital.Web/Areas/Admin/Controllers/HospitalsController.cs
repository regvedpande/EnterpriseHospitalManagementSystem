using cloudscribe.Pagination.Models;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    public class HospitalsController : Controller
    {
        [Area("admin")]
        private HospitalInfoViewModel _hospitalInfo;
        public HospitalsController(HospitalInfoViewModel hospitalInfo)
        {
            _hospitalInfo = hospitalInfo;
        }
        public IActionResult Index(int pageNumber=1, int pageSize=10)
        {
            return View(_hospitalInfo.GetAll(pageNumber, pageSize));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var viewModel = _hospitalInfo.GetHospitalbyId(id);
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(HospitalInfoViewModel vm)
        {
            _hospitalInfo.UpdateHospitalInfo(vm);
            return RedirectToAction("Index");

        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(HospitalInfoViewModel vm)
        {
            _hospitalInfo.InsertHospitalInfo(vm);
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var viewModel = _hospitalInfo.GetHospitalbyId(id);
            return View(viewModel);
        }

    }
}
