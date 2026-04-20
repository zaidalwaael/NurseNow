using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Admin;
using NurseNow.Models;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/complaints")]
    [Authorize]
    public class AdminComplaintsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminComplaintsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: /api/admin/complaints?search=john&status=Open
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetComplaints(
            [FromQuery] string? search,
            [FromQuery] string? status)
        {
            var query = _context.Complaints
                .Include(c => c.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(c => c.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower();

                query = query.Where(c =>
                    (c.Subject != null && c.Subject.ToLower().Contains(keyword)) ||
                    (c.Category != null && c.Category.ToLower().Contains(keyword)) ||
                    (c.User.FullName != null && c.User.FullName.ToLower().Contains(keyword)));
            }

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ComplaintListDto
                {
                    ComplaintId = c.ComplaintId,
                    ComplaintCode = $"C{c.ComplaintId:D3}",
                    SubmittedBy = $"{c.User.FullName} ({c.User.RoleType})",
                    Category = c.Category,
                    Subject = c.Subject,
                    CreatedAt = c.CreatedAt,
                    Status = c.Status
                })
                .ToListAsync();

            return Ok(complaints);
        }

        // =====================================================
        // GET: /api/admin/complaints/{id}
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComplaintById(int id)
        {
            var complaint = await _context.Complaints
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null)
                return NotFound(new { message = "Complaint not found." });

            var result = new ComplaintDetailsDto
            {
                ComplaintId = complaint.ComplaintId,
                ComplaintCode = $"C{complaint.ComplaintId:D3}",
                SubmittedBy = $"{complaint.User.FullName} ({complaint.User.RoleType})",
                UserEmail = complaint.User.Email ?? "",
                Category = complaint.Category,
                Subject = complaint.Subject,
                Description = complaint.Description,
                Status = complaint.Status,
                CreatedAt = complaint.CreatedAt,
                AdminResponse = complaint.AdminResponse,
                RespondedAt = complaint.RespondedAt
            };

            return Ok(result);
        }

        // =====================================================
        // PUT: /api/admin/complaints/{id}/respond
        // =====================================================
        [HttpPut("{id}/respond")]
        public async Task<IActionResult> RespondToComplaint(int id, [FromBody] RespondComplaintDto dto)
        {
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null)
                return NotFound(new { message = "Complaint not found." });

            if (string.IsNullOrWhiteSpace(dto.Response))
                return BadRequest(new { message = "Response is required." });

            complaint.AdminResponse = dto.Response;
            complaint.RespondedAt = DateTime.UtcNow;

            if (complaint.Status == "Open")
                complaint.Status = "In Progress";

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Complaint Responded",
                Description = $"Complaint #{complaint.ComplaintId} received an admin response.",
                ActivityType = "Complaints",
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                UserId = complaint.UserId,
                Title = "Complaint Update",
                Message = $"Your complaint #{complaint.ComplaintId} has received a response from admin.",
                Type = "Complaint",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Response sent successfully." });
        }

        // =====================================================
        // PUT: /api/admin/complaints/{id}/resolve
        // =====================================================
        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveComplaint(int id)
        {
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.ComplaintId == id);

            if (complaint == null)
                return NotFound(new { message = "Complaint not found." });

            complaint.Status = "Resolved";

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Complaint Resolved",
                Description = $"Complaint #{complaint.ComplaintId} marked as resolved.",
                ActivityType = "Complaints",
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                UserId = complaint.UserId,
                Title = "Complaint Resolved",
                Message = $"Your complaint #{complaint.ComplaintId} has been marked as resolved.",
                Type = "Complaint",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Complaint marked as resolved." });
        }
    }
}