using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Patient;

namespace NurseNow.Controllers.Patient
{
    [ApiController]
    [Route("api/patient/payments")]
    [Authorize(Roles = "Patient")]
    public class PatientPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientPaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET: api/patient/payments/history
        // =========================================
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var history = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .Where(p => p.Booking.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PatientPaymentHistoryItemDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,

                    Amount = p.Amount,
                    Status = p.Status,
                    PaymentMethod = p.PaymentMethod,
                    CreatedAt = p.CreatedAt,

                    NurseName = p.Booking.Nurse.FullName,
                    ServiceName = p.Booking.Service.ServiceCatalog.Name
                })
                .ToListAsync();

            return Ok(history);
        }

        // =========================================
        // GET: api/patient/payments/{id}
        // =========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentDetails(int id)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var payment = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .Where(p => p.PaymentId == id && p.Booking.PatientId == patientId)
                .Select(p => new PatientPaymentDetailsDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,

                    Amount = p.Amount,
                    Status = p.Status,
                    PaymentMethod = p.PaymentMethod,
                    CreatedAt = p.CreatedAt,

                    PatientName = p.Booking.Patient.FullName,
                    NurseName = p.Booking.Nurse.FullName,
                    ServiceName = p.Booking.Service.ServiceCatalog.Name,

                    BookingDate = p.Booking.BookingDate,
                    StartTime = p.Booking.StartTime,
                    EndTime = p.Booking.EndTime,
                    ServiceAddress = p.Booking.ServiceAddress
                })
                .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound(new { message = "Transaction not found." });

            return Ok(payment);
        }
    }
}