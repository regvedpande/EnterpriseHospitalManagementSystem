using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class ContactViewModel
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string Phone { get; set; }

        [Required]
        public int HospitalInfoId { get; set; }

        public ContactViewModel() { }

        public ContactViewModel(Contact c)
        {
            if (c == null) return;
            Id = c.Id;
            Email = c.Email;
            Phone = c.Phone;
            HospitalInfoId = c.HospitalId;
        }

        public Contact ConvertViewModel() => new Contact
        {
            Id = this.Id,
            Email = this.Email,
            Phone = this.Phone,
            HospitalId = this.HospitalInfoId
        };
    }
}
