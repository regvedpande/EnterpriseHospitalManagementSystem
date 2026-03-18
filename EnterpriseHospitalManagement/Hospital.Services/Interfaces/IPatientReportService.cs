using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IPatientReportService
    {
        PagedResult<PatientReportViewModel> GetAll(int pageNumber, int pageSize);
        PagedResult<PatientReportViewModel> GetByDoctor(string doctorId, int pageNumber, int pageSize);
        PagedResult<PatientReportViewModel> GetByPatient(string patientId, int pageNumber, int pageSize);
        PatientReportViewModel? GetById(int id);
        void Insert(PatientReportViewModel vm);
        void Update(PatientReportViewModel vm);
        void Delete(int id);
    }
}
