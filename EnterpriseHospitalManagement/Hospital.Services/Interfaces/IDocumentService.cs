using Hospital.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Hospital.Services.Interfaces
{
    public interface IDocumentService
    {
        PagedResult<DocumentViewModel> GetAll(int pageNumber, int pageSize);
        PagedResult<DocumentViewModel> GetByPatient(string patientId, int pageNumber, int pageSize);
        DocumentViewModel? GetById(int id);
        Task<DocumentViewModel> UploadAsync(string patientId, string uploadedById, IFormFile file, string documentType, string? description);
        void Delete(int id);
        string GetDocumentPath(string fileName);
    }
}
