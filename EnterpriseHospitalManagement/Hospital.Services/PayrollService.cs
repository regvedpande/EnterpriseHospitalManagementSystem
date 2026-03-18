using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IGenericRepository<Payroll> _repo;
        public PayrollService(IGenericRepository<Payroll> repo) => _repo = repo;

        public PagedResult<PayrollViewModel> GetAll(int pageNumber, int pageSize)
        {
            var query = _repo.GetAll().Include(p => p.Employee);
            var total = query.Count();
            var items = query.OrderByDescending(p => p.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(p => new PayrollViewModel(p)).ToList();
            return new PagedResult<PayrollViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PayrollViewModel? GetById(int id)
        {
            var e = _repo.GetAll().Include(p => p.Employee).FirstOrDefault(p => p.Id == id);
            return e == null ? null : new PayrollViewModel(e);
        }

        public void Insert(PayrollViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel(); model.Id = 0;
            _repo.Add(model); _repo.Save();
        }

        public void Update(PayrollViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;
            existing.EmployeeId = vm.EmployeeId; existing.Status = vm.Status;
            existing.PayPeriodStart = vm.PayPeriodStart; existing.PayPeriodEnd = vm.PayPeriodEnd;
            existing.NetSalary = vm.NetSalary; existing.HourlySalary = vm.HourlySalary;
            existing.BonusSalary = vm.BonusSalary; existing.Compensation = vm.Compensation;
            existing.AccountNumber = vm.AccountNumber;
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
