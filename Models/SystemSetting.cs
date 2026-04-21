namespace NurseNow.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }

        public bool AutoAssignNurse { get; set; } = true;
        public bool RequireDocumentVerification { get; set; } = true;
        public bool EmailNotifications { get; set; } = true;

        public bool NotifyNewNurse { get; set; } = true;
        public bool NotifyNewServiceRequest { get; set; } = true;
        public bool NotifyNewComplaint { get; set; } = true;

        public int SessionTimeout { get; set; } = 30;
        public int MinimumPasswordLength { get; set; } = 8;
    }
}