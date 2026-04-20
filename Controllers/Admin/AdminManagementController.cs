using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Models;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/admin-management")]
    [Authorize(Roles = "Administrator")]
    public class AdminManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminManagementController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // =========================================
        // GET ALL ADMINS
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GetAdmins()
        {
            var admins = await _userManager.Users
                .Where(u => u.RoleType == "Admin")
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    createdAt = u.CreatedAt,
                    lastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            return Ok(admins);
        }

        // =========================================
        // GET ADMIN DETAILS
        // =========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminDetails(string id)
        {
            var admin = await _userManager.Users
                .Where(u => u.Id == id && u.RoleType == "Admin")
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    location = u.Location,
                    phoneNumber = u.PhoneNumber,
                    createdAt = u.CreatedAt,
                    lastLoginAt = u.LastLoginAt
                })
                .FirstOrDefaultAsync();

            if (admin == null)
                return NotFound(new { message = "Admin not found." });

            return Ok(admin);
        }

        // =========================================
        // CREATE ADMIN
        // =========================================
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new
                {
                    message = "Full name, email, and password are required."
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists." });
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                Location = dto.Location,
                PhoneNumber = dto.PhoneNumber,
                RoleType = "Admin",
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);

            if (!createResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Create admin failed.",
                    errors = createResult.Errors.Select(e => e.Description)
                });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Administrator");

            if (!roleResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Admin created but role assignment failed.",
                    errors = roleResult.Errors.Select(e => e.Description)
                });
            }

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Admin Created",
                Description = $"Created new admin account for {user.FullName}",
                ActivityType = "AdminManagement",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin created successfully." });
        }

        // =========================================
        // UPDATE ADMIN
        // =========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(string id, [FromBody] UpdateAdminDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.RoleType != "Admin")
            {
                return NotFound(new { message = "Admin not found." });
            }

            user.FullName = dto.FullName;
            user.Location = dto.Location;
            user.PhoneNumber = dto.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Failed to update admin.",
                    errors = updateResult.Errors.Select(e => e.Description)
                });
            }

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Admin Updated",
                Description = $"Updated admin account for {user.FullName}",
                ActivityType = "AdminManagement",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin updated successfully." });
        }

        // =========================================
        // DELETE ADMIN
        // =========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.RoleType != "Admin")
            {
                return NotFound(new { message = "Admin not found." });
            }

            var fullName = user.FullName;

            var deleteResult = await _userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Failed to delete admin.",
                    errors = deleteResult.Errors.Select(e => e.Description)
                });
            }

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Admin Deleted",
                Description = $"Deleted admin account for {fullName}",
                ActivityType = "AdminManagement",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin deleted successfully." });
        }

        // =========================================
        // RECENT ADMIN ACTIONS
        // =========================================
        [HttpGet("recent-actions")]
        public async Task<IActionResult> GetRecentActions()
        {
            var actions = await _context.AdminActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new
                {
                    title = a.Title,
                    description = a.Description,
                    activityType = a.ActivityType,
                    createdAt = a.CreatedAt,
                    adminName = "Admin"
                })
                .ToListAsync();

            return Ok(actions);
        }
    }
}