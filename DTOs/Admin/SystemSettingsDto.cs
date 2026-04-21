namespace NurseNow.DTOs.Admin
{
    public class SystemSettingsDto
    {
        public bool AutoAssignNurse { get; set; }
        public bool RequireDocumentVerification { get; set; }
        public bool EmailNotifications { get; set; }

        public bool NotifyNewNurse { get; set; }
        public bool NotifyNewServiceRequest { get; set; }
        public bool NotifyNewComplaint { get; set; }

        public int SessionTimeout { get; set; }
        public int MinimumPasswordLength { get; set; }
    }
}