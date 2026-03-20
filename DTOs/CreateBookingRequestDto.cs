namespace NurseNow.DTOs
{
    public class CreateBookingRequestDto
    {
        public string NurseId { get; set; }

        public int ServiceId { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public string ServiceAddress { get; set; }

        public string? AdditionalNotes { get; set; }
    }
}