using Hospital.Models;
using Hospital.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Number { get; set; } = "";

        [Required]
        public string Type { get; set; } = "";

        [Required]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public string? Description { get; set; }
        public string? Notes { get; set; }

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

        public string? CreatedById { get; set; }

        public AppointmentViewModel() { }

        public AppointmentViewModel(Appointment a)
        {
            if (a == null) return;
            Id = a.Id;
            Number = a.Number;
            Type = a.Type;
            AppointmentDate = a.AppointmentDate;
            CreatedDate = a.CreatedDate;
            Status = a.Status;
            Description = a.Description;
            Notes = a.Notes;
            DoctorId = a.DoctorId;
            DoctorName = a.Doctor?.Name ?? "";
            PatientId = a.PatientId;
            PatientName = a.Patient?.Name ?? "";
            CreatedById = a.CreatedById;
        }

        public Appointment ToModel() => new Appointment
        {
            Id = Id,
            Number = Number,
            Type = Type,
            AppointmentDate = AppointmentDate,
            CreatedDate = CreatedDate,
            Status = Status,
            Description = Description,
            Notes = Notes,
            DoctorId = DoctorId,
            PatientId = PatientId,
            CreatedById = CreatedById
        };
    }
}
