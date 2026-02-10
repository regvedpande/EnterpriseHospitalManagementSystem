using System.Linq;
using Hospital.Utilities;
using Hospital.ViewModels;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;

namespace Hospital.Services
{
    public class HospitalInfoService : IHospitalInfoService
    {
        private readonly IGenericRepository<HospitalInfo> _repo;

        public HospitalInfoService(IGenericRepository<HospitalInfo> repo)
        {
            _repo = repo;
        }

        public PagedResult<HospitalInfoViewModel> GetAll(int pageNumber, int pageSize)
        {
            var list = _repo.GetAll().ToList();

            return new PagedResult<HospitalInfoViewModel>
            {
                Data = list
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new HospitalInfoViewModel(x))
                    .ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = list.Count
            };
        }

        public HospitalInfoViewModel GetHospitalById(int hospitalId)
        {
            var model = _repo.GetById(hospitalId);
            return model == null ? null : new HospitalInfoViewModel(model);
        }

        public void InsertHospitalInfo(HospitalInfoViewModel hospital)
        {
            _repo.Insert(hospital.ToModel());
            _repo.Save();
        }

        public void UpdateHospitalInfo(HospitalInfoViewModel hospital)
        {
            _repo.Update(hospital.ToModel());
            _repo.Save();
        }

        public void DeleteHospitalInfo(int hospitalId)
        {
            _repo.Delete(hospitalId);
            _repo.Save();
        }
    }
}
