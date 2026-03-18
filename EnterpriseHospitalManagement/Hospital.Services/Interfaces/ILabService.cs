using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface ILabService
    {
        PagedResult<LabViewModel> GetAll(int pageNumber, int pageSize);
        PagedResult<LabViewModel> GetByPatient(string patientId, int pageNumber, int pageSize);
        PagedResult<LabViewModel> GetByTechnician(string techId, int pageNumber, int pageSize);
        LabViewModel? GetById(int id);
        void Insert(LabViewModel vm);
        void Update(LabViewModel vm);
        void Delete(int id);
    }
}
