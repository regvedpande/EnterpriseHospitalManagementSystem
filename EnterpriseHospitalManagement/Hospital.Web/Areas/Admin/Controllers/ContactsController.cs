using System;
using System.Text;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Areas.Admin.Controllers
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

        // IContactService.GetAll(pageNumber, pageSize) → PagedResult<ContactViewModel>
        public IActionResult Index(int pageNumber = 1, int pageSize = 25)
        {
            var result = _contactService.GetAll(pageNumber, pageSize);
            return View(result);
        }

        [HttpGet]
        public IActionResult Create() => View(new ContactViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(ContactViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _contactService.InsertContact(vm);
            TempData["success"] = "Contact created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _contactService.GetContactById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(ContactViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _contactService.UpdateContact(vm);
            TempData["success"] = "Contact updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _contactService.DeleteContact(id);
            TempData["success"] = "Contact deleted.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ExportCsv()
        {
            var list = _contactService.GetAll(1, int.MaxValue).Data;
            var sb = new StringBuilder();
            sb.AppendLine("Id,Email,Phone,HospitalInfoId");
            foreach (var c in list)
                sb.AppendLine($"{c.Id},{Escape(c.Email)},{Escape(c.Phone)},{c.HospitalInfoId}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"contacts_{DateTime.Now:yyyyMMdd}.csv");
        }

        public IActionResult ExportPdf()
        {
            var list = _contactService.GetAll(1, int.MaxValue).Data;
            var sb = new StringBuilder();
            sb.AppendLine("Contacts Export");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine(new string('-', 40));
            foreach (var c in list)
            {
                sb.AppendLine($"Id: {c.Id}");
                sb.AppendLine($"Email: {c.Email}");
                sb.AppendLine($"Phone: {c.Phone}");
                sb.AppendLine($"Hospital Id: {c.HospitalInfoId}");
                sb.AppendLine();
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/pdf",
                $"contacts_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private static string Escape(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (s.Contains(',') || s.Contains('"'))
                return $"\"{s.Replace("\"", "\"\"")}\"";
            return s;
        }
    }
}