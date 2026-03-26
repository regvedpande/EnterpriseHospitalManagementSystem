using Hospital.Models.Enums;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.Web.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = WebSiteRoles.Website_Patient)]
    public class DocumentsController : Controller
    {
        private readonly IDocumentService _docs;
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public DocumentsController(IDocumentService docs) => _docs = docs;

        public IActionResult Index(int page = 1, int size = 10) =>
            View(_docs.GetByPatient(UserId, page, size));

        [HttpGet]
        public IActionResult Upload()
        {
            ViewBag.DocumentTypes = Enum.GetNames<DocumentType>();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string documentType, string? description)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                ViewBag.DocumentTypes = Enum.GetNames<DocumentType>();
                return View();
            }

            await _docs.UploadAsync(UserId, UserId, file, documentType, description);
            TempData["success"] = "Document uploaded successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var doc = _docs.GetById(id);
            if (doc == null || doc.PatientId != UserId) return NotFound();
            _docs.Delete(id);
            TempData["success"] = "Document deleted.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(int id)
        {
            var doc = _docs.GetById(id);
            if (doc == null || doc.PatientId != UserId) return NotFound();
            var path = _docs.GetDocumentPath(doc.FileName);
            if (!System.IO.File.Exists(path)) return NotFound();
            return PhysicalFile(path, doc.ContentType, doc.OriginalFileName);
        }
    }
}
