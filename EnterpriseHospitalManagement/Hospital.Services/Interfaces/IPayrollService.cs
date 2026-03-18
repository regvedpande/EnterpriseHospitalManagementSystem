using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IPayrollService
    {
        PagedResult<PayrollViewModel> GetAll(int pageNumber, int pageSize);
        PayrollViewModel? GetById(int id);
        void Insert(PayrollViewModel vm);
        void Update(PayrollViewModel vm);
        void Delete(int id);
    }
}
