using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Services
{
    public class PatientReportService : IPatientReportService
    {
        private readonly IGenericRepository<PatientReport> _repo;
        public PatientReportService(IGenericRepository<PatientReport> repo) => _repo = repo;

        private IQueryable<PatientReport> FullQuery() => _repo.GetAll()
            .Include(r => r.Doctor).Include(r => r.Patient)
            .Include(r => r.PrescribedMedicines).ThenInclude(p => p.Medicine);

        public PagedResult<PatientReportViewModel> GetAll(int pageNumber, int pageSize)
        {
            var query = FullQuery();
            var total = query.Count();
            var items = query.OrderByDescending(r => r.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(r => new PatientReportViewModel(r)).ToList();
            return new PagedResult<PatientReportViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PagedResult<PatientReportViewModel> GetByDoctor(string doctorId, int pageNumber, int pageSize)
        {
            var query = FullQuery().Where(r => r.DoctorId == doctorId);
            var total = query.Count();
            var items = query.OrderByDescending(r => r.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(r => new PatientReportViewModel(r)).ToList();
            return new PagedResult<PatientReportViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PagedResult<PatientReportViewModel> GetByPatient(string patientId, int pageNumber, int pageSize)
        {
            var query = FullQuery().Where(r => r.PatientId == patientId);
            var total = query.Count();
            var items = query.OrderByDescending(r => r.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(r => new PatientReportViewModel(r)).ToList();
            return new PagedResult<PatientReportViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PatientReportViewModel? GetById(int id)
        {
            var e = FullQuery().FirstOrDefault(r => r.Id == id);
            return e == null ? null : new PatientReportViewModel(e);
        }

        public void Insert(PatientReportViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel(); model.Id = 0;
            _repo.Add(model); _repo.Save();
        }

        public void Update(PatientReportViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;
            existing.Diagnose = vm.Diagnose; existing.Notes = vm.Notes;
            existing.DoctorId = vm.DoctorId; existing.PatientId = vm.PatientId;
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
