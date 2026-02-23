using Microsoft.AspNetCore.Identity;
using Hospital.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Gender Gender { get; set; }

        public string? Nationality { get; set; }

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public DateTime DOB { get; set; }

        public string? Specialist { get; set; }

        [Required]
        public bool IsDoctor { get; set; }

        public string? PictureUri { get; set; }

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public string? Role { get; set; }

        public ICollection<Appointment> AppointmentsAsDoctor { get; set; } = new List<Appointment>();
        public ICollection<Appointment> AppointmentsAsPatient { get; set; } = new List<Appointment>();
        public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        public ICollection<PatientReport> PatientReportsAsDoctor { get; set; } = new List<PatientReport>();
        public ICollection<PatientReport> PatientReportsAsPatient { get; set; } = new List<PatientReport>();
        public ICollection<Timing> Timings { get; set; } = new List<Timing>();
    }
}