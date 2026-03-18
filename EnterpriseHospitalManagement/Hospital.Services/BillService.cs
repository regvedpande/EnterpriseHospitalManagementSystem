using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Services
{
    public class BillService : IBillService
    {
        private readonly IGenericRepository<Bill> _repo;

        public BillService(IGenericRepository<Bill> repo) => _repo = repo;

        public PagedResult<BillViewModel> GetAll(int pageNumber, int pageSize)
        {
            var query = _repo.GetAll().Include(b => b.Patient);
            var total = query.Count();
            var items = query.OrderByDescending(b => b.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(b => new BillViewModel(b)).ToList();
            return new PagedResult<BillViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public PagedResult<BillViewModel> GetByPatient(string patientId, int pageNumber, int pageSize)
        {
            var query = _repo.GetAll().Include(b => b.Patient).Where(b => b.PatientId == patientId);
            var total = query.Count();
            var items = query.OrderByDescending(b => b.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(b => new BillViewModel(b)).ToList();
            return new PagedResult<BillViewModel> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public BillViewModel? GetById(int id)
        {
            var e = _repo.GetAll().Include(b => b.Patient).FirstOrDefault(b => b.Id == id);
            return e == null ? null : new BillViewModel(e);
        }

        public void Insert(BillViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel();
            model.Id = 0;
            model.TotalBill = model.DoctorCharge + model.MedicineCharge + model.RoomCharge
                + model.OperationCharge + model.NursingCharge + model.LabCharge - model.Advance;
            _repo.Add(model);
            _repo.Save();
        }

        public void Update(BillViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;
            existing.BillNumber = vm.BillNumber; existing.PatientId = vm.PatientId;
            existing.InsuranceId = vm.InsuranceId; existing.Status = vm.Status;
            existing.PaidDate = vm.PaidDate;
            existing.DoctorCharge = vm.DoctorCharge; existing.MedicineCharge = vm.MedicineCharge;
            existing.RoomCharge = vm.RoomCharge; existing.OperationCharge = vm.OperationCharge;
            existing.NursingCharge = vm.NursingCharge; existing.LabCharge = vm.LabCharge;
            existing.Advance = vm.Advance; existing.NoOfDays = vm.NoOfDays;
            existing.TotalBill = vm.DoctorCharge + vm.MedicineCharge + vm.RoomCharge
                + vm.OperationCharge + vm.NursingCharge + vm.LabCharge - vm.Advance;
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
