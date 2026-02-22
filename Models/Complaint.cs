namespace NurseNow.Models
{
    public class Complaint
    {
        public int ComplaintId { get; set; }

        public int BookingId { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public Booking Booking { get; set; }
    }
}
