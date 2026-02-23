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
        public HospitalInfo Hospital { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;
    }
}