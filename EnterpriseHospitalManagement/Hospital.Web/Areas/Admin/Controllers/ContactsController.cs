using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Utilities;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class ContactsController : Controller
    {
        private readonly IContactService _contactService;

        public ContactsController(IContactService contactService)
        {
            _contactService = contactService;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            var model = _contactService.GetAll(pageNumber, pageSize);
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContactViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _contactService.InsertContact(vm);
                TempData["success"] = "Contact created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _contactService.GetContactById(id);
            if (vm == null)
            {
                return NotFound();
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ContactViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _contactService.UpdateContact(vm);
                TempData["success"] = "Contact updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _contactService.DeleteContact(id);
            TempData["success"] = "Contact deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ExportCsv()
        {
            var bytes = _contactService.ExportContactsCsv();
            return File(bytes, "text/csv", "contacts.csv");
        }

        [HttpGet]
        public IActionResult ExportPdf()
        {
            var bytes = _contactService.ExportContactsPdf();
            return File(bytes, "application/pdf", "contacts.pdf");
        }

    }
}
