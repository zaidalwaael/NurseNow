namespace NurseNow.DTOs.Admin
{
    public class ComplaintDetailsDto
    {
        public int ComplaintId { get; set; }
        public string ComplaintCode { get; set; } = "";
        public string SubmittedBy { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string? AdminResponse { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}