using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.ViewModels
{
    public class DoctorRegisterViewModel
    {
        // Account Information
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        // Personal Information
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

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
    }
}