using Hospital.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class PatientDocument
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; } = "";
        public ApplicationUser? Patient { get; set; }

        [Required]
        public string UploadedById { get; set; } = "";
        public ApplicationUser? UploadedBy { get; set; }

        [Required]
        public string FileName { get; set; } = "";          // stored name on disk

        [Required]
        public string OriginalFileName { get; set; } = "";  // original upload name

        public string ContentType { get; set; } = "";
        public long FileSizeBytes { get; set; }

        public DocumentType DocumentType { get; set; } = DocumentType.Other;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    }
}
