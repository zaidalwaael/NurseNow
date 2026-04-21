using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Models;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("nurse-details/{userId}")]
        public async Task<IActionResult> GetNurseDetails(string userId)
        {
            var nurseProfile = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse not found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                nurseProfile.UserId,
                nurseProfile.User.FullName,
                nurseProfile.User.Email,

                nurseProfile.Specialization,
                nurseProfile.ExperienceYears,
                nurseProfile.PhoneNumber,
                nurseProfile.Address,
                nurseProfile.Location,
                nurseProfile.NationalId,
                nurseProfile.VerificationStatus,

                ProfileImageUrl = nurseProfile.ProfileImagePath != null
                    ? $"{baseUrl}/{nurseProfile.ProfileImagePath}"
                    : null,

                CertificateUrl = nurseProfile.CertificatePath != null
                    ? $"{baseUrl}/{nurseProfile.CertificatePath}"
                    : null,

                NationalIdImageUrl = nurseProfile.NationalIdImagePath != null
                    ? $"{baseUrl}/{nurseProfile.NationalIdImagePath}"
                    : null
            });
        }

        [HttpPut("verify-nurse/{userId}")]
        public async Task<IActionResult> UpdateNurseStatus(string userId, [FromBody] UpdateNurseStatusDto model)
        {
            var nurseProfile = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found");

            model.Status = model.Status?.Trim();

            if (model.Status != "Approved" && model.Status != "Rejected")
                return BadRequest("Invalid status");

            if (model.Status == "Approved")
            {
                if (string.IsNullOrEmpty(nurseProfile.Specialization) ||
                    nurseProfile.ExperienceYears == null ||
                    string.IsNullOrEmpty(nurseProfile.PhoneNumber) ||
                    string.IsNullOrEmpty(nurseProfile.Address) ||
                    string.IsNullOrEmpty(nurseProfile.Location) ||
                    string.IsNullOrEmpty(nurseProfile.NationalId) ||
                    string.IsNullOrEmpty(nurseProfile.NationalIdImagePath) ||
                    string.IsNullOrEmpty(nurseProfile.CertificatePath) ||
                    string.IsNullOrEmpty(nurseProfile.LicenseNumber))
                {
                    return BadRequest("Nurse profile is incomplete. Cannot approve.");
                }
            }

            nurseProfile.VerificationStatus = model.Status;

            var notificationMessage = model.Status == "Approved"
                ? "Your account has been approved."
                : "Your account has been rejected.";

            _context.Notifications.Add(new Notification
            {
                UserId = nurseProfile.UserId,
                Title = model.Status == "Approved" ? "Account Approved" : "Account Rejected",
                Message = notificationMessage,
                Type = "Account",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok($"Nurse status updated to {model.Status}");
        }

        [HttpGet("pending-nurses")]
        public async Task<IActionResult> GetPendingNurses()
        {
            var pendingNurses = await _context.NurseProfiles
                .Include(n => n.User)
                .Where(n => n.VerificationStatus == "Pending")
                .Select(n => new
                {
                    n.UserId,
                    n.Specialization,
                    n.ExperienceYears,
                    n.VerificationStatus,
                    FullName = n.User.FullName,
                    Email = n.User.Email
                })
                .ToListAsync();

            return Ok(pendingNurses);
        }
    }
}