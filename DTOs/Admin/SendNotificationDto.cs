namespace NurseNow.DTOs.Admin
{
    public class SendNotificationDto
    {
        public string TargetAudience { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
    }
}