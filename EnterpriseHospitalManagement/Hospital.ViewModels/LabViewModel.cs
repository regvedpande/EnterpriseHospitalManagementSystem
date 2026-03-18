using Hospital.Models;
using Hospital.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class LabViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Lab #")]
        public string LabNumber { get; set; } = "";

        [Required]
        [Display(Name = "Patient")]
        public string PatientId { get; set; } = "";

        [Display(Name = "Patient")]
        public string PatientName { get; set; } = "";

        [Display(Name = "Doctor")]
        public string? DoctorId { get; set; }

        [Display(Name = "Doctor")]
        public string DoctorName { get; set; } = "";

        [Display(Name = "Technician")]
        public string? TechnicianId { get; set; }

        [Display(Name = "Technician")]
        public string TechnicianName { get; set; } = "";

        [Required]
        [Display(Name = "Test Type")]
        public string TestType { get; set; } = "";

        [Required]
        [Display(Name = "Test Code")]
        public string TestCode { get; set; } = "";

        public LabTestStatus Status { get; set; } = LabTestStatus.Ordered;

        public string? Weight { get; set; }

        [Display(Name = "Blood Pressure")]
        public int BloodPressure { get; set; }

        public int Temperature { get; set; }

        [Display(Name = "Test Result")]
        public string? TestResult { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Completed")]
        public DateTime? CompletedDate { get; set; }

        public LabViewModel() { }

        public LabViewModel(Lab l)
        {
            if (l == null) return;
            Id = l.Id; LabNumber = l.LabNumber;
            PatientId = l.PatientId; PatientName = l.Patient?.Name ?? "";
            DoctorId = l.DoctorId; DoctorName = l.Doctor?.Name ?? "";
            TechnicianId = l.TechnicianId; TechnicianName = l.Technician?.Name ?? "";
            TestType = l.TestType; TestCode = l.TestCode;
            Status = l.Status; Weight = l.Weight;
            BloodPressure = l.BloodPressure; Temperature = l.Temperature;
            TestResult = l.TestResult;
            CreatedDate = l.CreatedDate; CompletedDate = l.CompletedDate;
        }

        public Lab ToModel() => new Lab
        {
            Id = Id, LabNumber = LabNumber,
            PatientId = PatientId, DoctorId = DoctorId, TechnicianId = TechnicianId,
            TestType = TestType, TestCode = TestCode,
            Status = Status, Weight = Weight,
            BloodPressure = BloodPressure, Temperature = Temperature,
            TestResult = TestResult,
            CreatedDate = CreatedDate, CompletedDate = CompletedDate
        };
    }
}
