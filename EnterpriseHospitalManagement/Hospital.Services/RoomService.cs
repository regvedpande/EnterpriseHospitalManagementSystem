using System.Collections.Generic;
using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;

namespace Hospital.Services
{
    public class RoomService : IRoomService
    {
        private readonly IGenericRepository<Room> _repo;

        public RoomService(IGenericRepository<Room> repo)
        {
            _repo = repo;
        }

        public PagedResult<RoomViewModel> GetAll(int pageNumber, int pageSize)
        {
            var list = _repo.GetAll().ToList();
            return new PagedResult<RoomViewModel>
            {
                Data = list
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RoomViewModel(r))
                    .ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = list.Count
            };
        }

        public RoomViewModel? GetRoomById(int roomId)
        {
            var model = _repo.GetById(roomId);
            return model == null ? null : new RoomViewModel(model);
        }

        public void InsertRoom(RoomViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel();
            model.Id = 0;
            _repo.Add(model);
            _repo.Save();
        }

        public void UpdateRoom(RoomViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null)
            {
                InsertRoom(vm);
                return;
            }
            existing.RoomNumber = vm.RoomNumber;
            existing.Type = vm.Type;
            existing.Status = vm.Status;
            existing.HospitalId = vm.HospitalId;
            _repo.Update(existing);
            _repo.Save();
        }

        public void DeleteRoom(int roomId)
        {
            var existing = _repo.GetById(roomId);
            if (existing == null) return;
            _repo.Delete(existing);
            _repo.Save();
        }
    }
}