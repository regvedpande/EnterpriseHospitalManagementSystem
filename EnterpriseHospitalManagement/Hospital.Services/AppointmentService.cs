using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IGenericRepository<Appointment> _repo;

        public AppointmentService(IGenericRepository<Appointment> repo) => _repo = repo;

        public PagedResult<AppointmentViewModel> GetAll(int pageNumber, int pageSize)
        {
            var query = _repo.GetAll().Include(a => a.Doctor).Include(a => a.Patient);
            var total = query.Count();
            var items = query.OrderByDescending(a => a.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new AppointmentViewModel(a)).ToList();
            return new PagedResult<AppointmentViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PagedResult<AppointmentViewModel> GetByDoctor(string doctorId, int pageNumber, int pageSize)
        {
            var query = _repo.GetAll().Include(a => a.Doctor).Include(a => a.Patient).Where(a => a.DoctorId == doctorId);
            var total = query.Count();
            var items = query.OrderByDescending(a => a.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new AppointmentViewModel(a)).ToList();
            return new PagedResult<AppointmentViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PagedResult<AppointmentViewModel> GetByPatient(string patientId, int pageNumber, int pageSize)
        {
            var query = _repo.GetAll().Include(a => a.Doctor).Include(a => a.Patient).Where(a => a.PatientId == patientId);
            var total = query.Count();
            var items = query.OrderByDescending(a => a.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new AppointmentViewModel(a)).ToList();
            return new PagedResult<AppointmentViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public AppointmentViewModel? GetById(int id)
        {
            var e = _repo.GetAll().Include(a => a.Doctor).Include(a => a.Patient).FirstOrDefault(a => a.Id == id);
            return e == null ? null : new AppointmentViewModel(e);
        }

        public void Insert(AppointmentViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel();
            model.Id = 0;
            model.CreatedDate = System.DateTime.Now;
            _repo.Add(model);
            _repo.Save();
        }

        public void Update(AppointmentViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;
            existing.Number = vm.Number; existing.Type = vm.Type;
            existing.AppointmentDate = vm.AppointmentDate;
            existing.Status = vm.Status; existing.Description = vm.Description;
            existing.Notes = vm.Notes; existing.DoctorId = vm.DoctorId;
            existing.PatientId = vm.PatientId;
            _repo.Update(existing);
            _repo.Save();
        }

        public void Delete(int id)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return;
            _repo.Delete(existing);
            _repo.Save();
        }
    }
}
