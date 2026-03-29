using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Hospital.Web.Infrastructure.Caching;
using Hospital.Web.Infrastructure.Messaging;
using Hospital.Web.Infrastructure.Messaging.Messages;
using Hospital.Web.Infrastructure.Queue;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Services
{
    public class BillService : IBillService
    {
        private const string CacheAllKey    = "bill:all";
        private const string CachePatPrefix = "bill:pat:";
        private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(3);

        private readonly IGenericRepository<Bill> _repo;
        private readonly ICacheService        _cache;
        private readonly IBackgroundTaskQueue _queue;

        public BillService(
            IGenericRepository<Bill> repo,
            ICacheService        cache,
            IBackgroundTaskQueue queue)
        {
            _repo  = repo;
            _cache = cache;
            _queue = queue;
        }

        public PagedResult<BillViewModel> GetAll(int pageNumber, int pageSize)
        {
            var key    = $"{CacheAllKey}:{pageNumber}:{pageSize}";
            var cached = _cache.Get<PagedResult<BillViewModel>>(key);
            if (cached != null) return cached;

            var query = _repo.GetAll().Include(b => b.Patient);
            var total = query.Count();
            var items = query.OrderByDescending(b => b.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(b => new BillViewModel(b)).ToList();

            var result = new PagedResult<BillViewModel>
                { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
            _cache.Set(key, result, Ttl);
            return result;
        }

        public PagedResult<BillViewModel> GetByPatient(string patientId, int pageNumber, int pageSize)
        {
            var key    = $"{CachePatPrefix}{patientId}:{pageNumber}:{pageSize}";
            var cached = _cache.Get<PagedResult<BillViewModel>>(key);
            if (cached != null) return cached;

            var query = _repo.GetAll().Include(b => b.Patient).Where(b => b.PatientId == patientId);
            var total = query.Count();
            var items = query.OrderByDescending(b => b.CreatedDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(b => new BillViewModel(b)).ToList();

            var result = new PagedResult<BillViewModel>
                { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
            _cache.Set(key, result, Ttl);
            return result;
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
            model.Id        = 0;
            model.TotalBill = model.DoctorCharge + model.MedicineCharge + model.RoomCharge
                + model.OperationCharge + model.NursingCharge + model.LabCharge - model.Advance;
            _repo.Add(model);
            _repo.Save();

            InvalidateBillCache(vm.PatientId);

            // Publish billing event asynchronously
            var billId    = model.Id;
            var billNum   = vm.BillNumber.ToString();
            var patId     = vm.PatientId;
            var patName   = vm.PatientName;
            var total     = model.TotalBill;
            _queue.Enqueue(async (sp, ct) =>
            {
                var bus = sp.GetService(typeof(IMessageBus)) as IMessageBus;
                if (bus == null) return;
                await bus.PublishAsync("bill.generated",
                    new BillGeneratedMessage
                    {
                        BillId      = billId,
                        PatientId   = patId ?? "",
                        PatientName = patName ?? "",
                        TotalAmount = total,
                        BillNumber  = billNum
                    }, ct);
            });
        }

        public void Update(BillViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;

            existing.BillNumber     = vm.BillNumber;
            existing.PatientId      = vm.PatientId;
            existing.InsuranceId    = vm.InsuranceId;
            existing.Status         = vm.Status;
            existing.PaidDate       = vm.PaidDate;
            existing.DoctorCharge   = vm.DoctorCharge;
            existing.MedicineCharge = vm.MedicineCharge;
            existing.RoomCharge     = vm.RoomCharge;
            existing.OperationCharge = vm.OperationCharge;
            existing.NursingCharge  = vm.NursingCharge;
            existing.LabCharge      = vm.LabCharge;
            existing.Advance        = vm.Advance;
            existing.NoOfDays       = vm.NoOfDays;
            existing.TotalBill      = vm.DoctorCharge + vm.MedicineCharge + vm.RoomCharge
                + vm.OperationCharge + vm.NursingCharge + vm.LabCharge - vm.Advance;
            _repo.Update(existing);
            _repo.Save();

            InvalidateBillCache(vm.PatientId);
        }

        public void Delete(int id)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return;
            var patientId = existing.PatientId;
            _repo.Delete(existing);
            _repo.Save();
            InvalidateBillCache(patientId);
        }

        private void InvalidateBillCache(string? patientId)
        {
            int[] sizes = [10, 20, 50, 100, 1000];
            for (int p = 1; p <= 10; p++)
                foreach (var sz in sizes)
                    _cache.Remove($"{CacheAllKey}:{p}:{sz}");

            if (!string.IsNullOrEmpty(patientId))
                for (int p = 1; p <= 10; p++)
                    foreach (var sz in sizes)
                        _cache.Remove($"{CachePatPrefix}{patientId}:{p}:{sz}");
        }
    }
}
