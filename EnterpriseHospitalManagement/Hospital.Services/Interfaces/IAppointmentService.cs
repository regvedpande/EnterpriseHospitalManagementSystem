using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IAppointmentService
    {
        PagedResult<AppointmentViewModel> GetAll(int pageNumber, int pageSize);
        PagedResult<AppointmentViewModel> GetByDoctor(string doctorId, int pageNumber, int pageSize);
        PagedResult<AppointmentViewModel> GetByPatient(string patientId, int pageNumber, int pageSize);
        AppointmentViewModel? GetById(int id);
        void Insert(AppointmentViewModel vm);
        void Update(AppointmentViewModel vm);
        void Delete(int id);
    }
}
