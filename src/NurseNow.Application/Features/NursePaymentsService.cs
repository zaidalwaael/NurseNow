using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NurseNow.DTOs.Nurse;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class NursePaymentsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public NursePaymentsService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    // =========================================
    // GET: api/nurse/payments/summary
    // =========================================
    public async Task<OperationResult> GetPaymentSummary()
    {
        var nurseId = _requestContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var payments = await _context.Payments.AsNoTracking().Include(p => p.Booking).Where(p => p.Booking.NurseId == nurseId).ToListAsync(_requestContext.CancellationToken);
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
        var nurseId = _requestContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var query = _context.Payments.AsNoTracking().Include(p => p.Booking).ThenInclude(b => b.Patient).Include(p => p.Booking).ThenInclude(b => b.Service).ThenInclude(s => s.ServiceCatalog).Where(p => p.Booking.NurseId == nurseId).AsQueryable();
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        if (tab == "thisMonth")
        {
            query = query.Where(p => p.CreatedAt >= monthStart && p.CreatedAt < nextMonthStart);
        }
        else if (tab == "history")
        {
            query = query.Where(p => p.Status == "Paid" || p.Status == "Completed");
        }

        var payments = await query.OrderByDescending(p => p.CreatedAt).Select(p => new NursePaymentHistoryItemDto { PaymentId = p.PaymentId, BookingId = p.BookingId, PatientName = p.Booking.Patient.FullName, ServiceName = p.Booking.Service.ServiceCatalog.Name, Amount = p.Amount, PaymentMethod = p.PaymentMethod, Status = p.Status, CreatedAt = p.CreatedAt }).ToListAsync(_requestContext.CancellationToken);
        return Ok(payments);
    }

    // =========================================
    // GET: api/nurse/payments/{id}
    // =========================================
    public async Task<OperationResult> GetPaymentDetails(int id)
    {
        var nurseId = _requestContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var payment = await _context.Payments.AsNoTracking().Include(p => p.Booking).ThenInclude(b => b.Patient).Include(p => p.Booking).ThenInclude(b => b.Nurse).Include(p => p.Booking).ThenInclude(b => b.Service).ThenInclude(s => s.ServiceCatalog).Where(p => p.PaymentId == id && p.Booking.NurseId == nurseId).Select(p => new NursePaymentHistoryDetailsDto { PaymentId = p.PaymentId, BookingId = p.BookingId, PatientName = p.Booking.Patient.FullName, NurseName = p.Booking.Nurse.FullName, ServiceName = p.Booking.Service.ServiceCatalog.Name, Amount = p.Amount, PaymentMethod = p.PaymentMethod, Status = p.Status, CreatedAt = p.CreatedAt, ServiceAddress = p.Booking.ServiceAddress, BookingDate = p.Booking.BookingDate, StartTime = p.Booking.StartTime, EndTime = p.Booking.EndTime }).FirstOrDefaultAsync(_requestContext.CancellationToken);
        if (payment == null)
            return NotFound(new { message = "Payment not found." });
        return Ok(payment);
    }
}
