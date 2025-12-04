using cloudscribe.Pagination.Models;
using EnterpriseHospitalManagement.Hospital.Services.Interfaces;
using EnterpriseHospitalManagement.Hospital.ViewModels;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hospital.Services
{
    public class HospitalInfoService : IHospitalInfoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HospitalInfoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedResult<HospitalInfoViewModel> GetAll(int pageNumber, int pageSize)
        {
            int totalCount;
            var modelList = _unitOfWork.Repository<HospitalInfo>()
                .GetAll()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            totalCount = _unitOfWork.Repository<HospitalInfo>().GetAll().Count();

            var vmList = ConvertModelToViewModelList(modelList);

            return new PagedResult<HospitalInfoViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public HospitalInfoViewModel GetHospitalById(int hospitalId)
        {
            var model = _unitOfWork.Repository<HospitalInfo>().GetById(hospitalId);
            return new HospitalInfoViewModel(model);
        }

        public void InsertHospitalInfo(HospitalInfoViewModel hospital)
        {
            var model = hospital.ConvertViewModel();
            _unitOfWork.Repository<HospitalInfo>().Add(model);
            _unitOfWork.Save();
        }

        public void UpdateHospitalInfo(HospitalInfoViewModel hospital)
        {
            var model = hospital.ConvertViewModel();
            _unitOfWork.Repository<HospitalInfo>().Update(model);
            _unitOfWork.Save();
        }

        public void DeleteHospitalInfo(int id)
        {
            var model = _unitOfWork.Repository<HospitalInfo>().GetById(id);
            _unitOfWork.Repository<HospitalInfo>().Delete(model);
            _unitOfWork.Save();
        }

        private List<HospitalInfoViewModel> ConvertModelToViewModelList(List<HospitalInfo> modelList)
        {
            return modelList.Select(h => new HospitalInfoViewModel(h)).ToList();
        }
    }
}
