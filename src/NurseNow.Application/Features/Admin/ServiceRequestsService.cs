using Microsoft.EntityFrameworkCore;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class ServiceRequestsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public ServiceRequestsService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    // ✅ 1. Get All Requests (مع Search + Filter)
    public async Task<OperationResult> GetAll(string? search = "", string? status = "")
    {
        var query = _context.Bookings.AsNoTracking().Include(b => b.Patient).Include(b => b.Nurse).Include(b => b.Service).ThenInclude(s => s.ServiceCatalog).AsQueryable();
        // 🔍 Search
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b => b.Patient.FullName.Contains(search) || b.Nurse.FullName.Contains(search) || b.Service.ServiceCatalog.Name.Contains(search));
        }

        // 🎯 Filter by Status
        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            query = query.Where(b => b.Status == status);
        }

        var data = await query.OrderByDescending(b => b.CreatedAt).Select(b => new { requestId = b.BookingId, patientName = b.Patient.FullName, nurseName = b.Nurse != null ? b.Nurse.FullName : "Unassigned", serviceType = b.Service.ServiceCatalog.Name, dateTime = b.BookingDate, status = b.Status }).ToListAsync(_requestContext.CancellationToken);
        return Ok(data);
    }

    // ✅ 2. Get Request Details
    public async Task<OperationResult> GetDetails(int id)
    {
        var booking = await _context.Bookings.AsNoTracking().Include(b => b.Patient).Include(b => b.Nurse).Include(b => b.Service).ThenInclude(s => s.ServiceCatalog).Include(b => b.Payment).FirstOrDefaultAsync(b => b.BookingId == id, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound();
        var result = new
        {
            requestId = booking.BookingId,
            patient = new
            {
                name = booking.Patient.FullName,
                email = booking.Patient.Email,
                phone = booking.Patient.PhoneNumber
            },
            nurse = booking.Nurse == null ? null : new
            {
                name = booking.Nurse.FullName,
                email = booking.Nurse.Email,
                phone = booking.Nurse.PhoneNumber
            },
            service = new
            {
                name = booking.Service.ServiceCatalog.Name,
                price = booking.Service.Price
            },
            bookingDate = booking.BookingDate,
            startTime = booking.StartTime,
            endTime = booking.EndTime,
            address = booking.ServiceAddress,
            notes = booking.AdditionalNotes,
            status = booking.Status,
            payment = booking.Payment == null ? null : new
            {
                amount = booking.Payment.Amount,
                status = booking.Payment.Status,
                method = booking.Payment.PaymentMethod,
                createdAt = booking.Payment.CreatedAt
            }
        };
        return Ok(result);
    }
}
