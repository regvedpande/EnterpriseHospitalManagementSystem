using System;
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Models
{
    public class TestProd
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } // Product Name

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; } // Product Price

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } // Product Description

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a positive number.")]
        public int Stock { get; set; } // Stock Quantity

        [Display(Name = "Date Added")]
        [DataType(DataType.Date)]
        public DateTime DateAdded { get; set; } = DateTime.Now; // Date Product Added
    }
}