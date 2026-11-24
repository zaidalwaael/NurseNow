namespace NurseNow.Application.DTOs.Persistence;

public sealed class ReservedTimeSlot
{
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
}
