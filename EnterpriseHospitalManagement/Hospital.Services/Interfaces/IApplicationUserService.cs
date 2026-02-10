using Hospital.Utilities;
using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IApplicationUserService
    {
        PagedResult<ApplicationUserViewModel> GetAll(int pageNumber, int pageSize);
        PagedResult<ApplicationUserViewModel> GetAllDoctors(int pageNumber, int pageSize);
        PagedResult<ApplicationUserViewModel> GetAllPatients(int pageNumber, int pageSize);
        PagedResult<ApplicationUserViewModel> SearchDoctors(int pageNumber, int pageSize, string search);
    }
}
