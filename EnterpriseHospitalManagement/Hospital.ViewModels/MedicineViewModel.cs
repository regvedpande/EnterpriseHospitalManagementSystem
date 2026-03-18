using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class MedicineViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Type { get; set; } = "";

        [Required]
        [Display(Name = "Cost")]
        [DataType(DataType.Currency)]
        public decimal Cost { get; set; }

        public string? Description { get; set; }

        public MedicineViewModel() { }

        public MedicineViewModel(Medicine m)
        {
            if (m == null) return;
            Id = m.Id; Name = m.Name; Type = m.Type;
            Cost = m.Cost; Description = m.Description;
        }

        public Medicine ToModel() => new Medicine
        {
            Id = Id, Name = Name, Type = Type,
            Cost = Cost, Description = Description
        };
    }

    public class DepartmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string? Description { get; set; }

        public DepartmentViewModel() { }

        public DepartmentViewModel(Department d)
        {
            if (d == null) return;
            Id = d.Id; Name = d.Name; Description = d.Description;
        }

        public Department ToModel() => new Department
        {
            Id = Id, Name = Name, Description = Description
        };
    }

    public class SupplierViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Company { get; set; } = "";

        [Required]
        [Phone]
        public string Phone { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Address { get; set; } = "";

        public SupplierViewModel() { }

        public SupplierViewModel(Supplier s)
        {
            if (s == null) return;
            Id = s.Id; Company = s.Company; Phone = s.Phone;
            Email = s.Email; Address = s.Address;
        }

        public Supplier ToModel() => new Supplier
        {
            Id = Id, Company = Company, Phone = Phone,
            Email = Email, Address = Address
        };
    }
}
