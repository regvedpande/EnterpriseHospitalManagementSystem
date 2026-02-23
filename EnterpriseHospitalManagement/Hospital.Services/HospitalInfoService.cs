using System.Collections.Generic;
using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;

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
                    .Select(h => new HospitalInfoViewModel(h))
                    .ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = list.Count
            };
        }

        public HospitalInfoViewModel? GetHospitalById(int hospitalId)
        {
            var model = _repo.GetById(hospitalId);
            return model == null ? null : new HospitalInfoViewModel(model);
        }

        public void InsertHospitalInfo(HospitalInfoViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel();
            model.Id = 0;
            _repo.Add(model);
            _repo.Save();
        }

        public void UpdateHospitalInfo(HospitalInfoViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null)
            {
                InsertHospitalInfo(vm);
                return;
            }
            existing.Name = vm.Name;
            existing.Type = vm.Type;
            existing.City = vm.City;
            existing.PinCode = vm.PinCode;
            existing.Country = vm.Country;
            _repo.Update(existing);
            _repo.Save();
        }

        public void DeleteHospitalInfo(int hospitalId)
        {
            var existing = _repo.GetById(hospitalId);
            if (existing == null) return;
            _repo.Delete(existing);
            _repo.Save();
        }
    }
}