using Grpc.Core;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hospital.ViewModels
{
    public class TimingViewModel
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public int MorningShiftStartTime { get; set; }
        public int MorningShiftEndTime { get; set; }
        public int AfternoonShiftStartTime { get; set; }
        public int AfternoonShiftEndTime { get; set; }
        public int Duration { get; set; }
        public Status Status { get; set; }

        public List<SelectListItem> morningShiftStart { get; } = new List<SelectListItem>();
        public List<SelectListItem> morningShiftEnd { get; } = new List<SelectListItem>();
        public List<SelectListItem> AfternoonShiftStart { get; } = new List<SelectListItem>();
        public List<SelectListItem> AfternoonShiftEnd { get; } = new List<SelectListItem>();

        public ApplicationUser Doctor { get; set; }
    }
}
