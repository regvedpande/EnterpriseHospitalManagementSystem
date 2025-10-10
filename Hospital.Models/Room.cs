namespace Hospital.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public int Status { get; set; }
        public int HospitalId { get; set; }
        public HospitalInfoViewModel Hospital { get; set; }

    }
}