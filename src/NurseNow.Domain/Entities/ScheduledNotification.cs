namespace NurseNow.Models
{
    public class ScheduledNotification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public bool IsSent { get; set; } = false;
    }
}
