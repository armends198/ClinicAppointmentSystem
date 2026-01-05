using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Enums
{
    public enum DayOfWeekEnum
    {
        [Display(Name = "Sunday")]
        Sunday = 0,

        [Display(Name = "Monday")]
        Monday = 1,

        [Display(Name = "Tuesday")]
        Tuesday = 2,

        [Display(Name = "Wednesday")]
        Wednesday = 3,

        [Display(Name = "Thursday")]
        Thursday = 4,

        [Display(Name = "Friday")]
        Friday = 5,

        [Display(Name = "Saturday")]
        Saturday = 6
    }

    public enum AppointmentStatusEnum
    {
        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Approved")]
        Approved = 2,

        [Display(Name = "Completed")]
        Completed = 3,

        [Display(Name = "Cancelled")]
        Cancelled = 4
    }

    public enum NotificationChannelEnum
    {
        [Display(Name = "Email")]
        Email,

        [Display(Name = "SMS")]
        SMS,

        [Display(Name = "In-App")]
        InApp
    }

    public enum NotificationTypeEnum
    {
        [Display(Name = "Reminder")]
        Reminder,

        [Display(Name = "Confirmation")]
        Confirmation,

        [Display(Name = "Cancellation")]
        Cancellation,

        [Display(Name = "Status Change")]
        StatusChange
    }

    public enum GenderEnum
    {
        [Display(Name = "Male")]
        Male,

        [Display(Name = "Female")]
        Female,

        [Display(Name = "Other")]
        Other
    }

    public enum UserRoleEnum
    {
        [Display(Name = "Patient")]
        Patient,

        [Display(Name = "Doctor")]
        Doctor,

        [Display(Name = "Admin")]
        Admin
    }
}