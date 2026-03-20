namespace NurseNow.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public string PatientId { get; set; }

        public string NurseId { get; set; }

        public int ServiceId { get; set; }

        public DateTime BookingDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string ServiceAddress { get; set; }

        public string? AdditionalNotes { get; set; }

        public string Status { get; set; } = "Pending";

        public ApplicationUser Patient { get; set; }

        public ApplicationUser Nurse { get; set; }

        public Service Service { get; set; }
    }
}