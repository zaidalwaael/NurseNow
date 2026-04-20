namespace NurseNow.DTOs.Admin
{
    public class ReportsOverviewDto
    {
        public int TotalRequests { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal Revenue { get; set; }
        public decimal RequestsGrowthPercentage { get; set; }
        public decimal CompletionGrowthPercentage { get; set; }
        public decimal RevenueGrowthPercentage { get; set; }
        public string CurrentMonthLabel { get; set; } = "";
    }
}