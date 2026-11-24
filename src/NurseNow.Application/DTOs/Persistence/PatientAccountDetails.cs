namespace NurseNow.Application.DTOs.Persistence;

public sealed class PatientAccountDetails
{
    public string? PhoneNumber { get; init; } = default!;
    public string? Gender { get; init; } = default!;
    public DateTime? DateOfBirth { get; init; }
    public string? BloodType { get; init; } = default!;
    public string? Governorate { get; init; } = default!;
    public string? Area { get; init; } = default!;
    public string? Address { get; init; } = default!;
    public string? Conditions { get; init; } = default!;
    public string? Allergies { get; init; } = default!;
    public string? Notes { get; init; } = default!;
}
