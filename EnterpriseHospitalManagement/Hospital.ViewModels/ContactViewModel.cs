using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class ContactViewModel
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public int HospitalInfoId { get; set; }

        public ContactViewModel() { }

        public ContactViewModel(Contact model)
        {
            Id = model.Id;
            Email = model.Email;
            Phone = model.Phone;
            HospitalInfoId = model.HospitalId;
        }

        public Contact ConvertViewModel()
        {
            return new Contact
            {
                Id = Id,
                Email = Email,
                Phone = Phone,
                HospitalId = HospitalInfoId
            };
        }
    }
}
