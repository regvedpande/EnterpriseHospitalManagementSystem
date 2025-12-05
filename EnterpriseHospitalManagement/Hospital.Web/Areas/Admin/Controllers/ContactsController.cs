using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactsController : Controller
    {
        private readonly IContactService _contactService;

        public ContactsController(IContactService contactService)
        {
            _contactService = contactService;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            return View(_contactService.GetAll(pageNumber, pageSize));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContactViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _contactService.InsertContact(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _contactService.GetContactById(id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ContactViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _contactService.UpdateContact(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        public IActionResult Delete(int id)
        {
            _contactService.DeleteContact(id);
            return RedirectToAction("Index");
        }
    }
}
