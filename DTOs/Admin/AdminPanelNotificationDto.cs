namespace NurseNow.DTOs.Admin
{
    public class AdminPanelNotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "";
        public string SentBy { get; set; } = "";
        public string TargetAudience { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}