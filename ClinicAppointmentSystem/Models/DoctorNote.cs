using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class DoctorNote
    {
        [Key]
        public int NoteId { get; set; }

        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Diagnosis { get; set; }

        [MaxLength(1000)]
        public string? Treatment { get; set; }

        [MaxLength(1000)]
        public string? Prescription { get; set; }

        [MaxLength(2000)]
        public string? AdditionalNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Appointment? Appointment { get; set; }
    }
}