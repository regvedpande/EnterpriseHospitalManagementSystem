// Areas/Admin/Controllers/ContactsController.cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;

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
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _contactService.DeleteContact(id);
            return RedirectToAction(nameof(Index));
        }

        // CSV export - generate CSV from current data
        public IActionResult ExportContactsCsv()
        {
            // fetch all (use large page size to get all, or adapt your service to provide "all")
            var page = 1;
            var pageSize = int.MaxValue;
            var list = _contactService.GetAll(page, pageSize)?.Data ?? Enumerable.Empty<ContactViewModel>();

            var sb = new StringBuilder();
            // header (try to use common properties)
            var header = "Id,Name,Email,Phone,Message";
            sb.AppendLine(header);

            foreach (var c in list)
            {
                var id = SafeGetProperty(c, "Id");
                var name = SafeGetProperty(c, "Name");
                var email = SafeGetProperty(c, "Email");
                var phone = SafeGetProperty(c, "Phone");
                var message = SafeGetProperty(c, "Message");

                var line = $"{EscapeCsv(id)},{EscapeCsv(name)},{EscapeCsv(email)},{EscapeCsv(phone)},{EscapeCsv(message)}";
                sb.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "contacts.csv");
        }

        // "PDF" export - simple placeholder that returns CSV bytes with PDF content-type.
        // Replace with real PDF generation if you want formatted PDF output.
        public IActionResult ExportContactsPdf()
        {
            var page = 1;
            var pageSize = int.MaxValue;
            var list = _contactService.GetAll(page, pageSize)?.Data ?? Enumerable.Empty<ContactViewModel>();

            var sb = new StringBuilder();
            sb.AppendLine("Contacts Export");
            sb.AppendLine("----------------");
            foreach (var c in list)
            {
                sb.AppendLine($"Id: {SafeGetProperty(c, "Id")}");
                sb.AppendLine($"Name: {SafeGetProperty(c, "Name")}");
                sb.AppendLine($"Email: {SafeGetProperty(c, "Email")}");
                sb.AppendLine($"Phone: {SafeGetProperty(c, "Phone")}");
                sb.AppendLine("");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            // Not a real PDF generator — returns plain text with PDF content-type. Replace with PDF lib if needed.
            return File(bytes, "application/pdf", "contacts.pdf");
        }

        private static string SafeGetProperty(object obj, string name)
        {
            if (obj == null) return string.Empty;
            var prop = obj.GetType().GetProperty(name) ?? obj.GetType().GetProperties().FirstOrDefault();
            if (prop == null) return string.Empty;
            var v = prop.GetValue(obj);
            return v?.ToString() ?? string.Empty;
        }

        private static string EscapeCsv(string s)
        {
            if (s == null) return "";
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\r") || s.Contains("\n"))
            {
                var escaped = s.Replace("\"", "\"\"");
                return $"\"{escaped}\"";
            }
            return s;
        }
    }
}
