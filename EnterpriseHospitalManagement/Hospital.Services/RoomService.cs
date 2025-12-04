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
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedResult<RoomViewModel> GetAll(int pageNumber, int pageSize)
        {
            int totalCount;
            var modelList = _unitOfWork.Repository<Room>()
                .GetAll(includeProperties: "Hospital")
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            totalCount = _unitOfWork.Repository<Room>().GetAll().Count();

            var vmList = ConvertModelToViewModelList(modelList);

            return new PagedResult<RoomViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public RoomViewModel GetRoomById(int roomId)
        {
            var model = _unitOfWork.Repository<Room>().GetById(roomId);
            return new RoomViewModel(model);
        }

        public void InsertRoom(RoomViewModel room)
        {
            var model = room.ConvertViewModel();
            _unitOfWork.Repository<Room>().Add(model);
            _unitOfWork.Save();
        }

        public void UpdateRoom(RoomViewModel room)
        {
            var model = room.ConvertViewModel();
            _unitOfWork.Repository<Room>().Update(model);
            _unitOfWork.Save();
        }

        public void DeleteRoom(int id)
        {
            var model = _unitOfWork.Repository<Room>().GetById(id);
            _unitOfWork.Repository<Room>().Delete(model);
            _unitOfWork.Save();
        }

        private List<RoomViewModel> ConvertModelToViewModelList(List<Room> modelList)
        {
            return modelList.Select(r => new RoomViewModel(r)).ToList();
        }
    }
}
