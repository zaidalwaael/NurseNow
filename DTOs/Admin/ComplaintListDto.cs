namespace NurseNow.DTOs.Admin
{
    public class ComplaintListDto
    {
        public int ComplaintId { get; set; }
        public string ComplaintCode { get; set; } = "";
        public string SubmittedBy { get; set; } = "";
        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }
}