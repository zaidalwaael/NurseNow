namespace NurseNow.Application.DTOs.Persistence;

public sealed class AdminUserItem
{
    public string userId { get; init; } = default!;
    public string name { get; init; } = default!;
    public string? email { get; init; } = default!;
    public string? phone { get; init; } = default!;
    public DateTime joinDate { get; init; }
    public string status { get; init; } = default!;
    public string roleType { get; init; } = default!;
}
