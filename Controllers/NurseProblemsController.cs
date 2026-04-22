using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Nurse;
using NurseNow.Models;

namespace NurseNow.Controllers.Nurse
{
    [ApiController]
    [Route("api/nurse/problems")]
    [Authorize(Roles = "Nurse")]
    public class NurseProblemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NurseProblemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // POST: api/nurse/problems
        // =========================================
        [HttpPost]
        public async Task<IActionResult> SubmitProblem([FromBody] SubmitProblemDto dto)
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var allowedCategories = new[]
            {
                "Technical",
                "Payment",
                "Service Issue",
                "Account",
                "Other"
            };

            if (string.IsNullOrWhiteSpace(dto.Category))
                return BadRequest(new { message = "Problem category is required." });

            if (!allowedCategories.Contains(dto.Category))
                return BadRequest(new { message = "Invalid problem category." });

            if (string.IsNullOrWhiteSpace(dto.Subject))
                return BadRequest(new { message = "Subject is required." });

            if (dto.Subject.Trim().Length > 200)
                return BadRequest(new { message = "Subject must not exceed 200 characters." });

            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "Detailed description is required." });

            if (dto.Description.Trim().Length > 1000)
                return BadRequest(new { message = "Description must not exceed 1000 characters." });

            var complaint = new Complaint
            {
                UserId = nurseId,
                Category = dto.Category.Trim(),
                Subject = dto.Subject.Trim(),
                Description = dto.Description.Trim(),
                IsUrgent = dto.IsUrgent,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "New Nurse Problem Report",
                Description = $"A nurse submitted a new problem report: {dto.Subject}",
                ActivityType = "Complaint",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Problem report submitted successfully.",
                complaintId = complaint.ComplaintId
            });
        }

        // =========================================
        // GET: api/nurse/problems/my-reports
        // =========================================
        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var reports = await _context.Complaints
                .Where(c => c.UserId == nurseId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new NurseProblemItemDto
                {
                    ComplaintId = c.ComplaintId,
                    Category = c.Category,
                    Subject = c.Subject,
                    Status = c.Status,
                    IsUrgent = c.IsUrgent,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }

        // =========================================
        // GET: api/nurse/problems/{id}
        // =========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProblemDetails(int id)
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var report = await _context.Complaints
                .Where(c => c.ComplaintId == id && c.UserId == nurseId)
                .Select(c => new NurseProblemDetailsDto
                {
                    ComplaintId = c.ComplaintId,
                    Category = c.Category,
                    Subject = c.Subject,
                    Description = c.Description,
                    IsUrgent = c.IsUrgent,
                    Status = c.Status,
                    AdminResponse = c.AdminResponse,
                    CreatedAt = c.CreatedAt,
                    RespondedAt = c.RespondedAt
                })
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound(new { message = "Problem report not found." });

            return Ok(report);
        }
    }
}