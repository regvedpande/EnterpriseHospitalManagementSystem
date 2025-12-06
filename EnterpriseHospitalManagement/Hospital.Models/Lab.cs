using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class Lab
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string LabNumber { get; set; }

        [Required]
        [ForeignKey(nameof(Patient))]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }

        [Required]
        public string TestType { get; set; }

        [Required]
        public string TestCode { get; set; }

        public string Weight { get; set; }

        public int Height { get; set; }

        public int BloodPressure { get; set; }

        public int Temperature { get; set; }

        public string TestResult { get; set; }
    }
}
