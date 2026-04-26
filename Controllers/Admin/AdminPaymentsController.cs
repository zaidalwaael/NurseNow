using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/admin/payments")]
    [Authorize(Roles = "Administrator")]
    public class AdminPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminPaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // GET ALL TRANSACTIONS
        // =============================
        [HttpGet]
        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    id = p.PaymentId,
                    transactionId = "TXN-" + p.PaymentId, // مؤقت
                    patientName = p.Booking.Patient.FullName,
                    nurseName = p.Booking.Nurse.FullName,
                    service = p.Booking.Service.ServiceCatalog.Name,
                    totalAmount = p.Amount,
                    status = p.Status,
                    paymentMethod = p.PaymentMethod,
                    date = p.CreatedAt
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // =============================
        // GET STATS
        // =============================
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var payments = await _context.Payments.ToListAsync();

            var totalRevenue = payments
                .Where(p => p.Status == "Paid")
                .Sum(p => p.Amount);

            var refunded = payments
                .Where(p => p.Status == "Refunded")
                .Sum(p => p.Amount);

            var pending = payments
                .Where(p => p.Status == "Pending")
                .Sum(p => p.Amount);

            return Ok(new
            {
                totalRevenue,
                refunded,
                pending
            });
        }
    }
}