using Hospital.Models;
using Hospital.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class TimingViewModel
    {
        public int Id { get; set; }

        [Required]
        public string DoctorId { get; set; }

        [Required]
        [Display(Name = "Schedule Date")]
        [DataType(DataType.Date)]
        public DateTime ScheduleDate { get; set; }

        [Required]
        [Display(Name = "Morning Start (24h)")]
        public int MorningShiftStartTime { get; set; }

        [Required]
        [Display(Name = "Morning End (24h)")]
        public int MorningShiftEndTime { get; set; }

        [Required]
        [Display(Name = "Afternoon Start (24h)")]
        public int AfternoonShiftStartTime { get; set; }

        [Required]
        [Display(Name = "Afternoon End (24h)")]
        public int AfternoonShiftEndTime { get; set; }

        [Required]
        [Display(Name = "Slot Duration (minutes)")]
        public int Duration { get; set; }

        [Required]
        [Display(Name = "Status")]
        public Status Status { get; set; }

        public List<SelectListItem> MorningShiftStart { get; set; } = new();
        public List<SelectListItem> MorningShiftEnd { get; set; } = new();
        public List<SelectListItem> AfternoonShiftStart { get; set; } = new();
        public List<SelectListItem> AfternoonShiftEnd { get; set; } = new();

        public TimingViewModel()
        {
        }

        public TimingViewModel(Timing model)
        {
            Id = model.Id;
            DoctorId = model.DoctorId;
            ScheduleDate = model.ScheduleDate;
            MorningShiftStartTime = model.MorningShiftStartTime;
            MorningShiftEndTime = model.MorningShiftEndTime;
            AfternoonShiftStartTime = model.AfternoonShiftStartTime;
            AfternoonShiftEndTime = model.AfternoonShiftEndTime;
            Duration = model.Duration;
            Status = model.Status;
        }

        public Timing ConvertViewModel()
        {
            return new Timing
            {
                Id = Id,
                DoctorId = DoctorId,
                ScheduleDate = ScheduleDate,
                MorningShiftStartTime = MorningShiftStartTime,
                MorningShiftEndTime = MorningShiftEndTime,
                AfternoonShiftStartTime = AfternoonShiftStartTime,
                AfternoonShiftEndTime = AfternoonShiftEndTime,
                Duration = Duration,
                Status = Status
            };
        }
    }
}
