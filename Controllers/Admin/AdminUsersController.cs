using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Admin;
using NurseNow.Models;

namespace NurseNow.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize]
    public class AdminUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =====================================================
        // GET: /api/admin/users?role=Patient&status=Active&search=ali
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? role,
            [FromQuery] string? status,
            [FromQuery] string? search)
        {
            var users = await _context.Users
                .Where(u => u.RoleType == "Nurse" || u.RoleType == "Patient")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var nurseProfiles = await _context.NurseProfiles.ToListAsync();
            var patientProfiles = await _context.PatientProfiles.ToListAsync();

            var result = users.Select(u =>
            {
                var nurseProfile = nurseProfiles.FirstOrDefault(x => x.UserId == u.Id);
                var patientProfile = patientProfiles.FirstOrDefault(x => x.UserId == u.Id);

                var phone = u.RoleType == "Nurse"
                    ? (nurseProfile?.PhoneNumber ?? "-")
                    : (patientProfile?.PhoneNumber ?? "-");

                return new AdminUserDto
                {
                    Id = u.Id,
                    UserCode = u.RoleType == "Nurse" ? $"N-{u.Id}" : $"P-{u.Id}",
                    FullName = u.FullName ?? "",
                    Email = u.Email ?? "",
                    Phone = string.IsNullOrWhiteSpace(phone) ? "-" : phone,
                    JoinDate = u.CreatedAt == default ? null : u.CreatedAt,
                    Status = string.IsNullOrWhiteSpace(u.AccountStatus) ? "Active" : u.AccountStatus,
                    Role = u.RoleType
                };
            });

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
            {
                result = result.Where(x => x.Role == role);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                result = result.Where(x => x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower();
                result = result.Where(x =>
                    x.FullName.ToLower().Contains(keyword) ||
                    x.Email.ToLower().Contains(keyword));
            }

            return Ok(result.ToList());
        }

        // =====================================================
        // GET: /api/admin/users/{id}
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && (u.RoleType == "Nurse" || u.RoleType == "Patient"));

            if (user == null)
                return NotFound(new { message = "User not found." });

            var nurseProfile = await _context.NurseProfiles
                .FirstOrDefaultAsync(x => x.UserId == id);

            var patientProfile = await _context.PatientProfiles
                .FirstOrDefaultAsync(x => x.UserId == id);

            var phone = user.RoleType == "Nurse"
                ? (nurseProfile?.PhoneNumber ?? "-")
                : (patientProfile?.PhoneNumber ?? "-");

            var address = user.RoleType == "Nurse"
                ? nurseProfile?.Address
                : patientProfile?.Address;

            var result = new AdminUserDetailsDto
            {
                Id = user.Id,
                UserCode = user.RoleType == "Nurse" ? $"N-{user.Id}" : $"P-{user.Id}",
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                Phone = string.IsNullOrWhiteSpace(phone) ? "-" : phone,
                JoinDate = user.CreatedAt == default ? null : user.CreatedAt,
                Status = string.IsNullOrWhiteSpace(user.AccountStatus) ? "Active" : user.AccountStatus,
                Role = user.RoleType,
                Address = string.IsNullOrWhiteSpace(address) ? "-" : address
            };

            return Ok(result);
        }

        // =====================================================
        // PUT: /api/admin/users/{id}/toggle-status
        // =====================================================
        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && (u.RoleType == "Nurse" || u.RoleType == "Patient"));

            if (user == null)
                return NotFound(new { message = "User not found." });

            var currentStatus = string.IsNullOrWhiteSpace(user.AccountStatus)
                ? "Active"
                : user.AccountStatus;

            user.AccountStatus = currentStatus == "Suspended" ? "Active" : "Suspended";

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "User Status Updated",
                Description = $"User {user.FullName} status changed to {user.AccountStatus}.",
                ActivityType = "Users",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User status updated successfully.",
                status = user.AccountStatus
            });
        }

        // =====================================================
        // PUT: /api/admin/users/{id}/reset-password
        // =====================================================
        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(string id, [FromBody] ResetUserPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { message = "New password is required." });

            var user = await _userManager.FindByIdAsync(id);

            if (user == null || (user.RoleType != "Nurse" && user.RoleType != "Patient"))
                return NotFound(new { message = "User not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!resetResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Failed to reset password.",
                    errors = resetResult.Errors.Select(e => e.Description)
                });
            }

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Password Reset",
                Description = $"Password reset for user {user.FullName}.",
                ActivityType = "Users",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully." });
        }
    }
}