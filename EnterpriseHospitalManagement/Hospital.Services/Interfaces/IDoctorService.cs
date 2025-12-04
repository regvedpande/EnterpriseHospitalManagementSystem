using cloudscribe.Pagination.Models;
using EnterpriseHospitalManagement.Hospital.ViewModels;
using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IDoctorService
    {
        PagedResult<TimingViewModel> GetAllTimings(int pageNumber, int pageSize);
        TimingViewModel GetTimingById(int timingId);
        void AddTiming(TimingViewModel timing);
        void UpdateTiming(TimingViewModel timing);
        void DeleteTiming(int timingId);
    }
}
