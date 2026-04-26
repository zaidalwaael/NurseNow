using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.Models;
using System.Security.Claims;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/management")]
    [Authorize(Roles = "Administrator")]
    public class AdminManagementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private const string SuperAdminEmail = "admin@nursenow.com";

        public AdminManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsSuperAdmin()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            return email != null &&
                   email.ToLower() == SuperAdminEmail.ToLower();
        }

        [HttpGet]
        public async Task<IActionResult> GetAdmins()
        {
            if (!IsSuperAdmin())
                return Forbid();

            var admins = await _context.Users
                .Where(u => u.RoleType == "Administrator")
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    adminId = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    createdDate = u.CreatedAt
                })
                .ToListAsync();

            return Ok(admins);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(CreateAdminDto dto)
        {
            if (!IsSuperAdmin())
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest(new { message = "Full name is required" });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required" });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Password is required" });

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
                return BadRequest(new { message = "Email already exists" });

            var admin = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                RoleType = "Administrator",
                EmailConfirmed = true,
                AccountStatus = "Active",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(admin, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(admin, "Administrator");

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "New admin created",
                Description = $"{dto.FullName} was added as an administrator.",
                ActivityType = "AdminManagement",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin created successfully" });
        }

        [HttpDelete("{adminId}")]
        public async Task<IActionResult> DeleteAdmin(string adminId)
        {
            if (!IsSuperAdmin())
                return Forbid();

            var admin = await _userManager.FindByIdAsync(adminId);

            if (admin == null)
                return NotFound(new { message = "Admin not found" });

            if (admin.Email != null &&
                admin.Email.ToLower() == SuperAdminEmail.ToLower())
            {
                return BadRequest(new { message = "Main admin cannot be deleted" });
            }

            var result = await _userManager.DeleteAsync(admin);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Admin deleted",
                Description = $"{admin.FullName} was deleted from admin accounts.",
                ActivityType = "AdminManagement",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin deleted successfully" });
        }
    }

    public class CreateAdminDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}