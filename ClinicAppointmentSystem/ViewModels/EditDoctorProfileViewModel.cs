using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.ViewModels
{
    public class EditDoctorProfileViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

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
    }
}