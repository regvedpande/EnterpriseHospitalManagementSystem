using Hospital.Models;
using Hospital.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class PayrollViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Employee")]
        public string EmployeeId { get; set; } = "";

        [Display(Name = "Employee")]
        public string EmployeeName { get; set; } = "";

        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

        [Required]
        [Display(Name = "Period Start")]
        [DataType(DataType.Date)]
        public DateTime PayPeriodStart { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        [Required]
        [Display(Name = "Period End")]
        [DataType(DataType.Date)]
        public DateTime PayPeriodEnd { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

        [Display(Name = "Created")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Net Salary")]
        [DataType(DataType.Currency)]
        public decimal NetSalary { get; set; }

        [Required]
        [Display(Name = "Hourly Salary")]
        [DataType(DataType.Currency)]
        public decimal HourlySalary { get; set; }

        [Required]
        [Display(Name = "Bonus")]
        [DataType(DataType.Currency)]
        public decimal BonusSalary { get; set; }

        [Required]
        [Display(Name = "Compensation")]
        [DataType(DataType.Currency)]
        public decimal Compensation { get; set; }

        [Required]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = "";

        public PayrollViewModel() { }

        public PayrollViewModel(Payroll p)
        {
            if (p == null) return;
            Id = p.Id; EmployeeId = p.EmployeeId;
            EmployeeName = p.Employee?.Name ?? "";
            Status = p.Status;
            PayPeriodStart = p.PayPeriodStart; PayPeriodEnd = p.PayPeriodEnd;
            CreatedDate = p.CreatedDate;
            NetSalary = p.NetSalary; HourlySalary = p.HourlySalary;
            BonusSalary = p.BonusSalary; Compensation = p.Compensation;
            AccountNumber = p.AccountNumber;
        }

        public Payroll ToModel() => new Payroll
        {
            Id = Id, EmployeeId = EmployeeId, Status = Status,
            PayPeriodStart = PayPeriodStart, PayPeriodEnd = PayPeriodEnd,
            CreatedDate = CreatedDate,
            NetSalary = NetSalary, HourlySalary = HourlySalary,
            BonusSalary = BonusSalary, Compensation = Compensation,
            AccountNumber = AccountNumber
        };
    }
}
