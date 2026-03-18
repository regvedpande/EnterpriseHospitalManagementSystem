using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Services
{
    public class LabService : ILabService
    {
        private readonly IGenericRepository<Lab> _repo;
        public LabService(IGenericRepository<Lab> repo) => _repo = repo;

        private IQueryable<Lab> FullQuery() => _repo.GetAll()
            .Include(l => l.Patient).Include(l => l.Doctor).Include(l => l.Technician);

        public PagedResult<LabViewModel> GetAll(int pageNumber, int pageSize)
        {
            var query = FullQuery();
            var total = query.Count();
            var items = query.OrderByDescending(l => l.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(l => new LabViewModel(l)).ToList();
            return new PagedResult<LabViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PagedResult<LabViewModel> GetByPatient(string patientId, int pageNumber, int pageSize)
        {
            var query = FullQuery().Where(l => l.PatientId == patientId);
            var total = query.Count();
            var items = query.OrderByDescending(l => l.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(l => new LabViewModel(l)).ToList();
            return new PagedResult<LabViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PagedResult<LabViewModel> GetByTechnician(string techId, int pageNumber, int pageSize)
        {
            var query = FullQuery().Where(l => l.TechnicianId == techId);
            var total = query.Count();
            var items = query.OrderByDescending(l => l.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(l => new LabViewModel(l)).ToList();
            return new PagedResult<LabViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public LabViewModel? GetById(int id)
        {
            var e = FullQuery().FirstOrDefault(l => l.Id == id);
            return e == null ? null : new LabViewModel(e);
        }

        public void Insert(LabViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel(); model.Id = 0;
            _repo.Add(model); _repo.Save();
        }

        public void Update(LabViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;
            existing.LabNumber = vm.LabNumber; existing.PatientId = vm.PatientId;
            existing.DoctorId = vm.DoctorId; existing.TechnicianId = vm.TechnicianId;
            existing.TestType = vm.TestType; existing.TestCode = vm.TestCode;
            existing.Status = vm.Status; existing.Weight = vm.Weight;
            existing.BloodPressure = vm.BloodPressure; existing.Temperature = vm.Temperature;
            existing.TestResult = vm.TestResult; existing.CompletedDate = vm.CompletedDate;
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
