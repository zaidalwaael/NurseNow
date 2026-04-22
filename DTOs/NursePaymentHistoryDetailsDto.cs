namespace NurseNow.DTOs.Nurse
{
    public class NursePaymentHistoryDetailsDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }

        public string PatientName { get; set; } = "";
        public string NurseName { get; set; } = "";
        public string ServiceName { get; set; } = "";

        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public string ServiceAddress { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}