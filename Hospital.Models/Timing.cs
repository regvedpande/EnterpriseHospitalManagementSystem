using System;
using Hospital.Models;

namespace Hospital.ViewModels
{
    public class TimingViewModel
    {
        public int Id { get; set; }

        // Doctor reference (assuming ApplicationUser is your Identity user)
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        public DateTime Date { get; set; }

        public DateTime MorningShiftStartTime { get; set; }
        public DateTime MorningShiftEndTime { get; set; }

        public DateTime AfternoonShiftStartTime { get; set; }
        public DateTime AfternoonShiftEndTime { get; set; }

        public int Duration { get; set; }

        public Status Status { get; set; }

        // Default constructor
        public TimingViewModel() { }

        // Entity → ViewModel
        public TimingViewModel(Timing model)
        {
            Id = model.Id;
            DoctorId = model.DoctorId?.Id;   // assuming DoctorId is ApplicationUser
            Doctor = model.DoctorId;
            Date = model.Date;
            MorningShiftStartTime = model.MorningShiftStartTime;
            MorningShiftEndTime = model.MorningShiftEndTime;
            AfternoonShiftStartTime = model.AfternoonShiftStartTime;
            AfternoonShiftEndTime = model.AfternoonShiftEndTime;
            Duration = model.Duration;
            Status = model.Status;
        }

        // ViewModel → Entity
        public Timing ConvertView()
        {
            return new Timing
            {
                Id = this.Id,
                DoctorId = this.Doctor,   // EF will track ApplicationUser
                Date = this.Date,
                MorningShiftStartTime = this.MorningShiftStartTime,
                MorningShiftEndTime = this.MorningShiftEndTime,
                AfternoonShiftStartTime = this.AfternoonShiftStartTime,
                AfternoonShiftEndTime = this.AfternoonShiftEndTime,
                Duration = this.Duration,
                Status = this.Status
            };
        }

        // Convenience properties for display
        public string DoctorName => Doctor?.FullName ?? "N/A";
        public string ShiftSummary =>
            $"Morning: {MorningShiftStartTime:hh\\:mm} - {MorningShiftEndTime:hh\\:mm}, " +
            $"Afternoon: {AfternoonShiftStartTime:hh\\:mm} - {AfternoonShiftEndTime:hh\\:mm}";
    }
}