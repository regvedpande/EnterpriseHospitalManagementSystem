using Microsoft.AspNetCore.Mvc.Rendering;
using Hospital.Models;
using System;
using System.Collections.Generic;

namespace Hospital.ViewModels
{
    public class TimingViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int MorningShiftStartTime { get; set; }
        public int MorningShiftEndTime { get; set; }
        public int AfternoonShiftStartTime { get; set; }
        public int AfternoonShiftEndTime { get; set; }
        public int Duration { get; set; }
        public Status Status { get; set; }

        // DropDown Lists for shift timings
        public List<SelectListItem> MorningShiftStart { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> MorningShiftEnd { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AfternoonShiftStart { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AfternoonShiftEnd { get; set; } = new List<SelectListItem>();

        // Doctor Reference
        public ApplicationUser DoctorId { get; set; }

        // -------------------------------
        // Constructors
        // -------------------------------
        // Default constructor
        public TimingViewModel()
        {
        }

        // Entity → ViewModel constructor
        public TimingViewModel(Timing model)
        {
            Id = model.Id;
            Date = model.Date;
            MorningShiftStartTime = model.MorningShiftStartTime;
            MorningShiftEndTime = model.MorningShiftEndTime;
            AfternoonShiftStartTime = model.AfternoonShiftStartTime;
            AfternoonShiftEndTime = model.AfternoonShiftEndTime;
            Duration = model.Duration;
            Status = model.Status;
            DoctorId = model.DoctorId;
        }

        // Static method to convert Timing model to TimingViewModel
        public static TimingViewModel ConvertViewModel(Timing model)
        {
            return new TimingViewModel
            {
                Id = model.Id,
                Date = model.Date,
                MorningShiftStartTime = model.MorningShiftStartTime,
                MorningShiftEndTime = model.MorningShiftEndTime,
                AfternoonShiftStartTime = model.AfternoonShiftStartTime,
                AfternoonShiftEndTime = model.AfternoonShiftEndTime,
                Duration = model.Duration,
                Status = model.Status,
                DoctorId = model.DoctorId,
            };
        }
    }
}
