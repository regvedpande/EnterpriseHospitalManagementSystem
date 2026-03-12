using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hospital.Models.Enums;

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

        [ForeignKey(nameof(Doctor))]
        public string? DoctorId { get; set; }
        public ApplicationUser? Doctor { get; set; }

        [ForeignKey(nameof(Technician))]
        public string? TechnicianId { get; set; }
        public ApplicationUser? Technician { get; set; }

        [Required]
        public string TestType { get; set; } = string.Empty;

        [Required]
        public string TestCode { get; set; } = string.Empty;

        public LabTestStatus Status { get; set; } = LabTestStatus.Ordered;

        public string? Weight { get; set; }
        public int BloodPressure { get; set; }
        public int Temperature { get; set; }
        public string? TestResult { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? CompletedDate { get; set; }
    }
}