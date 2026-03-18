using Hospital.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class PatientReportViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Diagnosis")]
        public string Diagnose { get; set; } = "";

        public string? Notes { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Doctor")]
        public string DoctorId { get; set; } = "";

        [Display(Name = "Doctor")]
        public string DoctorName { get; set; } = "";

        [Required]
        [Display(Name = "Patient")]
        public string PatientId { get; set; } = "";

        [Display(Name = "Patient")]
        public string PatientName { get; set; } = "";

        public List<PrescribedMedicineViewModel> Prescriptions { get; set; } = new();

        public PatientReportViewModel() { }

        public PatientReportViewModel(PatientReport r)
        {
            if (r == null) return;
            Id = r.Id; Diagnose = r.Diagnose; Notes = r.Notes;
            CreatedDate = r.CreatedDate;
            DoctorId = r.DoctorId; DoctorName = r.Doctor?.Name ?? "";
            PatientId = r.PatientId; PatientName = r.Patient?.Name ?? "";
            if (r.PrescribedMedicines != null)
                foreach (var p in r.PrescribedMedicines)
                    Prescriptions.Add(new PrescribedMedicineViewModel(p));
        }

        public PatientReport ToModel() => new PatientReport
        {
            Id = Id, Diagnose = Diagnose, Notes = Notes,
            CreatedDate = CreatedDate,
            DoctorId = DoctorId, PatientId = PatientId
        };
    }

    public class PrescribedMedicineViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Medicine")]
        public int MedicineId { get; set; }

        [Display(Name = "Medicine")]
        public string MedicineName { get; set; } = "";

        public int PatientReportId { get; set; }

        public string? Dosage { get; set; }
        public int Duration { get; set; }
        public string? Instructions { get; set; }

        public PrescribedMedicineViewModel() { }

        public PrescribedMedicineViewModel(PrescribedMedicine p)
        {
            if (p == null) return;
            Id = p.Id; MedicineId = p.MedicineId;
            MedicineName = p.Medicine?.Name ?? "";
            PatientReportId = p.PatientReportId;
            Dosage = p.Dosage; Duration = p.Duration;
            Instructions = p.Instructions;
        }

        public PrescribedMedicine ToModel() => new PrescribedMedicine
        {
            Id = Id, MedicineId = MedicineId,
            PatientReportId = PatientReportId,
            Dosage = Dosage, Duration = Duration,
            Instructions = Instructions
        };
    }
}
