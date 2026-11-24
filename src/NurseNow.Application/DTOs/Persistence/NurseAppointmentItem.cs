namespace NurseNow.Application.DTOs.Persistence;

public sealed class NurseAppointmentItem
{
    public int bookingId { get; init; }
    public string patientName { get; init; } = default!;
    public string serviceName { get; init; } = default!;
    public string date { get; init; } = default!;
    public string time { get; init; } = default!;
    public string address { get; init; } = default!;
    public decimal totalPrice { get; init; }
    public string status { get; init; } = default!;
}
