namespace NurseNow.DTOs.Nurse
{
    public class NursePaymentSummaryDto
    {
        public decimal TotalEarnings { get; set; }
        public decimal ThisMonthEarnings { get; set; }
        public decimal PendingAmount { get; set; }
    }
}