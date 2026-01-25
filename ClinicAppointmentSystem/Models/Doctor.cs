using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Specialization")]
        public int SpecializationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LicenseNumber { get; set; }

        [MaxLength(1000)]
        public string Biography { get; set; }

        public int ConsultationDuration { get; set; } = 30;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ConsultationFee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Specialization Specialization { get; set; }
        public ICollection<DoctorWorkingHours> WorkingHours { get; set; }
        public ICollection<Appointment> Appointments { get; set; }

    }
}