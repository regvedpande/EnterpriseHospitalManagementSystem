using System.Linq;
using Hospital.Utilities;
using Hospital.ViewModels;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;

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

        public TimingViewModel GetTimingById(int timingId)
        {
            var model = _repo.GetById(timingId);
            return model == null ? null : new TimingViewModel(model);
        }

        public void AddTiming(TimingViewModel timing)
        {
            new Timing
            {
                Id = timingViewModel.Id,
                Day = timingViewModel.Day,
                StartTime = timingViewModel.StartTime,
                EndTime = timingViewModel.EndTime
            }

        }

        public void UpdateTiming(TimingViewModel timing)
        {
            _repo.Update(timing.ToModel());
            _repo.Save();
        }

        public void DeleteTiming(int timingId)
        {
            _repo.Delete(timingId);
            _repo.Save();
        }
    }
}
