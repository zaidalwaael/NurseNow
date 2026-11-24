namespace NurseNow.Application.DTOs.Persistence;

public sealed class RequestStatusCount
{
    public string status { get; init; } = default!;
    public int count { get; init; }
}
