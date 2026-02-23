using Hospital.Utilities;
using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IRoomService
    {
        PagedResult<RoomViewModel> GetAll(int pageNumber, int pageSize);
        RoomViewModel? GetRoomById(int roomId);
        void InsertRoom(RoomViewModel room);
        void UpdateRoom(RoomViewModel room);
        void DeleteRoom(int roomId);
    }
}