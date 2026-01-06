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
    }
}