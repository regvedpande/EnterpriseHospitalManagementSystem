using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class HospitalInfoViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Hospital Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Pin Code")]
        public string PinCode { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }

        public HospitalInfoViewModel()
        {
        }

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
