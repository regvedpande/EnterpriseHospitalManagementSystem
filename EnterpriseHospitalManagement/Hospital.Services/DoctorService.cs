using cloudscribe.Pagination.Models;
using EnterpriseHospitalManagement.Hospital.ViewModels;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hospital.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedResult<TimingViewModel> GetAllTimings(int pageNumber, int pageSize)
        {
            int totalCount;
            var modelList = _unitOfWork.Repository<Timing>()
                .GetAll(includeProperties: "Doctor")
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            totalCount = _unitOfWork.Repository<Timing>().GetAll().Count();

            var vmList = ConvertModelToViewModelList(modelList);

            return new PagedResult<TimingViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public TimingViewModel GetTimingById(int timingId)
        {
            var model = _unitOfWork.Repository<Timing>().GetById(timingId);
            return new TimingViewModel(model);
        }

        public void AddTiming(TimingViewModel timing)
        {
            var model = timing.ConvertViewModel();
            _unitOfWork.Repository<Timing>().Add(model);
            _unitOfWork.Save();
        }

        public void UpdateTiming(TimingViewModel timing)
        {
            var model = timing.ConvertViewModel();
            _unitOfWork.Repository<Timing>().Update(model);
            _unitOfWork.Save();
        }

        public void DeleteTiming(int timingId)
        {
            var model = _unitOfWork.Repository<Timing>().GetById(timingId);
            _unitOfWork.Repository<Timing>().Delete(model);
            _unitOfWork.Save();
        }

        private List<TimingViewModel> ConvertModelToViewModelList(List<Timing> modelList)
        {
            return modelList.Select(t => new TimingViewModel(t)).ToList();
        }
    }
}
