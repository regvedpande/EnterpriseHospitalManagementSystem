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
    public class AppointmentService : IAppointmentService
    {
        private const string CacheAllKey     = "appt:all";
        private const string CacheDocPrefix  = "appt:doc:";
        private const string CachePatPrefix  = "appt:pat:";
        private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(2);

        private readonly IGenericRepository<Appointment> _repo;
        private readonly ICacheService        _cache;
        private readonly IBackgroundTaskQueue _queue;

        public AppointmentService(
            IGenericRepository<Appointment> repo,
            ICacheService        cache,
            IBackgroundTaskQueue queue)
        {
            _repo  = repo;
            _cache = cache;
            _queue = queue;
        }

        public PagedResult<AppointmentViewModel> GetAll(int pageNumber, int pageSize)
        {
            var key    = $"{CacheAllKey}:{pageNumber}:{pageSize}";
            var cached = _cache.Get<PagedResult<AppointmentViewModel>>(key);
            if (cached != null) return cached;

            var query = _repo.GetAll().Include(a => a.Doctor).Include(a => a.Patient);
            var total = query.Count();
            var items = query.OrderByDescending(a => a.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new AppointmentViewModel(a)).ToList();

            var result = new PagedResult<AppointmentViewModel>
                { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
            _cache.Set(key, result, Ttl);
            return result;
        }

        public PagedResult<AppointmentViewModel> GetByDoctor(string doctorId, int pageNumber, int pageSize)
        {
            var key    = $"{CacheDocPrefix}{doctorId}:{pageNumber}:{pageSize}";
            var cached = _cache.Get<PagedResult<AppointmentViewModel>>(key);
            if (cached != null) return cached;

            var query = _repo.GetAll().Include(a => a.Doctor).Include(a => a.Patient)
                             .Where(a => a.DoctorId == doctorId);
            var total = query.Count();
            var items = query.OrderByDescending(a => a.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new AppointmentViewModel(a)).ToList();

            var result = new PagedResult<AppointmentViewModel>
                { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
            _cache.Set(key, result, Ttl);
            return result;
        }

        public PagedResult<AppointmentViewModel> GetByPatient(string patientId, int pageNumber, int pageSize)
        {
            var key    = $"{CachePatPrefix}{patientId}:{pageNumber}:{pageSize}";
            var cached = _cache.Get<PagedResult<AppointmentViewModel>>(key);
            if (cached != null) return cached;

            var query = _repo.GetAll().Include(a => a.Doctor).Include(a => a.Patient)
                             .Where(a => a.PatientId == patientId);
            var total = query.Count();
            var items = query.OrderByDescending(a => a.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new AppointmentViewModel(a)).ToList();

            var result = new PagedResult<AppointmentViewModel>
                { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
            _cache.Set(key, result, Ttl);
            return result;
        }

        public AppointmentViewModel? GetById(int id)
        {
            var e = _repo.GetAll().Include(a => a.Doctor).Include(a => a.Patient)
                         .FirstOrDefault(a => a.Id == id);
            return e == null ? null : new AppointmentViewModel(e);
        }

        public void Insert(AppointmentViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel();
            model.Id          = 0;
            model.CreatedDate = DateTime.Now;
            _repo.Add(model);
            _repo.Save();

            InvalidateAppointmentCache(vm.DoctorId, vm.PatientId);

            // Publish domain event asynchronously (fire-and-forget via background queue)
            var appointmentId   = model.Id;
            var patientId       = vm.PatientId;
            var patientName     = vm.PatientName;
            var doctorName      = vm.DoctorName;
            var appointmentDate = vm.AppointmentDate;
            var apptType        = vm.Type;

            _queue.Enqueue(async (sp, ct) =>
            {
                var bus = sp.GetService(typeof(IMessageBus)) as IMessageBus;
                if (bus == null) return;
                await bus.PublishAsync("appointment.created",
                    new AppointmentCreatedMessage
                    {
                        AppointmentId   = appointmentId,
                        PatientId       = patientId ?? "",
                        PatientName     = patientName ?? "",
                        DoctorName      = doctorName  ?? "",
                        AppointmentDate = appointmentDate,
                        AppointmentType = apptType ?? ""
                    }, ct);
            });
        }

        public void Update(AppointmentViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null) return;

            var oldStatus = existing.Status;

            existing.Number          = vm.Number;
            existing.Type            = vm.Type;
            existing.AppointmentDate = vm.AppointmentDate;
            existing.Status          = vm.Status;
            existing.Description     = vm.Description;
            existing.Notes           = vm.Notes;
            existing.DoctorId        = vm.DoctorId;
            existing.PatientId       = vm.PatientId;
            _repo.Update(existing);
            _repo.Save();

            InvalidateAppointmentCache(vm.DoctorId, vm.PatientId);

            // Notify on status change
            if (oldStatus != vm.Status)
            {
                var apptId      = vm.Id;
                var patName     = vm.PatientName;
                var docName     = vm.DoctorName;
                var oldSt       = oldStatus.ToString();
                var newSt       = vm.Status.ToString();
                _queue.Enqueue(async (sp, ct) =>
                {
                    var bus = sp.GetService(typeof(IMessageBus)) as IMessageBus;
                    if (bus == null) return;
                    await bus.PublishAsync("appointment.status_changed",
                        new AppointmentStatusChangedMessage
                        {
                            AppointmentId = apptId,
                            PatientName   = patName ?? "",
                            OldStatus     = oldSt,
                            NewStatus     = newSt,
                            DoctorName    = docName ?? ""
                        }, ct);
                });
            }
        }

        public void Delete(int id)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return;
            var doctorId  = existing.DoctorId;
            var patientId = existing.PatientId;
            _repo.Delete(existing);
            _repo.Save();
            InvalidateAppointmentCache(doctorId, patientId);
        }

        private void InvalidateAppointmentCache(string? doctorId, string? patientId)
        {
            // Invalidate first 10 pages for all common page sizes
            int[] sizes = [10, 20, 50, 100, 1000];
            for (int p = 1; p <= 10; p++)
                foreach (var sz in sizes)
                    _cache.Remove($"{CacheAllKey}:{p}:{sz}");

            if (!string.IsNullOrEmpty(doctorId))
                for (int p = 1; p <= 10; p++)
                    foreach (var sz in sizes)
                        _cache.Remove($"{CacheDocPrefix}{doctorId}:{p}:{sz}");

            if (!string.IsNullOrEmpty(patientId))
                for (int p = 1; p <= 10; p++)
                    foreach (var sz in sizes)
                        _cache.Remove($"{CachePatPrefix}{patientId}:{p}:{sz}");
        }
    }
}
