namespace NurseNow.Application.DTOs.Persistence;

public sealed class PendingNurseSummary
{
    public string UserId { get; init; } = default!;
    public string? Specialization { get; init; } = default!;
    public int? ExperienceYears { get; init; }
    public string VerificationStatus { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string? Email { get; init; } = default!;
}
