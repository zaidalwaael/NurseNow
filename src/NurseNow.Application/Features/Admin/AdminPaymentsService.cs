using Microsoft.EntityFrameworkCore;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminPaymentsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public AdminPaymentsService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    // =============================
    // GET ALL TRANSACTIONS
    // =============================
    public async Task<OperationResult> GetAllTransactions()
    {
        var transactions = await _context.Payments.AsNoTracking().Include(p => p.Booking).ThenInclude(b => b.Patient).Include(p => p.Booking).ThenInclude(b => b.Nurse).Include(p => p.Booking).ThenInclude(b => b.Service).ThenInclude(s => s.ServiceCatalog).OrderByDescending(p => p.CreatedAt).Select(p => new { id = p.PaymentId, transactionId = "TXN-" + p.PaymentId, // مؤقت
 patientName = p.Booking.Patient.FullName, nurseName = p.Booking.Nurse.FullName, service = p.Booking.Service.ServiceCatalog.Name, totalAmount = p.Amount, status = p.Status, paymentMethod = p.PaymentMethod, date = p.CreatedAt }).ToListAsync(_requestContext.CancellationToken);
        return Ok(transactions);
    }

    // =============================
    // GET STATS
    // =============================
    public async Task<OperationResult> GetStats()
    {
        var totals = await _context.Payments.AsNoTracking().GroupBy(p => p.Status).Select(g => new { Status = g.Key, Total = g.Sum(p => p.Amount) }).ToDictionaryAsync(x => x.Status, x => x.Total, _requestContext.CancellationToken);
        var totalRevenue = totals.GetValueOrDefault("Paid");
        var refunded = totals.GetValueOrDefault("Refunded");
        var pending = totals.GetValueOrDefault("Pending");
        return Ok(new { totalRevenue, refunded, pending });
    }
}
