namespace NurseNow.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public string UserId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Type { get; set; }

        public int? BookingId { get; set; }

        public bool IsRead { get; set; } = false;

        public string TargetAudience { get; set; } = "Single User";
        public string? SentByAdminId { get; set; }
        public string? SentByAdminName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
    }
}