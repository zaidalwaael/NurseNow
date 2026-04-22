namespace NurseNow.DTOs.Patient
{
    public class PatientIssueItemDto
    {
        public int ComplaintId { get; set; }
        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsUrgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}