namespace NurseNow.DTOs.Admin
{
    public class TransactionStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal NursePayouts { get; set; }
        public decimal RefundedTransactions { get; set; }
        public decimal PendingPayments { get; set; }
    }
}