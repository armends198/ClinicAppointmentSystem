using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    return View(model);
                }

                // Create new user
                var user = new User
                {
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create role-specific profile
                if (model.Role == "Patient")
                {
                    var patient = new Patient
                    {
                        UserId = user.UserId,
                        DateOfBirth = DateTime.UtcNow.AddYears(-25), // Default value
                        Gender = "Other",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Patients.Add(patient);
                }
                else if (model.Role == "Doctor")
                {
                    var doctor = new Doctor
                    {
                        UserId = user.UserId,
                        SpecializationId = 5, // Default: General Practice
                        LicenseNumber = "TEMP-" + Guid.NewGuid().ToString().Substring(0, 8),
                        ConsultationDuration = 30,
                        ConsultationFee = 50,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Doctors.Add(doctor);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    // Store user info in session
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FirstName + " " + user.LastName);

                    TempData["SuccessMessage"] = "Login successful!";

                    // Redirect based on role
                    if (user.Role == "Admin")
                        return RedirectToAction("Index", "Admin");
                    else if (user.Role == "Doctor")
                        return RedirectToAction("Index", "Doctor");
                    else
                        return RedirectToAction("Index", "Patient");
                }

                ModelState.AddModelError("", "Invalid email or password");
            }

            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // GET: Account/RegisterPatient
        [HttpGet]
        public IActionResult RegisterPatient()
        {
            return View();
        }

        // POST: Account/RegisterPatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(PatientRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    return View(model);
                }

                // Create new user
                var user = new User
                {
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = "Patient",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create patient profile with all details
                var patient = new Patient
                {
                    UserId = user.UserId,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Address = model.Address,
                    BloodType = model.BloodType,
                    Allergies = model.Allergies,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactPhone = model.EmergencyContactPhone,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/RegisterDoctor
        [HttpGet]
        public async Task<IActionResult> RegisterDoctor()
        {
            // Get specializations for dropdown
            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            return View();
        }

        // POST: Account/RegisterDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterDoctor(DoctorRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    ViewBag.Specializations = await _context.Specializations.ToListAsync();
                    return View(model);
                }

                // Check if license number already exists
                var existingDoctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.LicenseNumber == model.LicenseNumber);

                if (existingDoctor != null)
                {
                    ModelState.AddModelError("LicenseNumber", "This license number is already registered");
                    ViewBag.Specializations = await _context.Specializations.ToListAsync();
                    return View(model);
                }

                // Create new user
                var user = new User
                {
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = "Doctor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create doctor profile with all details
                var doctor = new Doctor
                {
                    UserId = user.UserId,
                    SpecializationId = model.SpecializationId,
                    LicenseNumber = model.LicenseNumber,
                    Biography = model.Biography,
                    ConsultationDuration = model.ConsultationDuration,
                    ConsultationFee = model.ConsultationFee,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            return View(model);
        }
    }
}