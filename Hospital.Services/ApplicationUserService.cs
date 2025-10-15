using cloudscribe.Pagination.Models;
using Contracts.Services;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Repositories.Implementation;
using Hospital.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApplicationUserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedResult<ApplicationUserViewModel> GetAll(int PageNumber, int PageSize)
        {
            var vm = new ApplicationUserViewModel();
            int totalCount;
            List<ApplicationUserViewModel> vmList = new List<ApplicationUserViewModel>();

            try
            {
                int ExcludeRecords = (PageSize * PageNumber) - PageSize;

                var modelList = _unitOfWork.GenericRepository<ApplicationUser>()
                    .GetAll()
                    .Skip(ExcludeRecords)
                    .Take(PageSize)
                    .ToList();

                totalCount = _unitOfWork.GenericRepository<ApplicationUser>()
                    .GetAll()
                    .ToList()
                    .Count;

                vmList = ConvertModelToViewModelList(modelList);
            }
            catch (Exception)
            {
                throw;
            }

            var result = new PagedResult<ApplicationUserViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            return result;
        }

        public PagedResult<ApplicationUserViewModel> GetAllDoctor(int PageNumber, int PageSize)
        {
            var vm = new ApplicationUserViewModel();
            int totalCount;
            List<ApplicationUserViewModel> vmList = new List<ApplicationUserViewModel>();

            try
            {
                int ExcludeRecords = (PageSize * PageNumber) - PageSize;

                var modelList = _unitOfWork.GenericRepository<ApplicationUser>()
                    .GetAll(x => x.IsDoctor == true)
                    .Skip(ExcludeRecords)
                    .Take(PageSize)
                    .ToList();

                totalCount = _unitOfWork.GenericRepository<ApplicationUser>()
                    .GetAll(x => x.IsDoctor == true)
                    .ToList()
                    .Count;

                vmList = ConvertModelToViewModelList(modelList);
            }
            catch (Exception)
            {
                throw;
            }

            var result = new PagedResult<ApplicationUserViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            return result;
        }

        public PagedResult<ApplicationUserViewModel> GetAllPatient(int PageNumber, int PageSize)
        {
            var vm = new ApplicationUserViewModel();
            int totalCount;
            List<ApplicationUserViewModel> vmList = new List<ApplicationUserViewModel>();

            try
            {
                int ExcludeRecords = (PageSize * PageNumber) - PageSize;

                var modelList = _unitOfWork.GenericRepository<ApplicationUser>()
                    .GetAll(x => x.IsDoctor == false)
                    .Skip(ExcludeRecords)
                    .Take(PageSize)
                    .ToList();

                totalCount = _unitOfWork.GenericRepository<ApplicationUser>()
                    .GetAll(x => x.IsDoctor == false)
                    .ToList()
                    .Count;

                vmList = ConvertModelToViewModelList(modelList);
            }
            catch (Exception)
            {
                throw;
            }

            var result = new PagedResult<ApplicationUserViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            return result;
        }

        public PagedResult<ApplicationUserViewModel> SearchDoctor(int PageNumber, int PageSize, string Spicility = null)
        {
            var vm = new ApplicationUserViewModel();
            int totalCount;
            List<ApplicationUserViewModel> vmList = new List<ApplicationUserViewModel>();

            try
            {
                int ExcludeRecords = (PageSize * PageNumber) - PageSize;

                // Build filtered query: doctors and optional Spicility match
                var query = _unitOfWork.GenericRepository<ApplicationUser>()
                    .GetAll(x => x.IsDoctor == true && (string.IsNullOrEmpty(Spicility) || x.Spicility == Spicility));

                var modelList = query
                    .Skip(ExcludeRecords)
                    .Take(PageSize)
                    .ToList();

                totalCount = query.ToList().Count;

                vmList = ConvertModelToViewModelList(modelList);
            }
            catch (Exception)
            {
                throw;
            }

            var result = new PagedResult<ApplicationUserViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            return result;
        }

        private List<ApplicationUserViewModel> ConvertModelToViewModelList(List<ApplicationUser> modelList)
        {
            return modelList.Select(x => new ApplicationUserViewModel(x)).ToList();
        }
    }
}
