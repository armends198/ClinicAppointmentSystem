using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to check admin authorization
        private bool IsAdminAuthorized()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            return userId != null && userRole == "Admin";
        }

        #region Dashboard

        // GET: Admin/Index (Dashboard)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalDoctors = await _context.Doctors.CountAsync();
            ViewBag.TotalAppointments = await _context.Appointments.CountAsync();

            ViewBag.TodayAppointments = await _context.Appointments
                .Include(a => a.TimeSlot)
                .Where(a => a.TimeSlot.StartDateTime.Date == DateTime.Today)
                .CountAsync();

            ViewBag.PendingAppointments = await _context.Appointments.CountAsync(a => a.StatusId == 1);
            ViewBag.CompletedAppointments = await _context.Appointments.CountAsync(a => a.StatusId == 3);

            ViewBag.RecentAppointments = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.RecentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }

        #endregion

        #region User Management

        [HttpGet]
        public async Task<IActionResult> Users(string role = "all")
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var query = _context.Users.AsQueryable();

            switch (role.ToLower())
            {
                case "patient": query = query.Where(u => u.Role == "Patient"); break;
                case "doctor": query = query.Where(u => u.Role == "Doctor"); break;
                case "admin": query = query.Where(u => u.Role == "Admin"); break;
            }

            ViewBag.CurrentFilter = role;
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.PatientCount = await _context.Users.CountAsync(u => u.Role == "Patient");
            ViewBag.DoctorCount = await _context.Users.CountAsync(u => u.Role == "Doctor");
            ViewBag.AdminCount = await _context.Users.CountAsync(u => u.Role == "Admin");

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            // Get role-specific data
            if (user.Role == "Patient")
            {
                ViewBag.Patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == id);
                ViewBag.AppointmentCount = await _context.Appointments
                    .Include(a => a.Patient)
                    .CountAsync(a => a.Patient.UserId == id);
            }
            else if (user.Role == "Doctor")
            {
                ViewBag.Doctor = await _context.Doctors
                    .Include(d => d.Specialization)
                    .FirstOrDefaultAsync(d => d.UserId == id);
                ViewBag.AppointmentCount = await _context.Appointments
                    .Include(a => a.Doctor)
                    .CountAsync(a => a.Doctor.UserId == id);
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            var viewModel = new AdminEditUserViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(AdminEditUserViewModel model)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Users");
                }

                // Prevent changing own admin role
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (user.UserId == currentUserId && model.Role != "Admin")
                {
                    ModelState.AddModelError("Role", "You cannot change your own admin role.");
                    return View(model);
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Role = model.Role;
                user.IsActive = model.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("Users");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            // Prevent deactivating own account
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (user.UserId == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account.";
                return RedirectToAction("Users");
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully!";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            // Prevent deleting own account
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (user.UserId == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            // Check for related data
            if (user.Role == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == id);
                if (patient != null)
                {
                    var hasAppointments = await _context.Appointments.AnyAsync(a => a.PatientId == patient.PatientId);
                    if (hasAppointments)
                    {
                        TempData["ErrorMessage"] = "Cannot delete user with existing appointments. Deactivate instead.";
                        return RedirectToAction("Users");
                    }
                    _context.Patients.Remove(patient);
                }
            }
            else if (user.Role == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == id);
                if (doctor != null)
                {
                    var hasAppointments = await _context.Appointments.AnyAsync(a => a.DoctorId == doctor.DoctorId);
                    if (hasAppointments)
                    {
                        TempData["ErrorMessage"] = "Cannot delete user with existing appointments. Deactivate instead.";
                        return RedirectToAction("Users");
                    }
                    _context.Doctors.Remove(doctor);
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User deleted successfully!";
            return RedirectToAction("Users");
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            return View(new AdminCreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                // Check if email exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                var user = new User
                {
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create role-specific profile if needed
                if (model.Role == "Patient")
                {
                    var patient = new Patient
                    {
                        UserId = user.UserId,
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        Gender = "Other",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction("Users");
            }

            return View(model);
        }

        #endregion

        #region Doctor Management

        [HttpGet]
        public async Task<IActionResult> Doctors()
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> DoctorDetails(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Include(d => d.WorkingHours)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("Doctors");
            }

            ViewBag.TotalAppointments = await _context.Appointments.CountAsync(a => a.DoctorId == id);
            ViewBag.CompletedAppointments = await _context.Appointments.CountAsync(a => a.DoctorId == id && a.StatusId == 3);
            ViewBag.PendingAppointments = await _context.Appointments.CountAsync(a => a.DoctorId == id && a.StatusId == 1);

            return View(doctor);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDoctor()
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            return View(new AdminCreateDoctorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(AdminCreateDoctorViewModel model)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    ViewBag.Specializations = await _context.Specializations.ToListAsync();
                    return View(model);
                }

                if (await _context.Doctors.AnyAsync(d => d.LicenseNumber == model.LicenseNumber))
                {
                    ModelState.AddModelError("LicenseNumber", "License number already registered.");
                    ViewBag.Specializations = await _context.Specializations.ToListAsync();
                    return View(model);
                }

                var user = new User
                {
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = "Doctor",
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

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

                TempData["SuccessMessage"] = "Doctor created successfully!";
                return RedirectToAction("Doctors");
            }

            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditDoctor(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("Doctors");
            }

            var viewModel = new AdminEditDoctorViewModel
            {
                DoctorId = doctor.DoctorId,
                Email = doctor.User.Email,
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                PhoneNumber = doctor.User.PhoneNumber,
                SpecializationId = doctor.SpecializationId,
                LicenseNumber = doctor.LicenseNumber,
                Biography = doctor.Biography,
                ConsultationDuration = doctor.ConsultationDuration,
                ConsultationFee = doctor.ConsultationFee,
                IsActive = doctor.User.IsActive
            };

            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(AdminEditDoctorViewModel model)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == model.DoctorId);

                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Doctor not found.";
                    return RedirectToAction("Doctors");
                }

                if (await _context.Doctors.AnyAsync(d => d.LicenseNumber == model.LicenseNumber && d.DoctorId != model.DoctorId))
                {
                    ModelState.AddModelError("LicenseNumber", "License number already registered.");
                    ViewBag.Specializations = await _context.Specializations.ToListAsync();
                    return View(model);
                }

                doctor.User.FirstName = model.FirstName;
                doctor.User.LastName = model.LastName;
                doctor.User.PhoneNumber = model.PhoneNumber;
                doctor.User.IsActive = model.IsActive;
                doctor.User.UpdatedAt = DateTime.UtcNow;

                doctor.SpecializationId = model.SpecializationId;
                doctor.LicenseNumber = model.LicenseNumber;
                doctor.Biography = model.Biography;
                doctor.ConsultationDuration = model.ConsultationDuration;
                doctor.ConsultationFee = model.ConsultationFee;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Doctor updated successfully!";
                return RedirectToAction("Doctors");
            }

            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDoctorStatus(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.DoctorId == id);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("Doctors");
            }

            doctor.User.IsActive = !doctor.User.IsActive;
            doctor.User.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Doctor {(doctor.User.IsActive ? "activated" : "deactivated")}!";
            return RedirectToAction("Doctors");
        }

        #endregion

        #region Patient Management

        [HttpGet]
        public async Task<IActionResult> Patients()
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var patients = await _context.Patients
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(patients);
        }

        [HttpGet]
        public async Task<IActionResult> PatientDetails(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Patients");
            }

            ViewBag.Appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.TimeSlot.StartDateTime)
                .ToListAsync();

            ViewBag.MedicalHistory = await _context.MedicalHistories
                .Where(m => m.PatientId == id)
                .OrderByDescending(m => m.DiagnosedDate)
                .ToListAsync();

            return View(patient);
        }

        [HttpGet]
        public async Task<IActionResult> EditPatient(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Patients");
            }

            var viewModel = new AdminEditPatientViewModel
            {
                PatientId = patient.PatientId,
                Email = patient.User.Email,
                FirstName = patient.User.FirstName,
                LastName = patient.User.LastName,
                PhoneNumber = patient.User.PhoneNumber,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Address = patient.Address,
                BloodType = patient.BloodType,
                Allergies = patient.Allergies,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                IsActive = patient.User.IsActive
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(AdminEditPatientViewModel model)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.PatientId == model.PatientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction("Patients");
                }

                patient.User.FirstName = model.FirstName;
                patient.User.LastName = model.LastName;
                patient.User.PhoneNumber = model.PhoneNumber;
                patient.User.IsActive = model.IsActive;
                patient.User.UpdatedAt = DateTime.UtcNow;

                patient.DateOfBirth = model.DateOfBirth;
                patient.Gender = model.Gender;
                patient.Address = model.Address;
                patient.BloodType = model.BloodType;
                patient.Allergies = model.Allergies;
                patient.EmergencyContactName = model.EmergencyContactName;
                patient.EmergencyContactPhone = model.EmergencyContactPhone;
                patient.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Patient updated successfully!";
                return RedirectToAction("Patients");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePatientStatus(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Patients");
            }

            patient.User.IsActive = !patient.User.IsActive;
            patient.User.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Patient {(patient.User.IsActive ? "activated" : "deactivated")}!";
            return RedirectToAction("Patients");
        }

        #endregion

        #region Appointment Management

        [HttpGet]
        public async Task<IActionResult> Appointments(string status = "all")
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var query = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .AsQueryable();

            switch (status.ToLower())
            {
                case "pending": query = query.Where(a => a.StatusId == 1); break;
                case "approved": query = query.Where(a => a.StatusId == 2); break;
                case "completed": query = query.Where(a => a.StatusId == 3); break;
                case "cancelled": query = query.Where(a => a.StatusId == 4); break;
            }

            ViewBag.CurrentFilter = status;
            ViewBag.PendingCount = await _context.Appointments.CountAsync(a => a.StatusId == 1);
            ViewBag.ApprovedCount = await _context.Appointments.CountAsync(a => a.StatusId == 2);
            ViewBag.CompletedCount = await _context.Appointments.CountAsync(a => a.StatusId == 3);
            ViewBag.CancelledCount = await _context.Appointments.CountAsync(a => a.StatusId == 4);

            return View(await query.OrderByDescending(a => a.TimeSlot.StartDateTime).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.TimeSlot)
                .Include(a => a.AppointmentStatus)
                .Include(a => a.DoctorNotes)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, int statusId)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var appointment = await _context.Appointments.Include(a => a.TimeSlot).FirstOrDefaultAsync(a => a.AppointmentId == id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            appointment.StatusId = statusId;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (statusId == 4) // Cancelled
                appointment.TimeSlot.IsBooked = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment status updated!";
            return RedirectToAction("Appointments");
        }

        #endregion

        #region Specialization Management

        [HttpGet]
        public async Task<IActionResult> Specializations()
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            ViewBag.DoctorCounts = await _context.Doctors
                .GroupBy(d => d.SpecializationId)
                .Select(g => new { SpecializationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SpecializationId, x => x.Count);

            return View(await _context.Specializations.ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateSpecialization()
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            return View(new Specialization());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpecialization(Specialization model)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            ModelState.Remove("SpecializationId");

            if (ModelState.IsValid)
            {
                if (await _context.Specializations.AnyAsync(s => s.Name == model.Name))
                {
                    ModelState.AddModelError("Name", "Specialization already exists.");
                    return View(model);
                }

                _context.Specializations.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Specialization created!";
                return RedirectToAction("Specializations");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditSpecialization(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var spec = await _context.Specializations.FindAsync(id);
            if (spec == null)
            {
                TempData["ErrorMessage"] = "Specialization not found.";
                return RedirectToAction("Specializations");
            }

            return View(spec);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSpecialization(Specialization model)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var spec = await _context.Specializations.FindAsync(model.SpecializationId);
                if (spec == null)
                {
                    TempData["ErrorMessage"] = "Specialization not found.";
                    return RedirectToAction("Specializations");
                }

                spec.Name = model.Name;
                spec.Description = model.Description;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Specialization updated!";
                return RedirectToAction("Specializations");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecialization(int id)
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            var spec = await _context.Specializations.FindAsync(id);
            if (spec == null)
            {
                TempData["ErrorMessage"] = "Specialization not found.";
                return RedirectToAction("Specializations");
            }

            if (await _context.Doctors.AnyAsync(d => d.SpecializationId == id))
            {
                TempData["ErrorMessage"] = "Cannot delete - doctors are using this specialization.";
                return RedirectToAction("Specializations");
            }

            _context.Specializations.Remove(spec);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Specialization deleted!";
            return RedirectToAction("Specializations");
        }

        #endregion

        #region Reports

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            if (!IsAdminAuthorized())
                return RedirectToAction("Login", "Account");

            // SQLite cannot SUM decimal in the database.  Pull the numeric values to memory then aggregate.
            var completedFees = await _context.Appointments
                .Where(a => a.StatusId == 3) // Completed
                .Select(a => a.Doctor.ConsultationFee)
                .ToListAsync(); // materialize on client

            ViewBag.TotalRevenue = (completedFees?.Any() ?? false) ? completedFees.Sum() : 0m;

            ViewBag.AppointmentsBySpecialization = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .GroupBy(a => a.Doctor.Specialization.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.TopDoctors = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .GroupBy(a => new { a.DoctorId, a.Doctor.User.FirstName, a.Doctor.User.LastName })
                .Select(g => new { Name = "Dr. " + g.Key.FirstName + " " + g.Key.LastName, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            return View();
        }

        #endregion
    }
}