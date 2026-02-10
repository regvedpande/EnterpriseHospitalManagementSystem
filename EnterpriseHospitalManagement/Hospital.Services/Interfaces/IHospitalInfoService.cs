using Hospital.Utilities;
using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IHospitalInfoService
    {
        PagedResult<HospitalInfoViewModel> GetAll(int pageNumber, int pageSize);
        HospitalInfoViewModel GetHospitalById(int hospitalId);
        void InsertHospitalInfo(HospitalInfoViewModel hospital);
        void UpdateHospitalInfo(HospitalInfoViewModel hospital);
        void DeleteHospitalInfo(int hospitalId);
    }
}
