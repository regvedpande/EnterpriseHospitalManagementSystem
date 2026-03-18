using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IInsuranceService
    {
        PagedResult<InsuranceViewModel> GetAll(int pageNumber, int pageSize);
        InsuranceViewModel? GetById(int id);
        void Insert(InsuranceViewModel vm);
        void Update(InsuranceViewModel vm);
        void Delete(int id);
    }
}
