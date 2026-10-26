namespace NurseNow.DTOs
{
    public class AddWeeklyAvailabilityDto
    {
        public string DayOfWeek { get; set; } = string.Empty; // Monday, Tuesday, ...
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
