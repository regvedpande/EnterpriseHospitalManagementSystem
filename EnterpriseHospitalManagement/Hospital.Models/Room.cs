using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoomNumber { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public int Status { get; set; }

        [Required]
        [ForeignKey(nameof(Hospital))]
        public int HospitalId { get; set; }
        public HospitalInfo Hospital { get; set; }
    }
}
