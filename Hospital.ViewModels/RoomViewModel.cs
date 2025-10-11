using System;
using Hospital.Models;   // assuming Room and HospitalInfo live here

namespace Hospital.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }

        // Foreign key + navigation property
        public int HospitalId { get; set; }
        public HospitalInfo HospitalInfo { get; set; }

        // Default constructor
        public RoomViewModel() { }

        // Map from entity → ViewModel
        public RoomViewModel(Room model)
        {
            Id = model.Id;
            RoomNumber = model.RoomNumber;
            Type = model.Type;
            Status = model.Status;
            HospitalId = model.HospitalId;
            HospitalInfo = model.Hospital;   // requires EF Include() if lazy loading is off
        }

        // Map from ViewModel → entity
        public Room ConvertView()
        {
            return new Room
            {
                Id = this.Id,
                RoomNumber = this.RoomNumber,
                Type = this.Type,
                Status = this.Status,
                HospitalId = this.HospitalId,
                Hospital = model.HospitalInfo
                // Hospital is not set here to avoid EF tracking issues
            };
        }

        // Convenience: flatten hospital info for display
        public string HospitalName => HospitalInfo?.Name ?? "N/A";
        public string HospitalCity => HospitalInfo?.City ?? "N/A";
    }
}