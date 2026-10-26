using System.Globalization;
using Microsoft.Extensions.Configuration;
using NurseNow.Application.Abstractions;
using Stripe;

namespace NurseNow.Infrastructure.Payments;
public sealed class StripePaymentGateway(IConfiguration configuration) : IPaymentGateway
{
    private PaymentIntentService Service() => new(new StripeClient(configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Configure Stripe:SecretKey.")));
    private static long MinorUnits(decimal amount)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Invalid payment amount.");
        return checked((long)(amount * 100m)); // Existing currency is USD.
    }

    public async Task<PaymentIntentData> CreateAsync(int bookingId, string patientId, decimal amount, CancellationToken cancellationToken)
    {
        var intent = await Service().CreateAsync(new PaymentIntentCreateOptions { Amount = MinorUnits(amount), Currency = "usd", PaymentMethodTypes = new List<string> { "card" }, Metadata = new Dictionary<string, string> { ["bookingId"] = bookingId.ToString(CultureInfo.InvariantCulture), ["patientId"] = patientId } }, new RequestOptions { IdempotencyKey = $"nursenow-booking-{bookingId}" }, cancellationToken);
        return new PaymentIntentData(intent.Id, intent.ClientSecret);
    }

    public async Task<PaymentIntentData> GetAsync(string intentId, CancellationToken cancellationToken)
    {
        var intent = await Service().GetAsync(intentId, cancellationToken: cancellationToken);
        return new PaymentIntentData(intent.Id, intent.ClientSecret);
    }

    public async Task<bool> IsPaidAsync(string intentId, int bookingId, string patientId, decimal amount, CancellationToken cancellationToken)
    {
        var intent = await Service().GetAsync(intentId, cancellationToken: cancellationToken);
        return intent.Status == "succeeded" && intent.Currency == "usd" && intent.AmountReceived == MinorUnits(amount) && intent.Metadata.TryGetValue("bookingId", out var booking) && booking == bookingId.ToString(CultureInfo.InvariantCulture) && intent.Metadata.TryGetValue("patientId", out var patient) && patient == patientId;
    }
}
