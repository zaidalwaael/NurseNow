using NurseNow.Application.Repositories;
using System.Security.Claims;
using NurseNow.DTOs.Nurse;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class NursePaymentsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IPaymentRepository _payments;
    public NursePaymentsService(IPaymentRepository payments, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _payments = payments;
    }

    // =========================================
    // GET: api/nurse/payments/summary
    // =========================================
    public async Task<OperationResult> GetPaymentSummary()
    {
        var nurseId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var payments = await _payments.ListForNurseSummaryAsync(nurseId, _requestContext.CancellationToken);
        var result = new NursePaymentSummaryDto
        {
            TotalEarnings = payments.Where(p => p.Status == "Paid" || p.Status == "Completed").Sum(p => p.Amount),
            ThisMonthEarnings = payments.Where(p => (p.Status == "Paid" || p.Status == "Completed") && p.CreatedAt >= monthStart && p.CreatedAt < nextMonthStart).Sum(p => p.Amount),
            PendingAmount = payments.Where(p => p.Status == "Pending").Sum(p => p.Amount)
        };
        return Ok(result);
    }

    // =========================================
    // GET: api/nurse/payments/history
    // ?tab=all
    // ?tab=thisMonth
    // ?tab=history
    // =========================================
    public async Task<OperationResult> GetPaymentHistory(string? tab = "all")
    {
        var nurseId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var payments = await _payments.ListNurseHistoryAsync(tab, nurseId, _requestContext.CancellationToken);
        return Ok(payments);
    }

    // =========================================
    // GET: api/nurse/payments/{id}
    // =========================================
    public async Task<OperationResult> GetPaymentDetails(int id)
    {
        var nurseId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var payment = await _payments.GetNurseDetailsAsync(id, nurseId, _requestContext.CancellationToken);
        if (payment == null)
            return NotFound(new { message = "Payment not found." });
        return Ok(payment);
    }
}
