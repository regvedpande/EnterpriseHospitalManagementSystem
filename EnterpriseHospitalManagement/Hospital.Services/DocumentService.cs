using Hospital.Models;
using Hospital.Models.Enums;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IGenericRepository<PatientDocument> _repo;
        private readonly IWebHostEnvironment _env;

        public DocumentService(IGenericRepository<PatientDocument> repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env  = env;
        }

        public PagedResult<DocumentViewModel> GetAll(int pageNumber, int pageSize)
        {
            var query = _repo.GetAll()
                .Include(d => d.Patient)
                .Include(d => d.UploadedBy);
            var total = query.Count();
            var items = query.OrderByDescending(d => d.UploadedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(d => new DocumentViewModel(d)).ToList();
            return new PagedResult<DocumentViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PagedResult<DocumentViewModel> GetByPatient(string patientId, int pageNumber, int pageSize)
        {
            var query = _repo.GetAll()
                .Include(d => d.Patient)
                .Include(d => d.UploadedBy)
                .Where(d => d.PatientId == patientId);
            var total = query.Count();
            var items = query.OrderByDescending(d => d.UploadedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(d => new DocumentViewModel(d)).ToList();
            return new PagedResult<DocumentViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public DocumentViewModel? GetById(int id)
        {
            var d = _repo.GetAll()
                .Include(d => d.Patient)
                .Include(d => d.UploadedBy)
                .FirstOrDefault(d => d.Id == id);
            return d == null ? null : new DocumentViewModel(d);
        }

        public async Task<DocumentViewModel> UploadAsync(string patientId, string uploadedById, IFormFile file, string documentType, string? description)
        {
            var docsFolder = Path.Combine(_env.WebRootPath, "documents");
            Directory.CreateDirectory(docsFolder);

            var ext      = Path.GetExtension(file.FileName);
            var stored   = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(docsFolder, stored);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            _ = Enum.TryParse<DocumentType>(documentType, out var docType);

            var doc = new PatientDocument
            {
                PatientId        = patientId,
                UploadedById     = uploadedById,
                FileName         = stored,
                OriginalFileName = file.FileName,
                ContentType      = file.ContentType,
                FileSizeBytes    = file.Length,
                DocumentType     = docType,
                Description      = description,
                UploadedDate     = DateTime.UtcNow
            };

            _repo.Add(doc);
            _repo.Save();
            return new DocumentViewModel(doc);
        }

        public void Delete(int id)
        {
            var doc = _repo.GetById(id);
            if (doc == null) return;

            // Remove file from disk
            var path = Path.Combine(_env.WebRootPath, "documents", doc.FileName);
            if (File.Exists(path)) File.Delete(path);

            _repo.Delete(doc);
            _repo.Save();
        }

        public string GetDocumentPath(string fileName) =>
            Path.Combine(_env.WebRootPath, "documents", fileName);
    }
}
