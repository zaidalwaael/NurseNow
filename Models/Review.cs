namespace NurseNow.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int BookingId { get; set; }

        public string PatientId { get; set; }

        public string NurseId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Booking Booking { get; set; }

        public ApplicationUser Patient { get; set; }

        public ApplicationUser Nurse { get; set; }
    }
}