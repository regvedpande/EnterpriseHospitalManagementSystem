using System;
using Grpc.Core;
using Hospital.Models;

namespace Hospital.Models
{
    public class Timing
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

    } 
}

namespace Hospital.Models
{
    public enum Status
    {
        Available,Pending,Confirm
    }
}
        // Default constructor
