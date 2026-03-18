using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IBillService
    {
        PagedResult<BillViewModel> GetAll(int pageNumber, int pageSize);
        PagedResult<BillViewModel> GetByPatient(string patientId, int pageNumber, int pageSize);
        BillViewModel? GetById(int id);
        void Insert(BillViewModel vm);
        void Update(BillViewModel vm);
        void Delete(int id);
    }
}
