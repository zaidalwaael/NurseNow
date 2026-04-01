namespace NurseNow.Models
{
    public class WeeklyAvailability
    {
        public int WeeklyAvailabilityId { get; set; }

        public string NurseId { get; set; }

        public string DayOfWeek { get; set; } = ""; // Monday, Tuesday, ...

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public ApplicationUser Nurse { get; set; }
    }
}