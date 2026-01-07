namespace ClinicAppointmentSystem.ViewModels
{
    public class CalendarViewModel
    {
        public List<CalendarEvent> Events { get; set; }
    }

    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Color { get; set; }
        public string Status { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Reason { get; set; }
    }
}