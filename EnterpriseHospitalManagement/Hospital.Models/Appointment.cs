using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public string Description { get; set; }

        [Required]
        [ForeignKey(nameof(Doctor))]
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        [Required]
        [ForeignKey(nameof(Patient))]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
    }
}
