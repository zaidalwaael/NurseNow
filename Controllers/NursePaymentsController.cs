using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Nurse
{
    [ApiController]
    [Route("api/nurse/payments")]
    [Authorize(Roles = "Nurse")]
    public class NursePaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NursePaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET: api/nurse/payments/summary
        // =========================================
        [HttpGet("summary")]
        public async Task<IActionResult> GetPaymentSummary()
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            var payments = await _context.Payments
                .Include(p => p.Booking)
                .Where(p => p.Booking.NurseId == nurseId)
                .ToListAsync();

            var result = new NursePaymentSummaryDto
            {
                TotalEarnings = payments
                    .Where(p => p.Status == "Paid" || p.Status == "Completed")
                    .Sum(p => p.Amount),

                ThisMonthEarnings = payments
                    .Where(p =>
                        (p.Status == "Paid" || p.Status == "Completed") &&
                        p.CreatedAt >= monthStart &&
                        p.CreatedAt < nextMonthStart)
                    .Sum(p => p.Amount),

                PendingAmount = payments
                    .Where(p => p.Status == "Pending")
                    .Sum(p => p.Amount)
            };

            return Ok(result);
        }

        // =========================================
        // GET: api/nurse/payments/history
        // ?tab=all
        // ?tab=thisMonth
        // ?tab=history
        // =========================================
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory(string? tab = "all")
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .Where(p => p.Booking.NurseId == nurseId)
                .AsQueryable();

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

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new NursePaymentHistoryItemDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    PatientName = p.Booking.Patient.FullName,
                    ServiceName = p.Booking.Service.ServiceCatalog.Name,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(payments);
        }

        // =========================================
        // GET: api/nurse/payments/{id}
        // =========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentDetails(int id)
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var payment = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .Where(p => p.PaymentId == id && p.Booking.NurseId == nurseId)
                .Select(p => new NursePaymentHistoryDetailsDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,

                    PatientName = p.Booking.Patient.FullName,
                    NurseName = p.Booking.Nurse.FullName,
                    ServiceName = p.Booking.Service.ServiceCatalog.Name,

                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,

                    ServiceAddress = p.Booking.ServiceAddress,
                    BookingDate = p.Booking.BookingDate,
                    StartTime = p.Booking.StartTime,
                    EndTime = p.Booking.EndTime
                })
                .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound(new { message = "Payment not found." });

            return Ok(payment);
        }
    }
}