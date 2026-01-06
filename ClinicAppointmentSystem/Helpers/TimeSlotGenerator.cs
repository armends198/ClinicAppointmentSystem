using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentSystem.Helpers
{
    public class TimeSlotGenerator
    {
        private readonly ApplicationDbContext _context;

        public TimeSlotGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generate time slots for a specific doctor for the next N days
        /// </summary>
        public async Task GenerateTimeSlotsForDoctor(int doctorId, int daysAhead = 30)
        {
            // Get doctor and their working hours
            var doctor = await _context.Doctors
                .Include(d => d.WorkingHours)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null || !doctor.WorkingHours.Any())
            {
                return;
            }

            var today = DateTime.Today;
            var endDate = today.AddDays(daysAhead);

            // Loop through each day
            for (var date = today; date < endDate; date = date.AddDays(1))
            {
                var dayOfWeek = (int)date.DayOfWeek;

                // Get working hours for this day
                var workingHoursForDay = doctor.WorkingHours
                    .Where(w => w.DayOfWeek == dayOfWeek && w.IsActive)
                    .ToList();

                foreach (var workingHour in workingHoursForDay)
                {
                    // Check if time slots already exist for this working hour and date
                    var existingSlots = await _context.TimeSlots
                        .Where(t => t.WorkingHoursId == workingHour.WorkingHoursId
                                 && t.StartDateTime.Date == date)
                        .AnyAsync();

                    if (existingSlots)
                    {
                        continue; // Skip if already generated
                    }

                    // Generate time slots based on consultation duration
                    var currentTime = workingHour.StartTime;
                    var endTime = workingHour.EndTime;
                    var duration = TimeSpan.FromMinutes(doctor.ConsultationDuration);

                    while (currentTime.Add(duration) <= endTime)
                    {
                        var startDateTime = date.Add(currentTime);
                        var endDateTime = startDateTime.Add(duration);

                        // Don't create slots in the past
                        if (startDateTime > DateTime.Now)
                        {
                            var timeSlot = new TimeSlot
                            {
                                WorkingHoursId = workingHour.WorkingHoursId,
                                StartDateTime = startDateTime,
                                EndDateTime = endDateTime,
                                IsBooked = false,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.TimeSlots.Add(timeSlot);
                        }

                        currentTime = currentTime.Add(duration);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Generate time slots for all active doctors
        /// </summary>
        public async Task GenerateTimeSlotsForAllDoctors(int daysAhead = 30)
        {
            var doctors = await _context.Doctors.ToListAsync();

            foreach (var doctor in doctors)
            {
                await GenerateTimeSlotsForDoctor(doctor.DoctorId, daysAhead);
            }
        }

        /// <summary>
        /// Clean up old time slots (past dates that weren't booked)
        /// </summary>
        public async Task CleanupOldTimeSlots()
        {
            var cutoffDate = DateTime.Now.AddDays(-7); // Keep last 7 days

            var oldSlots = await _context.TimeSlots
                .Where(t => t.StartDateTime < cutoffDate && !t.IsBooked)
                .ToListAsync();

            _context.TimeSlots.RemoveRange(oldSlots);
            await _context.SaveChangesAsync();
        }
    }
}