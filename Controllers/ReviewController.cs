using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Models;
using System.Security.Claims;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class ReviewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewDto model)
        {
            var patientId = GetCurrentUserId();

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            if (model.Rating < 1 || model.Rating > 5)
                return BadRequest("Rating must be between 1 and 5.");

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == model.BookingId && b.PatientId == patientId);

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

            _context.Reviews.Add(review);

            booking.IsReviewSubmitted = true;
            booking.ReviewRemindLaterAt = null;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Review submitted successfully."
            });
        }

        [HttpPut("{bookingId}/dismiss")]
        public async Task<IActionResult> DismissReview(int bookingId)
        {
            var patientId = GetCurrentUserId();

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId);

            if (booking == null)
                return NotFound("Booking not found.");

            if (booking.Status != "Completed")
                return BadRequest("Only completed bookings can be dismissed from review.");

            if (booking.IsReviewSubmitted)
                return BadRequest("Review already submitted.");

            booking.IsReviewDismissed = true;
            booking.ReviewRemindLaterAt = null;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Review prompt dismissed successfully."
            });
        }

        [HttpPut("{bookingId}/later")]
        public async Task<IActionResult> RemindReviewLater(int bookingId)
        {
            var patientId = GetCurrentUserId();

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId);

            if (booking == null)
                return NotFound("Booking not found.");

            if (booking.Status != "Completed")
                return BadRequest("Only completed bookings can be reminded later.");

            if (booking.IsReviewSubmitted)
                return BadRequest("Review already submitted.");

            if (booking.IsReviewDismissed)
                return BadRequest("Review already dismissed.");

            booking.ReviewRemindLaterAt = DateTime.UtcNow.AddDays(1);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Review reminder postponed successfully."
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingReview()
        {
            var patientId = GetCurrentUserId();

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var now = DateTime.UtcNow;

            var booking = await _context.Bookings
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .Where(b =>
                    b.PatientId == patientId &&
                    b.Status == "Completed" &&
                    !b.IsReviewSubmitted &&
                    !b.IsReviewDismissed &&
                    (b.ReviewRemindLaterAt == null || b.ReviewRemindLaterAt <= now))
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .FirstOrDefaultAsync();

            if (booking == null)
                return Ok(null);

            return Ok(new
            {
                bookingId = booking.BookingId,
                nurseId = booking.NurseId,
                nurseName = booking.Nurse.FullName,
                serviceName = booking.Service.ServiceCatalog.Name,
                date = booking.BookingDate.ToString("yyyy-MM-dd"),
                time = booking.StartTime.ToString(@"hh\:mm")
            });
        }
    }
}