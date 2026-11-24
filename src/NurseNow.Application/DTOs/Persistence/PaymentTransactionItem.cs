namespace NurseNow.Application.DTOs.Persistence;

public sealed class PaymentTransactionItem
{
    public int id { get; init; }
    public string transactionId { get; init; } = default!;
    public string patientName { get; init; } = default!;
    public string nurseName { get; init; } = default!;
    public string service { get; init; } = default!;
    public decimal totalAmount { get; init; }
    public string status { get; init; } = default!;
    public string paymentMethod { get; init; } = default!;
    public DateTime date { get; init; }
}
