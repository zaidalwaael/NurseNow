using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Admin;
using NurseNow.Models;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/notifications")]
    [Authorize]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminNotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // POST: /api/admin/notifications/send
        // =====================================================
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TargetAudience))
                return BadRequest(new { message = "Target audience is required." });

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Title is required." });

            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new { message = "Message is required." });

            var validAudiences = new[] { "All Users", "Nurses Only", "Patients Only" };
            if (!validAudiences.Contains(dto.TargetAudience))
                return BadRequest(new { message = "Invalid target audience." });

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";

            var usersQuery = _context.Users.AsQueryable();

            if (dto.TargetAudience == "Nurses Only")
            {
                usersQuery = usersQuery.Where(u => u.RoleType == "Nurse");
            }
            else if (dto.TargetAudience == "Patients Only")
            {
                usersQuery = usersQuery.Where(u => u.RoleType == "Patient");
            }
            else
            {
                usersQuery = usersQuery.Where(u => u.RoleType == "Nurse" || u.RoleType == "Patient");
            }

            var users = await usersQuery.ToListAsync();

            if (!users.Any())
                return NotFound(new { message = "No users found for the selected audience." });

            var notifications = users.Select(user => new Notification
            {
                UserId = user.Id,
                Title = dto.Title,
                Message = dto.Message,
                Type = "Announcement",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                TargetAudience = dto.TargetAudience,
                SentByAdminId = adminId,
                SentByAdminName = adminName
            }).ToList();

            _context.Notifications.AddRange(notifications);

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Notification Sent",
                Description = $"Notification '{dto.Title}' sent to {dto.TargetAudience}.",
                ActivityType = "Notifications",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Notification sent successfully.",
                sentCount = notifications.Count
            });
        }

        // =====================================================
        // GET: /api/admin/notifications/stats
        // =====================================================
        [HttpGet("stats")]
        public async Task<IActionResult> GetNotificationStats()
        {
            var allUsers = await _context.Users
                .CountAsync(u => u.RoleType == "Nurse" || u.RoleType == "Patient");

            var nursesOnly = await _context.Users
                .CountAsync(u => u.RoleType == "Nurse");

            var patientsOnly = await _context.Users
                .CountAsync(u => u.RoleType == "Patient");

            var result = new NotificationStatsDto
            {
                AllUsers = allUsers,
                NursesOnly = nursesOnly,
                PatientsOnly = patientsOnly
            };

            return Ok(result);
        }

        // =====================================================
        // GET: /api/admin/notifications/recent
        // =====================================================
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentNotifications()
        {
            var recent = await _context.Notifications
                .Where(n => n.Type == "Announcement")
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new RecentNotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    TargetAudience = n.TargetAudience,
                    SentBy = string.IsNullOrWhiteSpace(n.SentByAdminName) ? "Admin" : n.SentByAdminName,
                    CreatedAt = n.CreatedAt
                })
                .Take(10)
                .ToListAsync();

            return Ok(recent);
        }


        // =====================================================
        // GET: /api/admin/all-notifications
        // =====================================================

        [HttpGet("/api/admin/all-notifications")]
        public async Task<IActionResult> GetAllAdminNotifications()
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new AdminPanelNotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = string.IsNullOrWhiteSpace(n.Type) ? "General" : n.Type,
                    SentBy = !string.IsNullOrWhiteSpace(n.SentByAdminName)
                        ? n.SentByAdminName
                        : "System",
                    TargetAudience = string.IsNullOrWhiteSpace(n.TargetAudience)
                        ? "Single User"
                        : n.TargetAudience,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }





    }
}