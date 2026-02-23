using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class HospitalInfoViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Hospital Name")]
        public string Name { get; set; } = "";

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; } = "";

        [Required]
        public string City { get; set; } = "";

        [Required]
        [Display(Name = "Pin Code")]
        public string PinCode { get; set; } = "";

        [Required]
        public string Country { get; set; } = "";

        public HospitalInfoViewModel() { }

        public HospitalInfoViewModel(HospitalInfo model)
        {
            if (model == null) return;
            Id = model.Id;
            Name = model.Name;
            Type = model.Type;
            City = model.City;
            PinCode = model.PinCode;
            Country = model.Country;
        }

        public HospitalInfo ToModel() => new HospitalInfo
        {
            Id = this.Id,
            Name = this.Name,
            Type = this.Type,
            City = this.City,
            PinCode = this.PinCode,
            Country = this.Country
        };
    }
}