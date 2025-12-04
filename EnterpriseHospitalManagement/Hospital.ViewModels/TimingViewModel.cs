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
        public DateTime ScheduleDate { get; set; }

        [Required]
        public int MorningShiftStartTime { get; set; }

        [Required]
        public int MorningShiftEndTime { get; set; }

        [Required]
        public int AfternoonShiftStartTime { get; set; }

        [Required]
        public int AfternoonShiftEndTime { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public Status Status { get; set; }

        public List<SelectListItem> MorningShiftStart { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> MorningShiftEnd { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AfternoonShiftStart { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AfternoonShiftEnd { get; set; } = new List<SelectListItem>();

        public TimingViewModel() { }

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
