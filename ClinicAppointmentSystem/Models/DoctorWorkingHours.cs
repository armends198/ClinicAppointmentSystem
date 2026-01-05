using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class DoctorWorkingHours
    {
        [Key]
        public int WorkingHoursId { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [Required]
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, ... 6=Saturday

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Doctor Doctor { get; set; }
    }
}