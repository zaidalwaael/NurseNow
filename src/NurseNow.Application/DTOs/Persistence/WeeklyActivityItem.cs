namespace NurseNow.Application.DTOs.Persistence;

public sealed class WeeklyActivityItem
{
    public DateTime date { get; init; }
    public int requests { get; init; }
    public int completed { get; init; }
}
