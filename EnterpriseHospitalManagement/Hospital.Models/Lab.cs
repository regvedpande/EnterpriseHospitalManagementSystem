using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class Lab
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string LabNumber { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(Patient))]
        public string PatientId { get; set; } = string.Empty;
        public ApplicationUser Patient { get; set; } = null!;

        [Required]
        public string TestType { get; set; } = string.Empty;

        [Required]
        public string TestCode { get; set; } = string.Empty;

        public string? Weight { get; set; }
        public int BloodPressure { get; set; }
        public int Temperature { get; set; }
        public string? TestResult { get; set; }
    }
}