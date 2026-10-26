using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NurseNow.DTOs.Patient;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class PatientPaymentsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public PatientPaymentsService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    // =========================================
    // GET: api/patient/payments/history
    // =========================================
    public async Task<OperationResult> GetPaymentHistory()
    {
        var patientId = _requestContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var history = await _context.Payments.AsNoTracking().Include(p => p.Booking).ThenInclude(b => b.Nurse).Include(p => p.Booking).ThenInclude(b => b.Service).ThenInclude(s => s.ServiceCatalog).Where(p => p.Booking.PatientId == patientId).OrderByDescending(p => p.CreatedAt).Select(p => new PatientPaymentHistoryItemDto { PaymentId = p.PaymentId, BookingId = p.BookingId, Amount = p.Amount, Status = p.Status, PaymentMethod = p.PaymentMethod, CreatedAt = p.CreatedAt, NurseName = p.Booking.Nurse.FullName, ServiceName = p.Booking.Service.ServiceCatalog.Name }).ToListAsync(_requestContext.CancellationToken);
        return Ok(history);
    }

    // =========================================
    // GET: api/patient/payments/{id}
    // =========================================
    public async Task<OperationResult> GetPaymentDetails(int id)
    {
        var patientId = _requestContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var payment = await _context.Payments.AsNoTracking().Include(p => p.Booking).ThenInclude(b => b.Patient).Include(p => p.Booking).ThenInclude(b => b.Nurse).Include(p => p.Booking).ThenInclude(b => b.Service).ThenInclude(s => s.ServiceCatalog).Where(p => p.PaymentId == id && p.Booking.PatientId == patientId).Select(p => new PatientPaymentDetailsDto { PaymentId = p.PaymentId, BookingId = p.BookingId, Amount = p.Amount, Status = p.Status, PaymentMethod = p.PaymentMethod, CreatedAt = p.CreatedAt, PatientName = p.Booking.Patient.FullName, NurseName = p.Booking.Nurse.FullName, ServiceName = p.Booking.Service.ServiceCatalog.Name, BookingDate = p.Booking.BookingDate, StartTime = p.Booking.StartTime, EndTime = p.Booking.EndTime, ServiceAddress = p.Booking.ServiceAddress }).FirstOrDefaultAsync(_requestContext.CancellationToken);
        if (payment == null)
            return NotFound(new { message = "Transaction not found." });
        return Ok(payment);
    }
}
