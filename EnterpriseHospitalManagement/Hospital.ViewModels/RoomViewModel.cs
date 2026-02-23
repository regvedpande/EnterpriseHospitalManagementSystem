using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; } = "";

        [Required]
        public string Type { get; set; } = "";

        /// <summary>0 = Available, 1 = Occupied, 2 = Maintenance</summary>
        [Required]
        public int Status { get; set; }

        [Required]
        [Display(Name = "Hospital")]
        public int HospitalId { get; set; }

        public RoomViewModel() { }

        public RoomViewModel(Room model)
        {
            if (model == null) return;
            Id = model.Id;
            RoomNumber = model.RoomNumber;
            Type = model.Type;
            Status = model.Status;
            HospitalId = model.HospitalId;
        }

        public Room ToModel() => new Room
        {
            Id = this.Id,
            RoomNumber = this.RoomNumber,
            Type = this.Type,
            Status = this.Status,
            HospitalId = this.HospitalId
        };
    }
}