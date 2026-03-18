using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hospital.Web.Areas.Accountant.Controllers
{
    [Area("Accountant")]
    [Authorize(Roles = WebSiteRoles.Website_Accountant)]
    public class PayrollsController : Controller
    {
        private readonly IPayrollService _svc;
        private readonly IApplicationUserService _users;
        public PayrollsController(IPayrollService svc, IApplicationUserService users)
        { _svc = svc; _users = users; }

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Employees = new SelectList(_users.GetAll(1, 200).Items, "Id", "Name");
            return View(new PayrollViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(PayrollViewModel vm)
        {
            if (!ModelState.IsValid) { ViewBag.Employees = new SelectList(_users.GetAll(1, 200).Items, "Id", "Name"); return View(vm); }
            _svc.Insert(vm);
            TempData["success"] = "Payroll record created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _svc.GetById(id);
            if (vm == null) return NotFound();
            ViewBag.Employees = new SelectList(_users.GetAll(1, 200).Items, "Id", "Name");
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(PayrollViewModel vm)
        {
            if (!ModelState.IsValid) { ViewBag.Employees = new SelectList(_users.GetAll(1, 200).Items, "Id", "Name"); return View(vm); }
            _svc.Update(vm);
            TempData["success"] = "Payroll record updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
