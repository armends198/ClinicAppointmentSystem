using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentSystem.Controllers
{
    public class AccountController : Controller
    {
        // Database context used to access users, doctors, patients, etc.
        private readonly ApplicationDbContext _context;

        // Constructor with dependency injection
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  REGISTER 

        // Shows the general register page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Handles register form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Check if form validation rules are satisfied
            if (ModelState.IsValid)
            {
                // Check if a user with the same email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    // Add validation error if email is already registered
                    ModelState.AddModelError("Email", "This email is already registered");
                    return View(model);
                }

                // Create a new user entity
                var user = new User
                {
                    Email = model.Email,
                    // Password is hashed for security before saving to database
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Save user to database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create additional data based on user role
                if (model.Role == "Patient")
                {
                    // Default patient profile
                    var patient = new Patient
                    {
                        UserId = user.UserId,
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        Gender = "Other",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Patients.Add(patient);
                }
                else if (model.Role == "Doctor")
                {
                    // Default doctor profile
                    var doctor = new Doctor
                    {
                        UserId = user.UserId,
                        SpecializationId = 5, // General Practice by default
                        LicenseNumber = "TEMP-" + Guid.NewGuid().ToString().Substring(0, 8),
                        ConsultationDuration = 30,
                        ConsultationFee = 50,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Doctors.Add(doctor);
                }

                // Save role-specific data
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            // If validation fails, reload form with errors
            return View(model);
        }

        //LOGIN
        // Shows login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Handles login form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                // Verify password using BCrypt
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    // Save basic user info in session
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FirstName + " " + user.LastName);

                    TempData["SuccessMessage"] = "Login successful!";

                    // Redirect user based on their role
                    if (user.Role == "Admin")
                        return RedirectToAction("Index", "Admin");
                    else if (user.Role == "Doctor")
                        return RedirectToAction("Index", "Doctor");
                    else
                        return RedirectToAction("Index", "Patient");
                }

                // Error shown if email or password is incorrect
                ModelState.AddModelError("", "Invalid email or password");
            }

            return View(model);
        }

        // LOGOUT

        // Clears session and logs out the user
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        //PATIENT REGISTRATION 
        // Shows patient registration page
        [HttpGet]
        public IActionResult RegisterPatient()
        {
            return View();
        }

        // Handles patient registration form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(PatientRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate email
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    return View(model);
                }

                // Create patient user
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

                // Create detailed patient profile
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

        // DOCTOR REGISTRATION 

        // Shows doctor registration page with specialization list
        [HttpGet]
        public async Task<IActionResult> RegisterDoctor()
        {
            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            return View();
        }

        // Handles doctor registration form
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

                // Ensure license number is unique
                var existingDoctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.LicenseNumber == model.LicenseNumber);

                if (existingDoctor != null)
                {
                    ModelState.AddModelError("LicenseNumber", "This license number is already registered");
                    ViewBag.Specializations = await _context.Specializations.ToListAsync();
                    return View(model);
                }

                // Create doctor user
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

                // Create doctor profile
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
