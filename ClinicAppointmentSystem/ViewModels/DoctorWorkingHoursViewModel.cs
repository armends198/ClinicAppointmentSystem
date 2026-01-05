using ClinicAppointmentSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.ViewModels
{
    public class DoctorWorkingHoursViewModel
    {
        [Required(ErrorMessage = "Day of week is required")]
        [Display(Name = "Day of Week")]
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public class ManageScheduleViewModel
    {
        public List<DoctorWorkingHours> ExistingSchedule { get; set; }
        public DoctorWorkingHoursViewModel NewSchedule { get; set; }
    }
}