using System.Collections.Generic;
using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;

namespace Hospital.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IGenericRepository<Timing> _repo;

        public DoctorService(IGenericRepository<Timing> repo)
        {
            _repo = repo;
        }

        public PagedResult<TimingViewModel> GetAllTimings(int pageNumber, int pageSize)
        {
            var list = _repo.GetAll().ToList();
            return new PagedResult<TimingViewModel>
            {
                Data = list
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new TimingViewModel(x))
                    .ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = list.Count
            };
        }

        public TimingViewModel? GetTimingById(int timingId)
        {
            var model = _repo.GetById(timingId);
            return model == null ? null : new TimingViewModel(model);
        }

        public void AddTiming(TimingViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel();
            model.Id = 0;
            _repo.Add(model);
            _repo.Save();
        }

        public void UpdateTiming(TimingViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null)
            {
                AddTiming(vm);
                return;
            }
            existing.DoctorId = vm.DoctorId;
            existing.ScheduleDate = vm.ScheduleDate;
            existing.MorningShiftStartTime = vm.MorningShiftStartTime;
            existing.MorningShiftEndTime = vm.MorningShiftEndTime;
            existing.AfternoonShiftStartTime = vm.AfternoonShiftStartTime;
            existing.AfternoonShiftEndTime = vm.AfternoonShiftEndTime;
            existing.Duration = vm.Duration;
            existing.Status = vm.Status;
            _repo.Update(existing);
            _repo.Save();
        }

        public void DeleteTiming(int timingId)
        {
            var existing = _repo.GetById(timingId);
            if (existing == null) return;
            _repo.Delete(existing);
            _repo.Save();
        }
    }
}