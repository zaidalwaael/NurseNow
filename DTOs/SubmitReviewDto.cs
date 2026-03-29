namespace NurseNow.DTOs
{
    public class SubmitReviewDto
    {
        public int BookingId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}