using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    // Appointment.cs
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [ForeignKey("TimeSlot")]
        public int TimeSlotId { get; set; }

        [ForeignKey("AppointmentStatus")]
        public int StatusId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public AppointmentStatus AppointmentStatus { get; set; }
        public ICollection<DoctorNote> DoctorNotes { get; set; }
    }
}