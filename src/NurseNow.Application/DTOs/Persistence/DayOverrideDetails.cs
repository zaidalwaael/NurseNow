namespace NurseNow.Application.DTOs.Persistence;

public sealed class DayOverrideDetails
{
    public int AvailabilityOverrideId { get; init; }
    public string date { get; init; } = default!;
    public string? startTime { get; init; } = default!;
    public string? endTime { get; init; } = default!;
    public bool IsBlocked { get; init; }
}
