using Hospital.Models;
using Hospital.Models.Enums;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class DocumentsController : Controller
    {
        private readonly IDocumentService _docs;
        private readonly IApplicationUserService _users;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentsController(IDocumentService docs, IApplicationUserService users, UserManager<ApplicationUser> userManager)
        {
            _docs        = docs;
            _users       = users;
            _userManager = userManager;
        }

        public IActionResult Index(int page = 1, int size = 10) =>
            View(_docs.GetAll(page, size));

        [HttpGet]
        public IActionResult Upload()
        {
            PopulateDropdowns();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(string patientId, IFormFile file, string documentType, string? description)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                PopulateDropdowns();
                return View();
            }
            if (string.IsNullOrWhiteSpace(patientId))
            {
                ModelState.AddModelError("", "Please select a patient.");
                PopulateDropdowns();
                return View();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var uploaderId  = currentUser?.Id ?? "";

            await _docs.UploadAsync(patientId, uploaderId, file, documentType, description);
            TempData["success"] = "Document uploaded successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _docs.Delete(id);
            TempData["success"] = "Document deleted.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(int id)
        {
            var doc = _docs.GetById(id);
            if (doc == null) return NotFound();
            var path = _docs.GetDocumentPath(doc.FileName);
            if (!System.IO.File.Exists(path)) return NotFound();
            return PhysicalFile(path, doc.ContentType, doc.OriginalFileName);
        }

        private void PopulateDropdowns()
        {
            ViewBag.Patients      = new SelectList(_users.GetAllPatients(1, 500).Items, "Id", "Name");
            ViewBag.DocumentTypes = Enum.GetNames<DocumentType>();
        }
    }
}
