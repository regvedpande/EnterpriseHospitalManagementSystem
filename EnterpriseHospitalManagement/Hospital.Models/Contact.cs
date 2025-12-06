using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Hospital))]
        public int HospitalId { get; set; }
        public HospitalInfo Hospital { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }
    }
}
