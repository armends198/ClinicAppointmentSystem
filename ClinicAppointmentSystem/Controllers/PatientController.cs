using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentSystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Patient/Index (Dashboard)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get patient info
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get patient's appointments
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.PatientName = patient.User.FirstName + " " + patient.User.LastName;
            ViewBag.Appointments = appointments;

            return View();
        }

        // GET: Patient/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(patient);
        }

        // GET: Patient/BookAppointment
        [HttpGet]
        public async Task<IActionResult> BookAppointment()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new ViewModels.BookAppointmentViewModel
            {
                Specializations = await _context.Specializations.ToListAsync(),
                Doctors = new List<Models.Doctor>(),
                AvailableSlots = new List<ViewModels.AvailableSlot>()
            };

            return View(viewModel);
        }

        // GET: Patient/GetDoctorsBySpecialization
        [HttpGet]
        public async Task<IActionResult> GetDoctorsBySpecialization(int specializationId)
        {
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

        // GET: Patient/GetAvailableSlots
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
            {
                return Json(new List<object>());
            }

            var dayOfWeek = (int)date.DayOfWeek;

            // Get doctor's working hours for this day
            var workingHours = await _context.DoctorWorkingHours
                .Where(w => w.DoctorId == doctorId && w.DayOfWeek == dayOfWeek && w.IsActive)
                .ToListAsync();

            if (!workingHours.Any())
            {
                return Json(new List<object>());
            }

            // Get already booked times for this doctor on this date
            var bookedTimes = await _context.Appointments
                .Include(a => a.TimeSlot)
                .Where(a => a.DoctorId == doctorId
                       && a.TimeSlot.StartDateTime.Date == date.Date
                       && a.StatusId != 4) // Not cancelled
                .Select(a => a.TimeSlot.StartDateTime)
                .ToListAsync();

            var availableSlots = new List<object>();

            foreach (var wh in workingHours)
            {
                var slotStart = date.Date.Add(wh.StartTime);
                var slotEnd = date.Date.Add(wh.EndTime);
                var duration = TimeSpan.FromMinutes(doctor.ConsultationDuration);

                while (slotStart.Add(duration) <= slotEnd)
                {
                    // Check if slot is not booked and is in the future
                    if (!bookedTimes.Contains(slotStart) && slotStart > DateTime.Now)
                    {
                        availableSlots.Add(new
                        {
                            // Use a generated identifier (we'll pass the time as string)
                            timeSlotId = 0, // Not used anymore
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

        // POST: Patient/BookAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(int? DoctorId, DateTime? SelectedDate, string SelectedTime, string Reason)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

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

            // Parse the selected time
            if (!TimeSpan.TryParse(SelectedTime, out TimeSpan selectedTimeSpan))
            {
                TempData["ErrorMessage"] = "Invalid time selected.";
                return RedirectToAction("BookAppointment");
            }

            var slotStartDateTime = SelectedDate.Value.Date.Add(selectedTimeSpan);
            var slotEndDateTime = slotStartDateTime.AddMinutes(doctor.ConsultationDuration);

            // Check if slot is still available (not booked by someone else)
            var isBooked = await _context.Appointments
                .Include(a => a.TimeSlot)
                .AnyAsync(a => a.DoctorId == DoctorId.Value
                          && a.TimeSlot.StartDateTime == slotStartDateTime
                          && a.StatusId != 4);

            if (isBooked)
            {
                TempData["ErrorMessage"] = "Sorry, this time slot is no longer available.";
                return RedirectToAction("BookAppointment");
            }

            // Get working hours reference for the TimeSlot
            var dayOfWeek = (int)SelectedDate.Value.DayOfWeek;
            var workingHours = await _context.DoctorWorkingHours
                .FirstOrDefaultAsync(w => w.DoctorId == DoctorId.Value && w.DayOfWeek == dayOfWeek && w.IsActive);

            if (workingHours == null)
            {
                TempData["ErrorMessage"] = "Doctor is not available on this day.";
                return RedirectToAction("BookAppointment");
            }

            // Create TimeSlot
            var timeSlot = new Models.TimeSlot
            {
                WorkingHoursId = workingHours.WorkingHoursId,
                StartDateTime = slotStartDateTime,
                EndDateTime = slotEndDateTime,
                IsBooked = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();

            // Create Appointment
            var appointment = new Models.Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = DoctorId.Value,
                TimeSlotId = timeSlot.TimeSlotId,
                StatusId = 1, // Pending
                Reason = Reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment booked successfully! Waiting for doctor approval.";
            return RedirectToAction("MyAppointments");
        }

        // GET: Patient/MyAppointments
        [HttpGet]
        public async Task<IActionResult> MyAppointments()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.TimeSlot.StartDateTime)
                .ToListAsync();

            return View(appointments);
        }

        // POST: Patient/CancelAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.PatientId == patient.PatientId);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("MyAppointments");
            }

            // Update status to cancelled
            appointment.StatusId = 4; // Cancelled
            appointment.UpdatedAt = DateTime.UtcNow;

            // Free up the time slot
            appointment.TimeSlot.IsBooked = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction("MyAppointments");
        }

        // GET: Patient/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new ViewModels.EditPatientProfileViewModel
            {
                FirstName = patient.User.FirstName,
                LastName = patient.User.LastName,
                PhoneNumber = patient.User.PhoneNumber,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Address = patient.Address,
                BloodType = patient.BloodType,
                Allergies = patient.Allergies,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone
            };

            return View(viewModel);
        }

        // POST: Patient/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ViewModels.EditPatientProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Update user info
            patient.User.FirstName = model.FirstName;
            patient.User.LastName = model.LastName;
            patient.User.PhoneNumber = model.PhoneNumber;
            patient.User.UpdatedAt = DateTime.UtcNow;

            // Update patient info
            patient.DateOfBirth = model.DateOfBirth;
            patient.Gender = model.Gender;
            patient.Address = model.Address;
            patient.BloodType = model.BloodType;
            patient.Allergies = model.Allergies;
            patient.EmergencyContactName = model.EmergencyContactName;
            patient.EmergencyContactPhone = model.EmergencyContactPhone;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update session with new name
            HttpContext.Session.SetString("UserName", model.FirstName + " " + model.LastName);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // GET: Patient/Calendar
        [HttpGet]
        public async Task<IActionResult> Calendar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .Where(a => a.PatientId == patient.PatientId)
                .ToListAsync();

            var events = appointments.Select(a => new ViewModels.CalendarEvent
            {
                Id = a.AppointmentId,
                Title = "Dr. " + a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                Start = a.TimeSlot.StartDateTime,
                End = a.TimeSlot.EndDateTime,
                Color = a.AppointmentStatus.StatusName switch
                {
                    "Pending" => "#ffc107", // Yellow
                    "Approved" => "#28a745", // Green
                    "Completed" => "#17a2b8", // Blue
                    "Cancelled" => "#dc3545", // Red
                    _ => "#6c757d" // Gray
                },
                Status = a.AppointmentStatus.StatusName,
                DoctorName = "Dr. " + a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                Reason = a.Reason
            }).ToList();

            var viewModel = new ViewModels.CalendarViewModel
            {
                Events = events
            };

            return View(viewModel);
        }
    }
}