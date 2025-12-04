using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class HospitalInfoViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string PinCode { get; set; }

        [Required]
        public string Country { get; set; }

        public HospitalInfoViewModel() { }

        public HospitalInfoViewModel(HospitalInfo model)
        {
            Id = model.Id;
            Name = model.Name;
            Type = model.Type;
            City = model.City;
            PinCode = model.PinCode;
            Country = model.Country;
        }

        public HospitalInfo ConvertViewModel()
        {
            return new HospitalInfo
            {
                Id = Id,
                Name = Name,
                Type = Type,
                City = City,
                PinCode = PinCode,
                Country = Country
            };
        }
    }
}
