namespace NurseNow.Application.DTOs.Persistence;

public sealed class NurseRequestItem
{
    public int bookingId { get; init; }
    public string patientName { get; init; } = default!;
    public string? patientPhone { get; init; } = default!;
    public string serviceName { get; init; } = default!;
    public string date { get; init; } = default!;
    public string time { get; init; } = default!;
    public string location { get; init; } = default!;
    public int durationInMinutes { get; init; }
    public decimal totalPrice { get; init; }
    public string? notes { get; init; } = default!;
    public string status { get; init; } = default!;
}
