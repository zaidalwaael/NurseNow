namespace NurseNow.DTOs.Nurse
{
    public class NurseProblemDetailsDto
    {
        public int ComplaintId { get; set; }

        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";

        public bool IsUrgent { get; set; }

        public string Status { get; set; } = "";
        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}