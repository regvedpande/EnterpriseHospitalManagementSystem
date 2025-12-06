using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; }

        [Required]
        [Display(Name = "Room Type")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Status")]
        public int Status { get; set; }

        [Required]
        [Display(Name = "Hospital")]
        public int HospitalInfoId { get; set; }

        public RoomViewModel()
        {
        }

        public RoomViewModel(Room model)
        {
            Id = model.Id;
            RoomNumber = model.RoomNumber;
            Type = model.Type;
            Status = model.Status;
            HospitalInfoId = model.HospitalId;
        }

        public Room ConvertViewModel()
        {
            return new Room
            {
                Id = Id,
                RoomNumber = RoomNumber,
                Type = Type,
                Status = Status,
                HospitalId = HospitalInfoId
            };
        }
    }
}
