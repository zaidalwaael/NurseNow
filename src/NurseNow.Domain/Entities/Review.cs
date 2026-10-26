namespace NurseNow.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int BookingId { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string NurseId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Booking Booking { get; set; } = null !;
        public ApplicationUser Patient { get; set; } = null !;
        public ApplicationUser Nurse { get; set; } = null !;
    }
}
