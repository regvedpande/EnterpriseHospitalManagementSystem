using Hospital.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class InsuranceViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Policy Number")]
        public string PolicyNumber { get; set; } = "";

        [Required]
        [Display(Name = "Provider")]
        public string ProviderName { get; set; } = "";

        [Display(Name = "Patient")]
        public string? PatientId { get; set; }

        [Display(Name = "Patient")]
        public string PatientName { get; set; } = "";

        [Display(Name = "Coverage Amount")]
        [DataType(DataType.Currency)]
        public decimal CoverageAmount { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

        public InsuranceViewModel() { }

        public InsuranceViewModel(Insurance i)
        {
            if (i == null) return;
            Id = i.Id; PolicyNumber = i.PolicyNumber;
            ProviderName = i.ProviderName; PatientId = i.PatientId;
            PatientName = i.Patient?.Name ?? "";
            CoverageAmount = i.CoverageAmount;
            StartDate = i.StartDate; EndDate = i.EndDate;
        }

        public Insurance ToModel() => new Insurance
        {
            Id = Id, PolicyNumber = PolicyNumber,
            ProviderName = ProviderName, PatientId = PatientId,
            CoverageAmount = CoverageAmount,
            StartDate = StartDate, EndDate = EndDate
        };
    }
}
