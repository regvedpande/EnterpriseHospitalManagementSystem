using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hospital.Models.Enums;

namespace Hospital.Models
{
    public class Timing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Doctor))]
        public string DoctorId { get; set; } = string.Empty;
        public ApplicationUser Doctor { get; set; } = null!;

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
    }
}