namespace NurseNow.Application.DTOs.Persistence;

public sealed class DayScheduleDetails
{
    public int WeeklyAvailabilityId { get; init; }
    public string DayOfWeek { get; init; } = default!;
    public string startTime { get; init; } = default!;
    public string endTime { get; init; } = default!;
}
