using cloudscribe.Pagination.Models;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hospital.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApplicationUserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedResult<ApplicationUserViewModel> GetAll(int pageNumber, int pageSize)
        {
            int totalCount;
            var modelList = _unitOfWork.Repository<ApplicationUser>()
                .GetAll()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            totalCount = _unitOfWork.Repository<ApplicationUser>().GetAll().Count();

            var vmList = ConvertModelToViewModelList(modelList);

            return new PagedResult<ApplicationUserViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public PagedResult<ApplicationUserViewModel> GetAllDoctors(int pageNumber, int pageSize)
        {
            int totalCount;
            var modelList = _unitOfWork.Repository<ApplicationUser>()
                .GetAll(u => u.IsDoctor)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            totalCount = _unitOfWork.Repository<ApplicationUser>().GetAll(u => u.IsDoctor).Count();

            var vmList = ConvertModelToViewModelList(modelList);

            return new PagedResult<ApplicationUserViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public PagedResult<ApplicationUserViewModel> GetAllPatients(int pageNumber, int pageSize)
        {
            int totalCount;
            var modelList = _unitOfWork.Repository<ApplicationUser>()
                .GetAll(u => !u.IsDoctor)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            totalCount = _unitOfWork.Repository<ApplicationUser>().GetAll(u => !u.IsDoctor).Count();

            var vmList = ConvertModelToViewModelList(modelList);

            return new PagedResult<ApplicationUserViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public PagedResult<ApplicationUserViewModel> SearchDoctors(int pageNumber, int pageSize, string specialty = null)
        {
            int totalCount;
            var query = _unitOfWork.Repository<ApplicationUser>()
                .GetAll(u => u.IsDoctor && (string.IsNullOrEmpty(specialty) || u.Specialist == specialty));

            var modelList = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            totalCount = query.Count();

            var vmList = ConvertModelToViewModelList(modelList);

            return new PagedResult<ApplicationUserViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        private List<ApplicationUserViewModel> ConvertModelToViewModelList(List<ApplicationUser> modelList)
        {
            return modelList.Select(u => new ApplicationUserViewModel(u)).ToList();
        }
    }
}
