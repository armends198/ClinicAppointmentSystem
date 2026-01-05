using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models
{
    public class AppointmentStatus
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        [MaxLength(50)]
        public string StatusName { get; set; } // "Pending", "Approved", "Completed", "Cancelled"

        [MaxLength(200)]
        public string Description { get; set; }
    }
}