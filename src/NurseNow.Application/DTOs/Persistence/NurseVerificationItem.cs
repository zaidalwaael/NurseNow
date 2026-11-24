namespace NurseNow.Application.DTOs.Persistence;

public sealed class NurseVerificationItem
{
    public int nurseProfileId { get; init; }
    public string nurseId { get; init; } = default!;
    public string name { get; init; } = default!;
    public string? email { get; init; } = default!;
    public string? phone { get; init; } = default!;
    public DateTime registrationDate { get; init; }
    public string status { get; init; } = default!;
}
