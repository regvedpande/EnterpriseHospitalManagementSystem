using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Services
{
    public class InsuranceService : IInsuranceService
    {
        private readonly IGenericRepository<Insurance> _repo;
        public InsuranceService(IGenericRepository<Insurance> repo) => _repo = repo;

        public PagedResult<InsuranceViewModel> GetAll(int pageNumber, int pageSize)
        {
            var query = _repo.GetAll().Include(i => i.Patient);
            var total = query.Count();
            var items = query.OrderByDescending(i => i.Id)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(i => new InsuranceViewModel(i)).ToList();
            return new PagedResult<InsuranceViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public InsuranceViewModel? GetById(int id)
        {
            var e = _repo.GetAll().Include(i => i.Patient).FirstOrDefault(i => i.Id == id);
            return e == null ? null : new InsuranceViewModel(e);
        }

        public void Insert(InsuranceViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel(); model.Id = 0;
            _repo.Add(model); _repo.Save();
        }

        public void Update(InsuranceViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;
            existing.PolicyNumber = vm.PolicyNumber; existing.ProviderName = vm.ProviderName;
            existing.PatientId = vm.PatientId; existing.CoverageAmount = vm.CoverageAmount;
            existing.StartDate = vm.StartDate; existing.EndDate = vm.EndDate;
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
