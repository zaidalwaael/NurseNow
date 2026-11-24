namespace NurseNow.Application.DTOs.Persistence;

public sealed class ServiceRequestItem
{
    public int requestId { get; init; }
    public string patientName { get; init; } = default!;
    public string nurseName { get; init; } = default!;
    public string serviceType { get; init; } = default!;
    public DateTime dateTime { get; init; }
    public string status { get; init; } = default!;
}
