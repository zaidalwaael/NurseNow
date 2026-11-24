namespace NurseNow.Application.DTOs.Persistence;

public sealed class PatientReviewItem
{
    public int reviewId { get; init; }
    public string patientName { get; init; } = default!;
    public int rating { get; init; }
    public string? comment { get; init; } = default!;
    public DateTime createdAt { get; init; }
}
