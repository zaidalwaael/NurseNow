using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Admin;
using NurseNow.Models;

namespace NurseNow.Controllers
{
    [Route("api/admin/nurse-verifications")]
    [ApiController]
    [Authorize]
    public class AdminNurseVerificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminNurseVerificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: /api/admin/nurse-verifications
        // List nurses for verification table
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetNurseVerifications(
     [FromQuery] string? search,
     [FromQuery] string? status)
        {
            var query =
                from u in _context.Users
                join np in _context.NurseProfiles on u.Id equals np.UserId into nurseProfiles
                from np in nurseProfiles.DefaultIfEmpty()
                where u.RoleType == "Nurse"
                select new { u, np };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower();

                query = query.Where(x =>
                    (x.u.FullName != null && x.u.FullName.ToLower().Contains(keyword)) ||
                    (x.u.Email != null && x.u.Email.ToLower().Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(x => x.u.AdminApprovalStatus == status);
            }

            var nurses = await query
                .OrderByDescending(x => x.u.CreatedAt)
                .Select(x => new NurseVerificationDto
                {
                    Id = x.u.Id,
                    NurseId = x.u.Id,
                    FullName = x.u.FullName ?? "",
                    Email = x.u.Email ?? "",
                    Phone = x.np != null && !string.IsNullOrWhiteSpace(x.np.PhoneNumber)
                        ? x.np.PhoneNumber
                        : "-",
                    RegistrationDate = x.u.CreatedAt == default ? null : x.u.CreatedAt,
                    Status = string.IsNullOrWhiteSpace(x.u.AdminApprovalStatus)
                        ? "Pending"
                        : x.u.AdminApprovalStatus
                })
                .ToListAsync();

            return Ok(nurses);
        }

        // =====================================================
        // GET: /api/admin/nurse-verifications/{id}
        // Review popup details
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNurseVerificationDetails(string id)
        {
            var nurseData = await (
                from u in _context.Users
                join np in _context.NurseProfiles on u.Id equals np.UserId into nurseProfiles
                from np in nurseProfiles.DefaultIfEmpty()
                where u.Id == id && u.RoleType == "Nurse"
                select new { u, np }
            ).FirstOrDefaultAsync();

            if (nurseData == null)
                return NotFound(new { message = "Nurse not found." });

            var documents = await _context.Set<NurseDocument>()
                .Where(d => d.NurseId == id)
                .Select(d => new NurseVerificationDocumentDto
                {
                    Id = d.Id,
                    Name = d.DocumentName,
                    FileUrl = d.FileUrl
                })
                .ToListAsync();

            var result = new NurseVerificationDetailsDto
            {
                Id = nurseData.u.Id,
                NurseId = nurseData.u.Id,
                FullName = nurseData.u.FullName ?? "",
                Email = nurseData.u.Email ?? "",
                Phone = nurseData.np != null && !string.IsNullOrWhiteSpace(nurseData.np.PhoneNumber)
                    ? nurseData.np.PhoneNumber
                    : "-",
                RegistrationDate = nurseData.u.CreatedAt == default ? null : nurseData.u.CreatedAt,
                Status = string.IsNullOrWhiteSpace(nurseData.u.AdminApprovalStatus)
                    ? "Pending"
                    : nurseData.u.AdminApprovalStatus,
                RejectionReason = nurseData.u.RejectionReason,
                Documents = documents
            };

            return Ok(result);
        }
        // =====================================================
        // PUT: /api/admin/nurse-verifications/{id}/approve
        // =====================================================
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveNurseVerification(string id)
        {
            var nurse = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleType == "Nurse");

            if (nurse == null)
                return NotFound(new { message = "Nurse not found." });

            nurse.AdminApprovalStatus = "Approved";
            nurse.RejectionReason = null;

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Nurse Approved",
                Description = $"Nurse {nurse.FullName} has been approved.",
                ActivityType = "Verification",
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                UserId = nurse.Id,
                Title = "Verification Approved",
                Message = "Your nurse verification has been approved.",
                Type = "Verification",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Nurse approved successfully." });
        }

        // =====================================================
        // PUT: /api/admin/nurse-verifications/{id}/reject
        // =====================================================
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectNurseVerification(
            string id,
            [FromBody] RejectNurseVerificationDto dto)
        {
            var nurse = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleType == "Nurse");

            if (nurse == null)
                return NotFound(new { message = "Nurse not found." });

            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "Rejection reason is required." });

            nurse.AdminApprovalStatus = "Rejected";
            nurse.RejectionReason = dto.Reason;

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Nurse Rejected",
                Description = $"Nurse {nurse.FullName} has been rejected. Reason: {dto.Reason}",
                ActivityType = "Verification",
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                UserId = nurse.Id,
                Title = "Verification Rejected",
                Message = $"Your nurse verification has been rejected. Reason: {dto.Reason}",
                Type = "Verification",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Nurse rejected successfully." });
        }

        // =====================================================
        // GET: /api/admin/nurse-verifications/{id}/documents/{documentId}
        // =====================================================
        [HttpGet("{id}/documents/{documentId}")]
        public async Task<IActionResult> GetNurseDocument(string id, int documentId)
        {
            var document = await _context.Set<NurseDocument>()
                .FirstOrDefaultAsync(d => d.Id == documentId && d.NurseId == id);

            if (document == null)
                return NotFound(new { message = "Document not found." });

            return Ok(new
            {
                id = document.Id,
                name = document.DocumentName,
                fileUrl = document.FileUrl
            });
        }
    }
}