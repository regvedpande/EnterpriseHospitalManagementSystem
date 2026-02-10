using Hospital.Models;

namespace Hospital.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }

        public RoomViewModel() { }

        public RoomViewModel(Room model)
        {
            Id = model.Id;
            RoomNumber = model.RoomNumber;
        }

        public Room ToModel()
        {
            return new Room
            {
                Id = Id,
                RoomNumber = RoomNumber
            };
        }
    }
}
