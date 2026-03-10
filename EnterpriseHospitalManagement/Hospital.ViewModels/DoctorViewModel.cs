namespace Hospital.ViewModels
{
    /// <summary>
    /// Alias for doctor/timing data.
    /// Maps to the TimingViewModel fields your project uses.
    /// Add or remove fields to match your actual TimingViewModel.
    /// </summary>
    public class DoctorViewModel
    {
        public int Id { get; set; }
        public string DoctorName { get; set; } = "";
        public string Specialty { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public int? HospitalId { get; set; }
        public string HospitalName { get; set; } = "";
        public string Day { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
    }
}