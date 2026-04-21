namespace NurseNow.Models
{
    public class Complaint
    {
        public int ComplaintId { get; set; }
        public string UserId { get; set; }
        public string Category { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? AdminResponse { get; set; }
        public DateTime? RespondedAt { get; set; }

        public ApplicationUser User { get; set; }
    }
}