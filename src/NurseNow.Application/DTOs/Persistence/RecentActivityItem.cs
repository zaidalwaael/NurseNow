namespace NurseNow.Application.DTOs.Persistence;

public sealed class RecentActivityItem
{
    public string title { get; init; } = default!;
    public string? description { get; init; } = default!;
    public string activityType { get; init; } = default!;
    public DateTime createdAt { get; init; }
}
