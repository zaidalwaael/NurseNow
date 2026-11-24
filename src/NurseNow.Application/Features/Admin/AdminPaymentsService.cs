using NurseNow.Application.Repositories;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminPaymentsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IPaymentRepository _payments;
    public AdminPaymentsService(IPaymentRepository payments, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _payments = payments;
    }

    // =============================
    // GET ALL TRANSACTIONS
    // =============================
    public async Task<OperationResult> GetAllTransactions()
    {
        var transactions = await _payments.ListTransactionsAsync(_requestContext.CancellationToken);
        return Ok(transactions);
    }

    // =============================
    // GET STATS
    // =============================
    public async Task<OperationResult> GetStats()
    {
        var totals = await _payments.GetTotalsByStatusAsync(_requestContext.CancellationToken);
        var totalRevenue = totals.GetValueOrDefault("Paid");
        var refunded = totals.GetValueOrDefault("Refunded");
        var pending = totals.GetValueOrDefault("Pending");
        return Ok(new { totalRevenue, refunded, pending });
    }
}
