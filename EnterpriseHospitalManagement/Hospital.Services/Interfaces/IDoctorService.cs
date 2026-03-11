using Hospital.Utilities;
using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IDoctorService
    {
        // Timing-based operations
        PagedResult<TimingViewModel> GetAllTimings(int pageNumber, int pageSize);
        TimingViewModel? GetTimingById(int timingId);
        void AddTiming(TimingViewModel timing);
        void UpdateTiming(TimingViewModel timing);
        void DeleteTiming(int timingId);

        // Doctor-view operations (used by DoctorsController and Patient HomeController)
        PagedResult<DoctorViewModel> GetAll(int pageNumber, int pageSize);
        DoctorViewModel? GetDoctorById(int id);
        void InsertDoctor(DoctorViewModel vm);
        void UpdateDoctor(DoctorViewModel vm);
        void DeleteDoctor(int id);
    }
}