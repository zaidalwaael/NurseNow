namespace NurseNow.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public string PatientId { get; set; }
        public string NurseId { get; set; }

        public DateTime BookingDate { get; set; }

        public string Status { get; set; }
        public string PaymentStatus { get; set; }

        public ApplicationUser Patient { get; set; }
        public ApplicationUser Nurse { get; set; }

        public Payment Payment { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
    }
}
