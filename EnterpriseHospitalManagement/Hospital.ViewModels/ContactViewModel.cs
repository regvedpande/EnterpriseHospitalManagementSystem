using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class ContactViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Contact Name")]
        public string Name { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [Phone]
        public string Phone { get; set; } = "";

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get => Phone; set => Phone = value; }

        [Required]
        [Display(Name = "Hospital")]
        public int HospitalInfoId { get; set; }

        /// <summary>Alias for <see cref="HospitalInfoId"/>.</summary>
        public int HospitalId { get => HospitalInfoId; set => HospitalInfoId = value; }

        [Display(Name = "Hospital")]
        public string HospitalName { get; set; } = "";

        public ContactViewModel() { }

        public ContactViewModel(Contact c)
        {
            if (c == null) return;
            Id = c.Id;
            Email = c.Email;
            Phone = c.Phone;
            HospitalInfoId = c.HospitalId;
        }

        public Contact ToModel() => new Contact
        {
            Id = this.Id,
            Email = this.Email,
            Phone = this.Phone,
            HospitalId = this.HospitalInfoId
        };
    }
}