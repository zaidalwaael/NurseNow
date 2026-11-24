using NurseNow.Application.Repositories;
using NurseNow.DTOs;
using NurseNow.Models;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class ReviewService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookingRepository _bookings;
    private readonly IReviewRepository _reviews;
    public ReviewService(IUnitOfWork unitOfWork, IBookingRepository bookings, IReviewRepository reviews, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _bookings = bookings;
        _reviews = reviews;
    }

    private string? GetCurrentUserId()
    {
        return _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<OperationResult> SubmitReview(SubmitReviewDto model)
    {
        var patientId = GetCurrentUserId();
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        if (model.Rating < 1 || model.Rating > 5)
            return BadRequest("Rating must be between 1 and 5.");
        var booking = await _bookings.GetReviewBookingForUpdateAsync(model, patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        if (booking.Status != "Completed")
            return BadRequest("Review can only be submitted for completed bookings.");
        if (booking.IsReviewSubmitted)
            return BadRequest("Review has already been submitted for this booking.");
        if (booking.IsReviewDismissed)
            return BadRequest("Review was dismissed for this booking.");
        var review = new Review
        {
            BookingId = booking.BookingId,
            PatientId = booking.PatientId,
            NurseId = booking.NurseId,
            Rating = model.Rating,
            Comment = model.Comment
        };
        _reviews.Add(review);
        booking.IsReviewSubmitted = true;
        booking.ReviewRemindLaterAt = null;
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Review submitted successfully." });
    }

    public async Task<OperationResult> DismissReview(int bookingId)
    {
        var patientId = GetCurrentUserId();
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var booking = await _bookings.GetPatientBookingForUpdateAsync(bookingId, patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        if (booking.Status != "Completed")
            return BadRequest("Only completed bookings can be dismissed from review.");
        if (booking.IsReviewSubmitted)
            return BadRequest("Review already submitted.");
        booking.IsReviewDismissed = true;
        booking.ReviewRemindLaterAt = null;
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Review prompt dismissed successfully." });
    }

    public async Task<OperationResult> RemindReviewLater(int bookingId)
    {
        var patientId = GetCurrentUserId();
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var booking = await _bookings.GetPatientBookingForUpdateAsync(bookingId, patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        if (booking.Status != "Completed")
            return BadRequest("Only completed bookings can be reminded later.");
        if (booking.IsReviewSubmitted)
            return BadRequest("Review already submitted.");
        if (booking.IsReviewDismissed)
            return BadRequest("Review already dismissed.");
        booking.ReviewRemindLaterAt = DateTime.UtcNow.AddDays(1);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Review reminder postponed successfully." });
    }

    public async Task<OperationResult> GetPendingReview()
    {
        var patientId = GetCurrentUserId();
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var now = DateTime.UtcNow;
        var booking = await _bookings.GetPendingReviewAsync(patientId, now, _requestContext.CancellationToken);
        if (booking == null)
            return Ok(null);
        return Ok(new { bookingId = booking.BookingId, nurseId = booking.NurseId, nurseName = booking.Nurse.FullName, serviceName = booking.Service.ServiceCatalog.Name, date = booking.BookingDate.ToString("yyyy-MM-dd"), time = booking.StartTime.ToString(@"hh\:mm") });
    }
}
