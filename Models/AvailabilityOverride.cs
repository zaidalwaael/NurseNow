namespace NurseNow.Models
{
    public class AvailabilityOverride
    {
        public int AvailabilityOverrideId { get; set; }

        public string NurseId { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public bool IsBlocked { get; set; } = false;

        public ApplicationUser Nurse { get; set; }
    }
}