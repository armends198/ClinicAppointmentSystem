using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public bool CanManageDoctors { get; set; } = false;

        public bool CanManagePatients { get; set; } = false;

        public bool CanViewReports { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public User User { get; set; }
    }
    //flaflsakflkalkfs
}
}

//hellooo