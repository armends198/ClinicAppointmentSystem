using System.ComponentModel.DataAnnotations;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.ViewModels
{
    public class BookAppointmentViewModel
    {
        public List<Specialization> Specializations { get; set; }
        public List<Doctor> Doctors { get; set; }
        public List<AvailableSlot> AvailableSlots { get; set; }

        [Required(ErrorMessage = "Please select a specialization")]
        [Display(Name = "Specialization")]
        public int? SpecializationId { get; set; }

        [Required(ErrorMessage = "Please select a doctor")]
        [Display(Name = "Doctor")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "Please select a date")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime? SelectedDate { get; set; }

        [Required(ErrorMessage = "Please select a time slot")]
        [Display(Name = "Time Slot")]
        public int? TimeSlotId { get; set; }

        [Display(Name = "Reason for Visit")]
        [MaxLength(500)]
        public string Reason { get; set; }
    }

    public class AvailableSlot
    {
        public int TimeSlotId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsBooked { get; set; }
    }
}