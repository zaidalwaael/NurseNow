namespace NurseNow.DTOs.Admin
{
    public class MonthlyUsageItemDto
    {
        public string Month { get; set; } = "";
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
    }
}