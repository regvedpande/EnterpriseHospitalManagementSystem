using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class Insurance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public string ProviderName { get; set; } = string.Empty;

        [ForeignKey(nameof(Patient))]
        public string? PatientId { get; set; }
        public ApplicationUser? Patient { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CoverageAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }
}