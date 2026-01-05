using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Data;
using Microsoft.EntityFrameworkCore;

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
    }
}