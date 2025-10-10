using Hospital.Models;
using Hospital.Repositories.Implementation;
using Hospital.Utilities;
using Hospital.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Services
{
    public class HospitalInfoService : IHospitalInfo
    {
        public IUnitOfWork _unitOfWork;
        public HospitalInfoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ViewModels.HospitalInfoViewModel DeleteHospitalInfo(int id)
        {
            var model = _unitOfWork.GenericRepository<Models.HospitalInfoViewModel>().GetById(id);
            if (model != null)
            {
                _unitOfWork.GenericRepository<Models.HospitalInfoViewModel>().Delete(model);
                _unitOfWork.Save();
                return new ViewModels.HospitalInfoViewModel(model);
            }
            return null;
        }

        public PagedResult<ViewModels.HospitalInfoViewModel> GetAll(int pageNumber, int pageSize)
        {
            int totalCount = 0;
            List<ViewModels.HospitalInfoViewModel> vmList = new List<ViewModels.HospitalInfoViewModel>();

            try
            {
                int ExcludeRecords = (pageSize * pageNumber) - pageSize;

                var modelList = _unitOfWork.GenericRepository<Models.HospitalInfoViewModel>().GetAll()
                    .Skip(ExcludeRecords).Take(pageSize).ToList();

                totalCount = _unitOfWork.GenericRepository<Models.HospitalInfoViewModel>().GetAll().Count();

                vmList = ConvertModelToViewModelList(modelList);
            }
            catch (Exception)
            {
                // Optionally log or handle exception
            }

            var Result = new PagedResult<ViewModels.HospitalInfoViewModel>()
            {
                Data = vmList,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalCount
            };
            return Result;
        }

        public ViewModels.HospitalInfoViewModel GetHospitalbyId(int HospitalId)
        {
            var model = _unitOfWork.GenericRepository<Models.HospitalInfoViewModel>().GetById(HospitalId);
            var vm = new ViewModels.HospitalInfoViewModel(model);
            return vm;
        }

        public ViewModels.HospitalInfoViewModel InsertHospitalInfo(ViewModels.HospitalInfoViewModel model)
        {
            var hospitalModel = new ViewModels.HospitalInfoViewModel().ConvertViewModel(model);
            _unitOfWork.GenericRepository<Models.HospitalInfoViewModel>().AddAsync(hospitalModel);
            _unitOfWork.Save();
            return new ViewModels.HospitalInfoViewModel(hospitalModel);
        }

        public ViewModels.HospitalInfoViewModel UpdateHospitalInfo(ViewModels.HospitalInfoViewModel model)
        {
            var hospitalModel = new ViewModels.HospitalInfoViewModel().ConvertViewModel(model);
            var ModelById = _unitOfWork.GenericRepository<Models.HospitalInfoViewModel>().GetById(hospitalModel.Id);

            if (ModelById != null)
            {
                ModelById.Name = hospitalModel.Name;
                ModelById.City = hospitalModel.City;
                ModelById.Country = hospitalModel.Country;
                ModelById.PinCode = hospitalModel.PinCode;

                _unitOfWork.GenericRepository<Models.HospitalInfoViewModel>().Update(ModelById);
                _unitOfWork.Save();
            }

            return new ViewModels.HospitalInfoViewModel(ModelById);
        }

        private List<ViewModels.HospitalInfoViewModel> ConvertModelToViewModelList(List<Models.HospitalInfoViewModel> modelList)
        {
            return modelList.Select(x => new ViewModels.HospitalInfoViewModel(x)).ToList();
        }
    }
}
