using Hospital.Models;
using Hospital.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class BillViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Bill #")]
        public int BillNumber { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public string PatientId { get; set; } = "";

        [Display(Name = "Patient")]
        public string PatientName { get; set; } = "";

        [Display(Name = "Insurance")]
        public int? InsuranceId { get; set; }

        public BillStatus Status { get; set; } = BillStatus.Pending;

        [Display(Name = "Created")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Paid Date")]
        public DateTime? PaidDate { get; set; }

        public string? CreatedById { get; set; }

        [Display(Name = "Doctor Charge")]
        [DataType(DataType.Currency)]
        public decimal DoctorCharge { get; set; }

        [Display(Name = "Medicine Charge")]
        [DataType(DataType.Currency)]
        public decimal MedicineCharge { get; set; }

        [Display(Name = "Room Charge")]
        [DataType(DataType.Currency)]
        public decimal RoomCharge { get; set; }

        [Display(Name = "Operation Charge")]
        [DataType(DataType.Currency)]
        public decimal OperationCharge { get; set; }

        [Display(Name = "Nursing Charge")]
        [DataType(DataType.Currency)]
        public decimal NursingCharge { get; set; }

        [Display(Name = "Lab Charge")]
        [DataType(DataType.Currency)]
        public decimal LabCharge { get; set; }

        [Display(Name = "Advance")]
        [DataType(DataType.Currency)]
        public decimal Advance { get; set; }

        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal TotalBill { get; set; }

        [Display(Name = "Days")]
        public int NoOfDays { get; set; }

        public BillViewModel() { }

        public BillViewModel(Bill b)
        {
            if (b == null) return;
            Id = b.Id; BillNumber = b.BillNumber;
            PatientId = b.PatientId; PatientName = b.Patient?.Name ?? "";
            InsuranceId = b.InsuranceId; Status = b.Status;
            CreatedDate = b.CreatedDate; PaidDate = b.PaidDate;
            CreatedById = b.CreatedById;
            DoctorCharge = b.DoctorCharge; MedicineCharge = b.MedicineCharge;
            RoomCharge = b.RoomCharge; OperationCharge = b.OperationCharge;
            NursingCharge = b.NursingCharge; LabCharge = b.LabCharge;
            Advance = b.Advance; TotalBill = b.TotalBill; NoOfDays = b.NoOfDays;
        }

        public Bill ToModel() => new Bill
        {
            Id = Id, BillNumber = BillNumber,
            PatientId = PatientId, InsuranceId = InsuranceId,
            Status = Status, CreatedDate = CreatedDate, PaidDate = PaidDate,
            CreatedById = CreatedById,
            DoctorCharge = DoctorCharge, MedicineCharge = MedicineCharge,
            RoomCharge = RoomCharge, OperationCharge = OperationCharge,
            NursingCharge = NursingCharge, LabCharge = LabCharge,
            Advance = Advance, TotalBill = TotalBill, NoOfDays = NoOfDays
        };
    }
}
