namespace NurseNow.Application.DTOs.Persistence;

public sealed class ServiceCatalogItem
{
    public int ServiceCatalogId { get; init; }
    public string Name { get; init; } = default!;
    public int DefaultDurationInMinutes { get; init; }
}
