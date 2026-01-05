using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class MedicalHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ConditionName { get; set; }

        public DateTime DiagnosedDate { get; set; }

        public bool IsOngoing { get; set; } = true;

        [MaxLength(1000)]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Patient Patient { get; set; }
    }
}