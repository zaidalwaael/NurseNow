using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.Models;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/nurse-verification")]
    [Authorize(Roles = "Administrator")]
    public class AdminNurseVerificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminNurseVerificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingNurses([FromQuery] string? search)
        {
            var query = _context.NurseProfiles
                .Include(n => n.User)
                .Where(n => n.VerificationStatus == "Pending")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(n =>
                    n.User.FullName.Contains(search) ||
                    n.User.Email.Contains(search));
            }

            var nurses = await query
                .OrderByDescending(n => n.NurseProfileId)
                .Select(n => new
                {
                    nurseProfileId = n.NurseProfileId,
                    nurseId = n.UserId,
                    name = n.User.FullName,
                    email = n.User.Email,
                    phone = n.PhoneNumber,
                    registrationDate = n.User.CreatedAt,
                    status = n.VerificationStatus
                })
                .ToListAsync();

            return Ok(nurses);
        }

        [HttpGet("{nurseProfileId}")]
        public async Task<IActionResult> GetNurseDetails(int nurseProfileId)
        {
            var nurse = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NurseProfileId == nurseProfileId);

            if (nurse == null)
                return NotFound(new { message = "Nurse not found" });

            var documents = await _context.NurseDocuments
                .Where(d => d.NurseId == nurse.UserId)
                .Select(d => new
                {
                    id = d.Id,
                    documentName = d.DocumentName,
                    fileUrl = d.FileUrl
                })
                .ToListAsync();

            return Ok(new
            {
                nurseProfileId = nurse.NurseProfileId,
                nurseId = nurse.UserId,
                fullName = nurse.User.FullName,
                email = nurse.User.Email,
                phoneNumber = nurse.PhoneNumber,
                location = nurse.Location,
                address = nurse.Address,
                specialization = nurse.Specialization,
                experienceYears = nurse.ExperienceYears,
                licenseNumber = nurse.LicenseNumber,
                nationalId = nurse.NationalId,
                bio = nurse.Bio,
                status = nurse.VerificationStatus,
                rejectionReason = nurse.RejectionReason,
                registrationDate = nurse.User.CreatedAt,
                profileImagePath = nurse.ProfileImagePath,
                certificatePath = nurse.CertificatePath,
                nationalIdImagePath = nurse.NationalIdImagePath,
                documents
            });
        }

        [HttpPut("{nurseProfileId}/approve")]
        public async Task<IActionResult> ApproveNurse(int nurseProfileId)
        {
            var nurse = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NurseProfileId == nurseProfileId);

            if (nurse == null)
                return NotFound(new { message = "Nurse not found" });

            nurse.VerificationStatus = "Approved";
            nurse.RejectionReason = null;

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Nurse verification approved",
                Description = $"{nurse.User.FullName} has been approved.",
                ActivityType = "Verification",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Nurse approved successfully" });
        }

        [HttpPut("{nurseProfileId}/reject")]
        public async Task<IActionResult> RejectNurse(int nurseProfileId, RejectNurseDto dto)
        {
            var nurse = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NurseProfileId == nurseProfileId);

            if (nurse == null)
                return NotFound(new { message = "Nurse not found" });

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                return BadRequest(new { message = "Rejection reason is required" });

            nurse.VerificationStatus = "Rejected";
            nurse.RejectionReason = dto.RejectionReason;

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Nurse verification rejected",
                Description = $"{nurse.User.FullName} has been rejected. Reason: {dto.RejectionReason}",
                ActivityType = "Verification",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Nurse rejected successfully" });
        }
    }

    public class RejectNurseDto
    {
        public string RejectionReason { get; set; } = "";
    }
}