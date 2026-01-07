using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Data;
using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Doctor/Index (Dashboard)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get doctor info
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get doctor's appointments
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Get today's appointments
            var today = DateTime.Today;
            var todayAppointments = appointments
                .Where(a => a.TimeSlot.StartDateTime.Date == today)
                .ToList();

            // Get upcoming appointments
            var upcomingAppointments = appointments
                .Where(a => a.TimeSlot.StartDateTime > DateTime.Now)
                .OrderBy(a => a.TimeSlot.StartDateTime)
                .Take(5)
                .ToList();

            ViewBag.DoctorName = "Dr. " + doctor.User.FirstName + " " + doctor.User.LastName;
            ViewBag.Specialization = doctor.Specialization.Name;
            ViewBag.TodayAppointments = todayAppointments;
            ViewBag.UpcomingAppointments = upcomingAppointments;
            ViewBag.TotalAppointments = appointments.Count;

            return View();
        }

        // GET: Doctor/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(doctor);
        }

        // GET: Doctor/ManageSchedule
        [HttpGet]
        public async Task<IActionResult> ManageSchedule()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Load data first, then sort in memory (SQLite doesn't support TimeSpan in OrderBy)
            var existingSchedule = await _context.DoctorWorkingHours
                .Where(w => w.DoctorId == doctor.DoctorId)
                .ToListAsync();

            // Sort in memory
            existingSchedule = existingSchedule
                .OrderBy(w => w.DayOfWeek)
                .ThenBy(w => w.StartTime)
                .ToList();

            var viewModel = new ViewModels.ManageScheduleViewModel
            {
                ExistingSchedule = existingSchedule,
                NewSchedule = new ViewModels.DoctorWorkingHoursViewModel()
            };

            return View(viewModel);
        }

        // POST: Doctor/AddWorkingHours
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkingHours(int DayOfWeek, string StartTimeString, string EndTimeString, bool IsActive = true)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Manual validation for string time fields
            if (string.IsNullOrEmpty(StartTimeString) || string.IsNullOrEmpty(EndTimeString))
            {
                TempData["ErrorMessage"] = "Please select both start and end times.";
                return RedirectToAction("ManageSchedule");
            }

            // Parse times
            if (!TimeSpan.TryParse(StartTimeString, out TimeSpan startTime))
            {
                TempData["ErrorMessage"] = "Invalid start time format.";
                return RedirectToAction("ManageSchedule");
            }

            if (!TimeSpan.TryParse(EndTimeString, out TimeSpan endTime))
            {
                TempData["ErrorMessage"] = "Invalid end time format.";
                return RedirectToAction("ManageSchedule");
            }

            // Validate that end time is after start time
            if (endTime <= startTime)
            {
                TempData["ErrorMessage"] = $"End time must be after start time.";
                return RedirectToAction("ManageSchedule");
            }

            // Check for overlapping schedule
            var allSchedules = await _context.DoctorWorkingHours
                .Where(w => w.DoctorId == doctor.DoctorId
                       && w.DayOfWeek == DayOfWeek
                       && w.IsActive)
                .ToListAsync();

            // Check overlap in memory (SQLite TimeSpan issue)
            var overlapping = allSchedules.Any(w =>
                (startTime >= w.StartTime && startTime < w.EndTime)
                || (endTime > w.StartTime && endTime <= w.EndTime)
                || (startTime <= w.StartTime && endTime >= w.EndTime));

            if (overlapping)
            {
                TempData["ErrorMessage"] = "This time slot overlaps with an existing schedule.";
                return RedirectToAction("ManageSchedule");
            }

            var workingHours = new DoctorWorkingHours
            {
                DoctorId = doctor.DoctorId,
                DayOfWeek = DayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                IsActive = IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.DoctorWorkingHours.Add(workingHours);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Working hours added successfully!";
            return RedirectToAction("ManageSchedule");
        }

        // POST: Doctor/DeleteWorkingHours
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWorkingHours(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var workingHours = await _context.DoctorWorkingHours
                .FirstOrDefaultAsync(w => w.WorkingHoursId == id && w.DoctorId == doctor.DoctorId);

            if (workingHours != null)
            {
                _context.DoctorWorkingHours.Remove(workingHours);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Working hours deleted successfully!";
            }

            return RedirectToAction("ManageSchedule");
        }

        // POST: Doctor/GenerateTimeSlots
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateTimeSlots()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var generator = new Helpers.TimeSlotGenerator(_context);
                await generator.GenerateTimeSlotsForDoctor(doctor.DoctorId, 30);
                TempData["SuccessMessage"] = "Time slots generated successfully for the next 30 days!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating time slots: {ex.Message}";
            }

            return RedirectToAction("ManageSchedule");
        }

        // GET: Doctor/Appointments
        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(appointments);
        }

        // POST: Doctor/ApproveAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAppointment(int appointmentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctor.DoctorId);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            appointment.StatusId = 2; // Approved
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment approved successfully!";
            return RedirectToAction("Appointments");
        }

        // POST: Doctor/RejectAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAppointment(int appointmentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctor.DoctorId);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            appointment.StatusId = 4; // Cancelled
            appointment.UpdatedAt = DateTime.UtcNow;

            // Free up the time slot
            appointment.TimeSlot.IsBooked = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment rejected.";
            return RedirectToAction("Appointments");
        }

        // POST: Doctor/CompleteAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAppointment(int appointmentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctor.DoctorId);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            appointment.StatusId = 3; // Completed
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment marked as completed!";
            return RedirectToAction("Appointments");
        }

        // GET: Doctor/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new ViewModels.EditDoctorProfileViewModel
            {
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                PhoneNumber = doctor.User.PhoneNumber,
                SpecializationId = doctor.SpecializationId,
                LicenseNumber = doctor.LicenseNumber,
                Biography = doctor.Biography,
                ConsultationDuration = doctor.ConsultationDuration,
                ConsultationFee = doctor.ConsultationFee
            };

            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            return View(viewModel);
        }

        // POST: Doctor/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ViewModels.EditDoctorProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Specializations = await _context.Specializations.ToListAsync();
                return View(model);
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if license number is already used by another doctor
            var existingDoctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.LicenseNumber == model.LicenseNumber && d.DoctorId != doctor.DoctorId);

            if (existingDoctor != null)
            {
                ModelState.AddModelError("LicenseNumber", "This license number is already registered to another doctor");
                ViewBag.Specializations = await _context.Specializations.ToListAsync();
                return View(model);
            }

            // Update user info
            doctor.User.FirstName = model.FirstName;
            doctor.User.LastName = model.LastName;
            doctor.User.PhoneNumber = model.PhoneNumber;
            doctor.User.UpdatedAt = DateTime.UtcNow;

            // Update doctor info
            doctor.SpecializationId = model.SpecializationId;
            doctor.LicenseNumber = model.LicenseNumber;
            doctor.Biography = model.Biography;
            doctor.ConsultationDuration = model.ConsultationDuration;
            doctor.ConsultationFee = model.ConsultationFee;
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update session with new name
            HttpContext.Session.SetString("UserName", "Dr. " + model.FirstName + " " + model.LastName);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // GET: Doctor/Calendar
        [HttpGet]
        public async Task<IActionResult> Calendar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .ToListAsync();

            var events = appointments.Select(a => new ViewModels.CalendarEvent
            {
                Id = a.AppointmentId,
                Title = a.Patient.User.FirstName + " " + a.Patient.User.LastName,
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
                PatientName = a.Patient.User.FirstName + " " + a.Patient.User.LastName,
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