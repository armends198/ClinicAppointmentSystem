using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class PatientDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; }

        [Required]
        [MaxLength(50)]
        public string DocumentType { get; set; } // "LabResult", "Imaging", "Prescription"

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Patient Patient { get; set; }
    }
}