// Areas/Admin/Controllers/ContactsController.cs
using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;

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

        public IActionResult Index(int page = 1, int pageSize = 25)
        {
            var result = _contactService.GetAll(page, pageSize);
            return View(result);
        }

        public IActionResult Create()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContactViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _contactService.InsertContact(vm);
            TempData["success"] = "Contact created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var vm = _contactService.GetContactById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ContactViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _contactService.UpdateContact(vm);
            TempData["success"] = "Contact updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _contactService.DeleteContact(id);
            TempData["success"] = "Contact deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ExportContactsCsv()
        {
            var list = _contactService.GetAll(1, int.MaxValue)?.Data ?? Enumerable.Empty<ContactViewModel>();

            var sb = new StringBuilder();
            // Only columns that exist on ContactViewModel
            sb.AppendLine("Id,Email,Phone,HospitalInfoId");

            foreach (var c in list)
            {
                var line = $"{c.Id}," +
                           $"{EscapeCsv(c.Email)}," +
                           $"{EscapeCsv(c.Phone)}," +
                           $"{c.HospitalInfoId}";
                sb.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"contacts_{DateTime.Now:yyyyMMdd}.csv");
        }

        public IActionResult ExportContactsPdf()
        {
            var list = _contactService.GetAll(1, int.MaxValue)?.Data ?? Enumerable.Empty<ContactViewModel>();

            var sb = new StringBuilder();
            sb.AppendLine("Contacts Export");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine(new string('-', 40));

            foreach (var c in list)
            {
                sb.AppendLine($"Id:             {c.Id}");
                sb.AppendLine($"Email:          {c.Email}");
                sb.AppendLine($"Phone:          {c.Phone}");
                sb.AppendLine($"HospitalInfoId: {c.HospitalInfoId}");
                sb.AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "application/pdf", $"contacts_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private static string EscapeCsv(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (s.Contains(',') || s.Contains('"') || s.Contains('\r') || s.Contains('\n'))
                return $"\"{s.Replace("\"", "\"\"")}\"";
            return s;
        }
    }
}