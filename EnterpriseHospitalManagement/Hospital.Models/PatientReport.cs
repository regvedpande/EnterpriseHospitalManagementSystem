using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class PatientReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Diagnose { get; set; }

        [Required]
        [ForeignKey(nameof(Doctor))]
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        [Required]
        [ForeignKey(nameof(Patient))]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }

        public ICollection<PrescribedMedicine> PrescribedMedicines { get; set; }
    }
}
