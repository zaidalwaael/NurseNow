namespace NurseNow.Application.DTOs.Persistence;

public sealed class PaymentStatusTotal
{
    public string Status { get; init; } = default!;
    public decimal Total { get; init; }
}
