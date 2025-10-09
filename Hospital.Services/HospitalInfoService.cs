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

        public HospitalInfoViewModel DeleteHospitalInfo(int id)
        {
            var model = _unitOfWork.GenericRepository<HospitalInfo>().GetById(id);
            if (model != null)
            {
                _unitOfWork.GenericRepository<HospitalInfo>().Delete(model);
                _unitOfWork.Save();
                return new HospitalInfoViewModel(model);
            }
            return null;
        }

        public PagedResult<HospitalInfoViewModel> GetAll(int pageNumber, int pageSize)
        {
            int totalCount = 0;
            List<HospitalInfoViewModel> vmList = new List<HospitalInfoViewModel>();

            try
            {
                int ExcludeRecords = (pageSize * pageNumber) - pageSize;

                var modelList = _unitOfWork.GenericRepository<HospitalInfo>().GetAll()
                    .Skip(ExcludeRecords).Take(pageSize).ToList();

                totalCount = _unitOfWork.GenericRepository<HospitalInfo>().GetAll().Count();

                vmList = ConvertModelToViewModelList(modelList);
            }
            catch (Exception)
            {
                // Optionally log or handle exception
            }

            var Result = new PagedResult<HospitalInfoViewModel>()
            {
                Data = vmList,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalCount
            };
            return Result;
        }

        public HospitalInfoViewModel GetHospitalbyId(int HospitalId)
        {
            var model = _unitOfWork.GenericRepository<HospitalInfo>().GetById(HospitalId);
            var vm = new HospitalInfoViewModel(model);
            return vm;
        }

        public HospitalInfoViewModel InsertHospitalInfo(HospitalInfoViewModel model)
        {
            var hospitalModel = new HospitalInfoViewModel().ConvertViewModel(model);
            _unitOfWork.GenericRepository<HospitalInfo>().AddAsync(hospitalModel);
            _unitOfWork.Save();
            return new HospitalInfoViewModel(hospitalModel);
        }

        public HospitalInfoViewModel UpdateHospitalInfo(HospitalInfoViewModel model)
        {
            var hospitalModel = new HospitalInfoViewModel().ConvertViewModel(model);
            var ModelById = _unitOfWork.GenericRepository<HospitalInfo>().GetById(hospitalModel.Id);

            if (ModelById != null)
            {
                ModelById.Name = hospitalModel.Name;
                ModelById.City = hospitalModel.City;
                ModelById.Country = hospitalModel.Country;
                ModelById.PinCode = hospitalModel.PinCode;

                _unitOfWork.GenericRepository<HospitalInfo>().Update(ModelById);
                _unitOfWork.Save();
            }

            return new HospitalInfoViewModel(ModelById);
        }

        private List<HospitalInfoViewModel> ConvertModelToViewModelList(List<HospitalInfo> modelList)
        {
            return modelList.Select(x => new HospitalInfoViewModel(x)).ToList();
        }
    }
}
