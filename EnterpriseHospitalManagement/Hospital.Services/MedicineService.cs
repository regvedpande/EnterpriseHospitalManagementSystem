using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;

namespace Hospital.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IGenericRepository<Medicine> _repo;
        public MedicineService(IGenericRepository<Medicine> repo) => _repo = repo;

        public PagedResult<MedicineViewModel> GetAll(int pageNumber, int pageSize)
        {
            var list = _repo.GetAll().ToList();
            return new PagedResult<MedicineViewModel>
            {
                Items = list.OrderBy(m => m.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize)
                    .Select(m => new MedicineViewModel(m)).ToList(),
                TotalCount = list.Count, PageNumber = pageNumber, PageSize = pageSize
            };
        }

        public MedicineViewModel? GetById(int id)
        {
            var e = _repo.GetById(id);
            return e == null ? null : new MedicineViewModel(e);
        }

        public void Insert(MedicineViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel(); model.Id = 0;
            _repo.Add(model); _repo.Save();
        }

        public void Update(MedicineViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;
            existing.Name = vm.Name; existing.Type = vm.Type;
            existing.Cost = vm.Cost; existing.Description = vm.Description;
            _repo.Update(existing); _repo.Save();
        }

        public void Delete(int id)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return;
            _repo.Delete(existing); _repo.Save();
        }
    }

    public class DepartmentService : IDepartmentService
    {
        private readonly IGenericRepository<Department> _repo;
        public DepartmentService(IGenericRepository<Department> repo) => _repo = repo;

        public PagedResult<DepartmentViewModel> GetAll(int pageNumber, int pageSize)
        {
            var list = _repo.GetAll().ToList();
            return new PagedResult<DepartmentViewModel>
            {
                Items = list.OrderBy(d => d.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize)
                    .Select(d => new DepartmentViewModel(d)).ToList(),
                TotalCount = list.Count, PageNumber = pageNumber, PageSize = pageSize
            };
        }

        public DepartmentViewModel? GetById(int id)
        {
            var e = _repo.GetById(id); return e == null ? null : new DepartmentViewModel(e);
        }

        public void Insert(DepartmentViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel(); model.Id = 0;
            _repo.Add(model); _repo.Save();
        }

        public void Update(DepartmentViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;
            existing.Name = vm.Name; existing.Description = vm.Description;
            _repo.Update(existing); _repo.Save();
        }

        public void Delete(int id)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return;
            _repo.Delete(existing); _repo.Save();
        }
    }

    public class SupplierService : ISupplierService
    {
        private readonly IGenericRepository<Supplier> _repo;
        public SupplierService(IGenericRepository<Supplier> repo) => _repo = repo;

        public PagedResult<SupplierViewModel> GetAll(int pageNumber, int pageSize)
        {
            var list = _repo.GetAll().ToList();
            return new PagedResult<SupplierViewModel>
            {
                Items = list.OrderBy(s => s.Company).Skip((pageNumber - 1) * pageSize).Take(pageSize)
                    .Select(s => new SupplierViewModel(s)).ToList(),
                TotalCount = list.Count, PageNumber = pageNumber, PageSize = pageSize
            };
        }

        public SupplierViewModel? GetById(int id)
        {
            var e = _repo.GetById(id); return e == null ? null : new SupplierViewModel(e);
        }

        public void Insert(SupplierViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel(); model.Id = 0;
            _repo.Add(model); _repo.Save();
        }

        public void Update(SupplierViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;
            existing.Company = vm.Company; existing.Phone = vm.Phone;
            existing.Email = vm.Email; existing.Address = vm.Address;
            _repo.Update(existing); _repo.Save();
        }

        public void Delete(int id)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return;
            _repo.Delete(existing); _repo.Save();
        }
    }
}
