using Hospital.Models;
using Hospital.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class DocumentViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Patient")]
        public string PatientId { get; set; } = "";

        [Display(Name = "Patient")]
        public string PatientName { get; set; } = "";

        public string UploadedById { get; set; } = "";

        [Display(Name = "Uploaded By")]
        public string UploadedByName { get; set; } = "";

        public string FileName { get; set; } = "";

        [Display(Name = "File")]
        public string OriginalFileName { get; set; } = "";

        public string ContentType { get; set; } = "";
        public long FileSizeBytes { get; set; }

        [Display(Name = "File Size")]
        public string FileSizeDisplay => FileSizeBytes >= 1_048_576
            ? $"{FileSizeBytes / 1_048_576.0:F1} MB"
            : $"{FileSizeBytes / 1_024.0:F0} KB";

        [Required, Display(Name = "Document Type")]
        public DocumentType DocumentType { get; set; } = DocumentType.Other;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Uploaded")]
        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

        public DocumentViewModel() { }

        public DocumentViewModel(PatientDocument d)
        {
            Id               = d.Id;
            PatientId        = d.PatientId;
            PatientName      = d.Patient?.Name ?? "";
            UploadedById     = d.UploadedById;
            UploadedByName   = d.UploadedBy?.Name ?? "";
            FileName         = d.FileName;
            OriginalFileName = d.OriginalFileName;
            ContentType      = d.ContentType;
            FileSizeBytes    = d.FileSizeBytes;
            DocumentType     = d.DocumentType;
            Description      = d.Description;
            UploadedDate     = d.UploadedDate;
        }
    }
}
