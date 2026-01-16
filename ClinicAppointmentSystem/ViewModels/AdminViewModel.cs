using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.ViewModels
{
    public class AdminCreateDoctorViewModel
    {
        // Account Information
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        // Professional Information
        [Required(ErrorMessage = "Specialization is required")]
        [Display(Name = "Specialization")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "License number is required")]
        [Display(Name = "Medical License Number")]
        public string LicenseNumber { get; set; }

        [Display(Name = "Biography")]
        public string? Biography { get; set; }

        [Required(ErrorMessage = "Consultation duration is required")]
        [Display(Name = "Consultation Duration (minutes)")]
        [Range(15, 120, ErrorMessage = "Duration must be between 15 and 120 minutes")]
        public int ConsultationDuration { get; set; } = 30;

        [Required(ErrorMessage = "Consultation fee is required")]
        [Display(Name = "Consultation Fee ($)")]
        [Range(0, 10000, ErrorMessage = "Fee must be between 0 and 10000")]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public class AdminEditDoctorViewModel
    {
        public int DoctorId { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Specialization is required")]
        [Display(Name = "Specialization")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "License number is required")]
        [Display(Name = "Medical License Number")]
        public string LicenseNumber { get; set; }

        [Display(Name = "Biography")]
        public string? Biography { get; set; }

        [Required(ErrorMessage = "Consultation duration is required")]
        [Display(Name = "Consultation Duration (minutes)")]
        [Range(15, 120, ErrorMessage = "Duration must be between 15 and 120 minutes")]
        public int ConsultationDuration { get; set; }

        [Required(ErrorMessage = "Consultation fee is required")]
        [Display(Name = "Consultation Fee ($)")]
        [Range(0, 10000, ErrorMessage = "Fee must be between 0 and 10000")]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    public class AdminEditPatientViewModel
    {
        public int PatientId { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Blood Type")]
        public string? BloodType { get; set; }

        [Display(Name = "Allergies")]
        public string? Allergies { get; set; }

        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Phone")]
        public string? EmergencyContactPhone { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    public class DashboardStatsViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}