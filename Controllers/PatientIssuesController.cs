using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Patient;
using NurseNow.Models;

namespace NurseNow.Controllers.Patient
{
    [ApiController]
    [Route("api/patient/issues")]
    [Authorize(Roles = "Patient")]
    public class PatientIssuesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientIssuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // POST: api/patient/issues
        // =========================================
        [HttpPost]
        public async Task<IActionResult> SubmitIssue([FromBody] SubmitPatientIssueDto dto)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
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
                return BadRequest(new { message = "Category is required." });

            if (!allowedCategories.Contains(dto.Category))
                return BadRequest(new { message = "Invalid category." });

            if (string.IsNullOrWhiteSpace(dto.Subject))
                return BadRequest(new { message = "Subject is required." });

            if (dto.Subject.Trim().Length > 200)
                return BadRequest(new { message = "Subject must not exceed 200 characters." });

            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "Description is required." });

            if (dto.Description.Trim().Length > 1000)
                return BadRequest(new { message = "Description must not exceed 1000 characters." });

            var complaint = new Complaint
            {
                UserId = patientId,
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
                Title = "New Patient Issue Report",
                Description = $"A patient submitted a new issue report: {dto.Subject}",
                ActivityType = "Complaint",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Issue submitted successfully.",
                complaintId = complaint.ComplaintId
            });
        }

        // =========================================
        // GET: api/patient/issues/my-reports
        // =========================================
        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var reports = await _context.Complaints
                .Where(c => c.UserId == patientId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new PatientIssueItemDto
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
        // GET: api/patient/issues/{id}
        // =========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIssueDetails(int id)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var report = await _context.Complaints
                .Where(c => c.ComplaintId == id && c.UserId == patientId)
                .Select(c => new PatientIssueDetailsDto
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
                return NotFound(new { message = "Issue report not found." });

            return Ok(report);
        }
    }
}