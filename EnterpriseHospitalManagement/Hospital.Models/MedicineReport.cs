using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class MedicineReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        [Required]
        [ForeignKey(nameof(Medicine))]
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }

        [Required]
        public string Company { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime ProductionDate { get; set; }

        [Required]
        public DateTime ExpireDate { get; set; }

        [Required]
        public string Country { get; set; }
    }
}
