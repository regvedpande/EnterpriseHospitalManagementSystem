using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class PrescribedMedicine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Medicine))]
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }

        [Required]
        [ForeignKey(nameof(PatientReport))]
        public int PatientReportId { get; set; }
        public PatientReport PatientReport { get; set; }
    }
}
