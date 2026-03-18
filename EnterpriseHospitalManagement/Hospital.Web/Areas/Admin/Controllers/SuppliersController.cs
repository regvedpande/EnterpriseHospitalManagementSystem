using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _svc;
        public SuppliersController(ISupplierService svc) => _svc = svc;

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet] public IActionResult Create() => View(new SupplierViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(SupplierViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.Insert(vm);
            TempData["success"] = "Supplier created.";
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
        public IActionResult Edit(SupplierViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.Update(vm);
            TempData["success"] = "Supplier updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.Delete(id);
            TempData["success"] = "Supplier deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
