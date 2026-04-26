using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.Models;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/notifications")]
    [Authorize(Roles = "Administrator")]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminNotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("audience-counts")]
        public async Task<IActionResult> GetAudienceCounts()
        {
            var patientsCount = await _context.Users
                .CountAsync(u => u.RoleType == "Patient");

            var nursesCount = await _context.Users
                .CountAsync(u => u.RoleType == "Nurse");

            return Ok(new
            {
                allUsers = patientsCount + nursesCount,
                nursesOnly = nursesCount,
                patientsOnly = patientsCount
            });
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification(SendAdminNotificationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Notification title is required" });

            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { message = "Notification message is required" });

            var usersQuery = _context.Users.AsQueryable();

            if (dto.TargetAudience == "Nurses")
            {
                usersQuery = usersQuery.Where(u => u.RoleType == "Nurse");
            }
            else if (dto.TargetAudience == "Patients")
            {
                usersQuery = usersQuery.Where(u => u.RoleType == "Patient");
            }
            else if (dto.TargetAudience == "All")
            {
                usersQuery = usersQuery.Where(u => u.RoleType == "Patient" || u.RoleType == "Nurse");
            }
            else
            {
                return BadRequest(new { message = "Invalid target audience" });
            }

            var users = await usersQuery.ToListAsync();

            foreach (var user in users)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Title = dto.Title,
                    Message = dto.Message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Notification sent",
                Description = $"Admin sent notification '{dto.Title}' to {dto.TargetAudience}.",
                ActivityType = "Notification",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Notification sent successfully",
                sentTo = users.Count
            });
        }
    }

    public class SendAdminNotificationDto
    {
        public string TargetAudience { get; set; } = "All";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
    }
}