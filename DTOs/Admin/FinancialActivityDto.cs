namespace NurseNow.DTOs.Admin
{
    public class FinancialActivityDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public string Type { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}