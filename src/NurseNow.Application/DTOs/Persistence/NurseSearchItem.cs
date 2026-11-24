namespace NurseNow.Application.DTOs.Persistence;

public sealed class NurseSearchItem
{
    public string nurseId { get; init; } = default!;
    public string fullName { get; init; } = default!;
    public string? specialization { get; init; } = default!;
    public string? location { get; init; } = default!;
    public string? address { get; init; } = default!;
    public int? experienceYears { get; init; }
    public string? profileImageUrl { get; init; } = default!;
    public double rating { get; init; }
    public int reviewsCount { get; init; }
    public decimal? price { get; init; }
    public string availabilityLabel { get; init; } = default!;
}
