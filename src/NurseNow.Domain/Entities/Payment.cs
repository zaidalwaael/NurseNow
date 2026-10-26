namespace NurseNow.Models
{
    public class Payment
    {
        public string? StripePaymentIntentId { get; set; }
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Booking Booking { get; set; } = null !;
        public string PaymentMethod { get; set; } = "Cash";
    }
}
