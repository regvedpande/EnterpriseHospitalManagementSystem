// Hospital.Services/HospitalInfoService.cs
using System;
using System.Collections.Generic;
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
            var list = _repo.GetAll()?.ToList() ?? new List<HospitalInfo>();

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

        public HospitalInfoViewModel GetHospitalById(int hospitalId)
        {
            var model = _repo.GetById(hospitalId);
            return model == null ? null : new HospitalInfoViewModel(model);
        }

        public void InsertHospitalInfo(HospitalInfoViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ConvertViewModel();
            var addMi = _repo.GetType().GetMethod("Add");
            if (addMi != null) addMi.Invoke(_repo, new object[] { model });
            else _repo.AddOrUpdate(model);

            _repo.SaveChanges();
        }

        public void UpdateHospitalInfo(HospitalInfoViewModel vm)
        {
            if (vm == null) return;
            var idProp = vm.GetType().GetProperty("Id") ?? vm.GetType().GetProperty("ID");
            var id = 0;
            if (idProp != null)
            {
                var v = idProp.GetValue(vm);
                id = v == null ? 0 : Convert.ToInt32(v);
            }

            var existing = _repo.GetById(id);
            if (existing == null)
            {
                InsertHospitalInfo(vm);
                return;
            }

            var newModel = vm.ConvertViewModel();
            foreach (var p in newModel.GetType().GetProperties())
            {
                var target = existing.GetType().GetProperty(p.Name);
                if (target != null && target.CanWrite)
                {
                    try { target.SetValue(existing, p.GetValue(newModel)); } catch { }
                }
            }

            var updateMi = _repo.GetType().GetMethod("Update");
            if (updateMi != null) updateMi.Invoke(_repo, new object[] { existing });

            _repo.SaveChanges();
        }

        public void DeleteHospitalInfo(int hospitalId)
        {
            var existing = _repo.GetById(hospitalId);
            if (existing == null) return;

            var deleteMi = _repo.GetType().GetMethod("Delete", new[] { existing.GetType() });
            if (deleteMi != null)
                deleteMi.Invoke(_repo, new object[] { existing });
            else
            {
                var deleteById = _repo.GetType().GetMethod("Delete", new[] { typeof(int) });
                deleteById?.Invoke(_repo, new object[] { hospitalId });
            }

            _repo.SaveChanges();
        }
    }
}
