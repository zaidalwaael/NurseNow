namespace NurseNow.Application.Abstractions;
public sealed record PaymentIntentData(string Id, string ClientSecret);
public interface IPaymentGateway
{
    Task<PaymentIntentData> CreateAsync(int bookingId, string patientId, decimal amount, CancellationToken cancellationToken);
    Task<PaymentIntentData> GetAsync(string intentId, CancellationToken cancellationToken);
    Task<bool> IsPaidAsync(string intentId, int bookingId, string patientId, decimal amount, CancellationToken cancellationToken);
}
