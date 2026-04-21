namespace NurseNow.DTOs.Admin
{
    public class NursePerformanceItemDto
    {
        public string NurseName { get; set; } = "";
        public int CompletedRequests { get; set; }
        public decimal AverageRating { get; set; }
        public decimal RevenueGenerated { get; set; }
    }
}