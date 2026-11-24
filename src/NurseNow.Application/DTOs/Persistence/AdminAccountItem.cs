namespace NurseNow.Application.DTOs.Persistence;

public sealed class AdminAccountItem
{
    public string adminId { get; init; } = default!;
    public string fullName { get; init; } = default!;
    public string? email { get; init; } = default!;
    public DateTime createdDate { get; init; }
}
