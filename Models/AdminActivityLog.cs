namespace NurseNow.Models
{
    public class AdminActivityLog
    {
        public int AdminActivityLogId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public string ActivityType { get; set; }  // Verification, Registration, Booking, Payment, Complaint, Service

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}