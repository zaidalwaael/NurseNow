namespace NurseNow.Application.DTOs.Persistence;

public sealed class NurseProfileServiceItem
{
    public int ServiceId { get; init; }
    public int ServiceCatalogId { get; init; }
    public string serviceName { get; init; } = default!;
    public int durationInMinutes { get; init; }
    public decimal Price { get; init; }
}
