using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;

namespace NurseNow.Controllers.Admin
{
    [Route("api/admin/service-requests")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ 1. Get All Requests (مع Search + Filter)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search = "",
            string? status = ""
        )
        {
            var query = _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .AsQueryable();

            // 🔍 Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.Patient.FullName.Contains(search) ||
                    b.Nurse.FullName.Contains(search) ||
                    b.Service.ServiceCatalog.Name.Contains(search)
                );
            }

            // 🎯 Filter by Status
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(b => b.Status == status);
            }

            var data = await query
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    requestId = b.BookingId,
                    patientName = b.Patient.FullName,
                    nurseName = b.Nurse != null ? b.Nurse.FullName : "Unassigned",
                    serviceType = b.Service.ServiceCatalog.Name,
                    dateTime = b.BookingDate,
                    status = b.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ 2. Get Request Details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == id);

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
}