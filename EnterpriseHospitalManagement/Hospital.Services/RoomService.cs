// Hospital.Services/RoomService.cs
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
    public class RoomService : IRoomService
    {
        private readonly IGenericRepository<Room> _repo;

        public RoomService(IGenericRepository<Room> repo)
        {
            _repo = repo;
        }

        public PagedResult<RoomViewModel> GetAll(int pageNumber, int pageSize)
        {
            var list = _repo.GetAll()?.ToList() ?? new List<Room>();

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

        public RoomViewModel GetRoomById(int roomId)
        {
            var model = _repo.GetById(roomId);
            return model == null ? null : new RoomViewModel(model);
        }

        public void InsertRoom(RoomViewModel roomViewModel)
        {
            if (roomViewModel == null) return;

            // Use ConvertViewModel extension implemented above
            var model = roomViewModel.ConvertViewModel();
            var addMi = _repo.GetType().GetMethod("Add");
            if (addMi != null)
            {
                addMi.Invoke(_repo, new object[] { model });
            }
            else
            {
                // fallback if repo exposes Add
                _repo.AddOrUpdate(model);
            }

            _repo.SaveChanges();
        }

        public void UpdateRoom(RoomViewModel roomViewModel)
        {
            if (roomViewModel == null) return;

            // get id (common property names)
            var idProp = roomViewModel.GetType().GetProperty("Id") ?? roomViewModel.GetType().GetProperty("ID");
            var id = 0;
            if (idProp != null)
            {
                var v = idProp.GetValue(roomViewModel);
                id = v == null ? 0 : Convert.ToInt32(v);
            }

            var existing = _repo.GetById(id);
            if (existing == null)
            {
                InsertRoom(roomViewModel);
                return;
            }

            // copy properties by name
            var newModel = roomViewModel.ConvertViewModel();
            // copy only properties that exist in model (reflection)
            foreach (var p in newModel.GetType().GetProperties())
            {
                var target = existing.GetType().GetProperty(p.Name);
                if (target != null && target.CanWrite)
                {
                    try { target.SetValue(existing, p.GetValue(newModel)); } catch { }
                }
            }

            var updateMi = _repo.GetType().GetMethod("Update");
            if (updateMi != null)
                updateMi.Invoke(_repo, new object[] { existing });

            _repo.SaveChanges();
        }

        public void DeleteRoom(int roomId)
        {
            var existing = _repo.GetById(roomId);
            if (existing == null) return;

            var deleteMi = _repo.GetType().GetMethod("Delete", new[] { existing.GetType() });
            if (deleteMi != null)
                deleteMi.Invoke(_repo, new object[] { existing });
            else
            {
                // fallback - try Delete(int) if available
                var deleteById = _repo.GetType().GetMethod("Delete", new[] { typeof(int) });
                deleteById?.Invoke(_repo, new object[] { roomId });
            }

            _repo.SaveChanges();
        }
    }
}
