using NurseNow.Application.Repositories;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class ServiceRequestsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IBookingRepository _bookings;
    public ServiceRequestsService(IBookingRepository bookings, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _bookings = bookings;
    }

    // ✅ 1. Get All Requests (مع Search + Filter)
    public async Task<OperationResult> GetAll(string? search = "", string? status = "")
    {
        var data = await _bookings.SearchRequestsAsync(search, status, _requestContext.CancellationToken);
        return Ok(data);
    }

    // ✅ 2. Get Request Details
    public async Task<OperationResult> GetDetails(int id)
    {
        var booking = await _bookings.GetDetailsAsync(id, _requestContext.CancellationToken);
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
