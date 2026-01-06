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
        public string StartTimeString { get; set; } // Changed to string

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public string EndTimeString { get; set; } // Changed to string

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Helper properties to convert to TimeSpan
        public TimeSpan StartTime
        {
            get
            {
                if (TimeSpan.TryParse(StartTimeString, out var time))
                    return time;
                return TimeSpan.Zero;
            }
        }

        public TimeSpan EndTime
        {
            get
            {
                if (TimeSpan.TryParse(EndTimeString, out var time))
                    return time;
                return TimeSpan.Zero;
            }
        }
    }

    public class ManageScheduleViewModel
    {
        public List<DoctorWorkingHours> ExistingSchedule { get; set; }
        public DoctorWorkingHoursViewModel NewSchedule { get; set; }
    }
}