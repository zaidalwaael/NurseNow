namespace NurseNow.Application.DTOs.Persistence;

public sealed class NurseServiceItem
{
    public int serviceId { get; init; }
    public int serviceCatalogId { get; init; }
    public string serviceName { get; init; } = default!;
    public int durationInMinutes { get; init; }
    public decimal price { get; init; }
}
