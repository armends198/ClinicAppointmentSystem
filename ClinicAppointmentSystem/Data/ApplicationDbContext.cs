using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<DoctorWorkingHours> DoctorWorkingHours { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<AppointmentStatus> AppointmentStatuses { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DoctorNote> DoctorNotes { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<PatientDocument> PatientDocuments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@clinic.com",
                    PasswordHash = "$2a$11$HqHkvDLxPhJPfVaNWS7Yy.SXPYqPx9Z8K8h8PfRxU4Qz9S7rE3YWO", // Password: Admin123!
                    FirstName = "System",
                    LastName = "Admin",
                    PhoneNumber = "000-000-0000",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed data for AppointmentStatuses
            modelBuilder.Entity<AppointmentStatus>().HasData(
                new AppointmentStatus
                {
                    StatusId = 1,
                    StatusName = "Pending",
                    Description = "Appointment is awaiting confirmation"
                },
                new AppointmentStatus
                {
                    StatusId = 2,
                    StatusName = "Approved",
                    Description = "Appointment has been confirmed"
                },
                new AppointmentStatus
                {
                    StatusId = 3,
                    StatusName = "Completed",
                    Description = "Appointment has been completed"
                },
                new AppointmentStatus
                {
                    StatusId = 4,
                    StatusName = "Cancelled",
                    Description = "Appointment has been cancelled"
                }
            );

            // Seed data for Specializations
            modelBuilder.Entity<Specialization>().HasData(
                new Specialization
                {
                    SpecializationId = 1,
                    Name = "Cardiology",
                    Description = "Heart and cardiovascular system"
                },
                new Specialization
                {
                    SpecializationId = 2,
                    Name = "Dermatology",
                    Description = "Skin, hair, and nails"
                },
                new Specialization
                {
                    SpecializationId = 3,
                    Name = "Pediatrics",
                    Description = "Medical care for children"
                },
                new Specialization
                {
                    SpecializationId = 4,
                    Name = "Orthopedics",
                    Description = "Bones, joints, and muscles"
                },
                new Specialization
                {
                    SpecializationId = 5,
                    Name = "General Practice",
                    Description = "Primary care and general medicine"
                }
            );
        }
    }
}