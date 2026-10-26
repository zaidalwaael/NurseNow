namespace NurseNow.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public bool IsRead { get; set; } = false;
        public string TargetAudience { get; set; } = "Single User";
        public string? SentByAdminId { get; set; }
        public string? SentByAdminName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ApplicationUser User { get; set; } = null !;
    }
}
