using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Admin;
using NurseNow.Models;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/requests")]
    [Authorize]
    public class AdminRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: /api/admin/requests?search=ali&status=Pending
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetRequests(
            [FromQuery] string? search,
            [FromQuery] string? status)
        {
            var query = _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(b => b.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower();

                query = query.Where(b =>
                    (b.Patient.FullName != null && b.Patient.FullName.ToLower().Contains(keyword)) ||
                    (b.Nurse != null && b.Nurse.FullName != null && b.Nurse.FullName.ToLower().Contains(keyword)) ||
                    (b.Service != null && b.Service.ServiceCatalog != null && b.Service.ServiceCatalog.Name.ToLower().Contains(keyword))
                );
            }

            var requests = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Select(b => new ServiceRequestListDto
                {
                    RequestId = b.BookingId,
                    PatientName = b.Patient.FullName ?? "",
                    AssignedNurse = b.Nurse != null ? b.Nurse.FullName ?? "" : "Unassigned",
                    ServiceType = b.Service != null && b.Service.ServiceCatalog != null
                        ? b.Service.ServiceCatalog.Name
                        : "",
                    BookingDate = b.BookingDate,
                    StartTime = b.StartTime.ToString(@"hh\:mm"),
                    Status = b.Status
                })
                .ToListAsync();

            return Ok(requests);
        }

        // =====================================================
        // GET: /api/admin/requests/{id}
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound(new { message = "Request not found." });

            var patientPhone = await _context.PatientProfiles
                .Where(p => p.UserId == booking.PatientId)
                .Select(p => p.PhoneNumber)
                .FirstOrDefaultAsync();

            var result = new ServiceRequestDetailsDto
            {
                RequestId = booking.BookingId,
                PatientName = booking.Patient.FullName ?? "",
                PatientEmail = booking.Patient.Email ?? "",
                PatientPhone = string.IsNullOrWhiteSpace(patientPhone) ? "-" : patientPhone,
                AssignedNurse = booking.Nurse != null ? booking.Nurse.FullName ?? "" : "Unassigned",
                NurseId = booking.NurseId ?? "",
                ServiceType = booking.Service != null && booking.Service.ServiceCatalog != null
                    ? booking.Service.ServiceCatalog.Name
                    : "",
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime.ToString(@"hh\:mm"),
                EndTime = booking.EndTime.ToString(@"hh\:mm"),
                Status = booking.Status,
                ServiceAddress = booking.ServiceAddress ?? "",
                AdditionalNotes = booking.AdditionalNotes ?? ""
            };

            return Ok(result);
        }

        // =====================================================
        // PUT: /api/admin/requests/{id}/reassign
        // =====================================================
        [HttpPut("{id}/reassign")]
        public async Task<IActionResult> ReassignRequest(int id, [FromBody] ReassignServiceRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NurseId))
                return BadRequest(new { message = "NurseId is required." });

            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound(new { message = "Request not found." });

            var nurseExists = await _context.Users
                .AnyAsync(u => u.Id == dto.NurseId && u.RoleType == "Nurse");

            if (!nurseExists)
                return BadRequest(new { message = "Selected nurse not found." });

            booking.NurseId = dto.NurseId;
            booking.Status = "Assigned";

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Request Reassigned",
                Description = $"Booking #{booking.BookingId} was reassigned to nurse {dto.NurseId}.",
                ActivityType = "Requests",
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                UserId = booking.PatientId,
                Title = "Request Updated",
                Message = $"Your request #{booking.BookingId} has been reassigned.",
                Type = "Booking",
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                UserId = dto.NurseId,
                Title = "New Assigned Request",
                Message = $"You have been assigned to booking #{booking.BookingId}.",
                Type = "Booking",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Request reassigned successfully." });
        }

        // =====================================================
        // PUT: /api/admin/requests/{id}/cancel
        // =====================================================
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelRequest(int id, [FromBody] CancelServiceRequestDto? dto)
        {
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
                return NotFound(new { message = "Request not found." });

            booking.Status = "Cancelled";

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Request Cancelled",
                Description = string.IsNullOrWhiteSpace(dto?.Reason)
                    ? $"Booking #{booking.BookingId} was cancelled by admin."
                    : $"Booking #{booking.BookingId} was cancelled by admin. Reason: {dto!.Reason}",
                ActivityType = "Requests",
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                UserId = booking.PatientId,
                Title = "Request Cancelled",
                Message = string.IsNullOrWhiteSpace(dto?.Reason)
                    ? $"Your request #{booking.BookingId} has been cancelled."
                    : $"Your request #{booking.BookingId} has been cancelled. Reason: {dto!.Reason}",
                Type = "Booking",
                CreatedAt = DateTime.UtcNow
            });

            if (!string.IsNullOrWhiteSpace(booking.NurseId))
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = booking.NurseId,
                    Title = "Request Cancelled",
                    Message = $"Booking #{booking.BookingId} has been cancelled.",
                    Type = "Booking",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Request cancelled successfully." });
        }
    }
}