using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Data;
using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Controllers
{
    public class PatientController : Controller
    {
        // Database context used to access patients, appointments, documents, etc.
        private readonly ApplicationDbContext _context;

        // Inject ApplicationDbContext through constructor
        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        //DASHBOARD 

        // Displays patient dashboard with recent appointments
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in by reading session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Get patient entity linked to logged-in user
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            // Get last 5 appointments for dashboard overview
            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Pass data to view
            ViewBag.PatientName = patient.User.FirstName + " " + patient.User.LastName;
            ViewBag.Appointments = appointments;

            return View();
        }

        //PROFILE 

        // Shows patient profile information
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            return View(patient);
        }

        //BOOK APPOINTMENT 

        // Displays appointment booking page
        [HttpGet]
        public async Task<IActionResult> BookAppointment()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Initialize empty view model for dropdowns
            var viewModel = new ViewModels.BookAppointmentViewModel
            {
                Specializations = await _context.Specializations.ToListAsync(),
                Doctors = new List<Doctor>(),
                AvailableSlots = new List<ViewModels.AvailableSlot>()
            };

            return View(viewModel);
        }

        // Returns doctors based on selected specialization (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetDoctorsBySpecialization(int specializationId)
        {
            // Select only needed data to send as JSON
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Where(d => d.SpecializationId == specializationId)
                .Select(d => new
                {
                    doctorId = d.DoctorId,
                    name = "Dr. " + d.User.FirstName + " " + d.User.LastName,
                    specialization = d.Specialization.Name,
                    fee = d.ConsultationFee
                })
                .ToListAsync();

            return Json(doctors);
        }

        // Returns available time slots for selected doctor and date
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
                return Json(new List<object>());

            var dayOfWeek = (int)date.DayOfWeek;

            // Get doctor's active working hours for that day
            var workingHours = await _context.DoctorWorkingHours
                .Where(w => w.DoctorId == doctorId && w.DayOfWeek == dayOfWeek && w.IsActive)
                .ToListAsync();

            if (!workingHours.Any())
                return Json(new List<object>());

            // Get already booked slots for the selected date
            var bookedTimes = await _context.Appointments
                .Include(a => a.TimeSlot)
                .Where(a => a.DoctorId == doctorId &&
                            a.TimeSlot.StartDateTime.Date == date.Date &&
                            a.StatusId != 4) // Exclude cancelled
                .Select(a => a.TimeSlot.StartDateTime)
                .ToListAsync();

            var availableSlots = new List<object>();

            foreach (var wh in workingHours)
            {
                var slotStart = date.Date.Add(wh.StartTime);
                var slotEnd = date.Date.Add(wh.EndTime);
                var duration = TimeSpan.FromMinutes(doctor.ConsultationDuration);

                // Generate slots within working hours
                while (slotStart.Add(duration) <= slotEnd)
                {
                    // Allow only future and unbooked slots
                    if (!bookedTimes.Contains(slotStart) && slotStart > DateTime.Now)
                    {
                        availableSlots.Add(new
                        {
                            startTime = slotStart.ToString("HH:mm"),
                            endTime = slotStart.Add(duration).ToString("HH:mm"),
                            startDateTime = slotStart
                        });
                    }

                    slotStart = slotStart.Add(duration);
                }
            }

            return Json(availableSlots);
        }

        //CONFIRM BOOKING 

        // Handles appointment booking submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(int? DoctorId, DateTime? SelectedDate, string SelectedTime, string Reason)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
                return RedirectToAction("Login", "Account");

            // Validate required input
            if (!DoctorId.HasValue || !SelectedDate.HasValue || string.IsNullOrEmpty(SelectedTime))
            {
                TempData["ErrorMessage"] = "Please select a doctor, date, and time slot.";
                return RedirectToAction("BookAppointment");
            }

            var doctor = await _context.Doctors.FindAsync(DoctorId.Value);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("BookAppointment");
            }

            // Convert selected time string to TimeSpan
            if (!TimeSpan.TryParse(SelectedTime, out TimeSpan selectedTimeSpan))
            {
                TempData["ErrorMessage"] = "Invalid time selected.";
                return RedirectToAction("BookAppointment");
            }

            var slotStartDateTime = SelectedDate.Value.Date.Add(selectedTimeSpan);
            var slotEndDateTime = slotStartDateTime.AddMinutes(doctor.ConsultationDuration);

            // Check if slot was booked by someone else
            var isBooked = await _context.Appointments
                .Include(a => a.TimeSlot)
                .AnyAsync(a => a.DoctorId == DoctorId.Value &&
                               a.TimeSlot.StartDateTime == slotStartDateTime &&
                               a.StatusId != 4);

            if (isBooked)
            {
                TempData["ErrorMessage"] = "This time slot is no longer available.";
                return RedirectToAction("BookAppointment");
            }

            // Get working hours reference
            var dayOfWeek = (int)SelectedDate.Value.DayOfWeek;
            var workingHours = await _context.DoctorWorkingHours
                .FirstOrDefaultAsync(w => w.DoctorId == DoctorId.Value && w.DayOfWeek == dayOfWeek && w.IsActive);

            if (workingHours == null)
            {
                TempData["ErrorMessage"] = "Doctor is not available on this day.";
                return RedirectToAction("BookAppointment");
            }

            // Create and save time slot
            var timeSlot = new TimeSlot
            {
                WorkingHoursId = workingHours.WorkingHoursId,
                StartDateTime = slotStartDateTime,
                EndDateTime = slotEndDateTime,
                IsBooked = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();

            // Create appointment record
            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = DoctorId.Value,
                TimeSlotId = timeSlot.TimeSlotId,
                StatusId = 1, // Pending approval
                Reason = Reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment booked successfully!";
            return RedirectToAction("MyAppointments");
        }

    }
}
