namespace NurseNow.Application.DTOs.Persistence;

public sealed class NurseAccountDetails
{
    public string? PhoneNumber { get; init; } = default!;
    public string? Location { get; init; } = default!;
    public string? Address { get; init; } = default!;
    public string? Specialization { get; init; } = default!;
    public int? ExperienceYears { get; init; }
    public string? LicenseNumber { get; init; } = default!;
    public string VerificationStatus { get; init; } = default!;
    public string? Bio { get; init; } = default!;
}
