namespace NurseNow.Models
{
    public class ScheduledNotification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime ScheduledTime { get; set; }
        public bool IsSent { get; set; } = false;
    }
}