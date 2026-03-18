namespace NurseNow.DTOs
{
    public class OverrideDayAvailabilityDto
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}