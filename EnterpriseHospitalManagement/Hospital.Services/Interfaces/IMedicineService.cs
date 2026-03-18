using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IMedicineService
    {
        PagedResult<MedicineViewModel> GetAll(int pageNumber, int pageSize);
        MedicineViewModel? GetById(int id);
        void Insert(MedicineViewModel vm);
        void Update(MedicineViewModel vm);
        void Delete(int id);
    }

    public interface IDepartmentService
    {
        PagedResult<DepartmentViewModel> GetAll(int pageNumber, int pageSize);
        DepartmentViewModel? GetById(int id);
        void Insert(DepartmentViewModel vm);
        void Update(DepartmentViewModel vm);
        void Delete(int id);
    }

    public interface ISupplierService
    {
        PagedResult<SupplierViewModel> GetAll(int pageNumber, int pageSize);
        SupplierViewModel? GetById(int id);
        void Insert(SupplierViewModel vm);
        void Update(SupplierViewModel vm);
        void Delete(int id);
    }
}
