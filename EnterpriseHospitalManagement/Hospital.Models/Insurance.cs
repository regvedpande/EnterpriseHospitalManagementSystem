namespace Hospital.Models
{
    public class Insurance
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<Bill> Bills { get; set; }
    }
}
