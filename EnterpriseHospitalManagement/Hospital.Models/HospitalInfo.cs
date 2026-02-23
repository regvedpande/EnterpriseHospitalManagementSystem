using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class HospitalInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string PinCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    }
}