using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class Payroll
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Employee))]
        public string EmployeeId { get; set; } = string.Empty;
        public ApplicationUser Employee { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HourlySalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BonusSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Compensation { get; set; }

        [Required]
        public string AccountNumber { get; set; } = string.Empty;
    }
}