namespace NurseNow.DTOs.Admin
{
    public class TransactionListDto
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; } = "";
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string NurseName { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public decimal Commission { get; set; }
        public decimal NurseAmount { get; set; }
        public string Status { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}