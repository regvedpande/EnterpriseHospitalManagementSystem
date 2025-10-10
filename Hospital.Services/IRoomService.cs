using cloudscribe.Pagination.Models;
using Hospital.ViewModels;
using Hospital.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Models;

namespace Hospital.Services
{
    public interface IRoomService
    {
        PagedResult<RoomViewModel> GetAll(int pageNumber, int pageSize);

        RoomViewModel GetRoomById(int id);

        void AddRoom(RoomViewModel Room);

        void UpdateRoom(RoomViewModel Room);

        void DeleteRoom(int id);
    }
}
