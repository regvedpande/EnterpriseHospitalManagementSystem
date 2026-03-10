using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class ContactsController : Controller
    {
        private readonly IContactService _svc;
        public ContactsController(IContactService svc) => _svc = svc;

        public IActionResult Index(int page = 1, int size = 10) => View(_svc.GetAll(page, size));

        [HttpGet] public IActionResult Create() => View(new ContactViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(ContactViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.InsertContact(vm);
            TempData["success"] = "Contact created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet] public IActionResult Edit(int id)
        {
            var vm = _svc.GetContactById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(ContactViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _svc.UpdateContact(vm);
            TempData["success"] = "Contact updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _svc.DeleteContact(id);
            TempData["success"] = "Contact deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
