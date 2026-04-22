namespace NurseNow.DTOs.Nurse
{
    public class NursePaymentHistoryItemDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }

        public string PatientName { get; set; } = "";
        public string ServiceName { get; set; } = "";

        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}