using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class TimeSlot
    {
        [Key]
        public int TimeSlotId { get; set; }

        [ForeignKey("DoctorWorkingHours")]
        public int WorkingHoursId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        public bool IsBooked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public DoctorWorkingHours DoctorWorkingHours { get; set; }
    }
}