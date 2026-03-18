namespace NurseNow.DTOs
{
    public class UpdateWeeklyAvailabilityDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}