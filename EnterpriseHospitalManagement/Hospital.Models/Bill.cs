using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BillNumber { get; set; }

        [Required]
        [ForeignKey(nameof(Patient))]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }

        [ForeignKey(nameof(Insurance))]
        public int? InsuranceId { get; set; }
        public Insurance Insurance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DoctorCharge { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MedicineCharge { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RoomCharge { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OperationCharge { get; set; }

        [Required]
        public int NoOfDays { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NursingCharge { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LabCharge { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Advance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalBill { get; set; }
    }
}
