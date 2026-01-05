using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } // "Create", "Update", "Delete"

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; }

        public int? EntityId { get; set; }

        [MaxLength(2000)]
        public string OldValues { get; set; }

        [MaxLength(2000)]
        public string NewValues { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }

        [MaxLength(500)]
        public string UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; }
    }
}