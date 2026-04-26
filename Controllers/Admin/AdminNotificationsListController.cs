using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using System.Security.Claims;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/notifications-list")]
    [Authorize(Roles = "Administrator")]
    public class AdminNotificationsListController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminNotificationsListController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(string? search = "", string? type = "", string? status = "")
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(n =>
                    n.Title.Contains(search) ||
                    n.Message.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(type) && type != "All")
            {
                query = query.Where(n => n.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                if (status == "Read")
                    query = query.Where(n => n.IsRead);

                if (status == "Unread")
                    query = query.Where(n => !n.IsRead);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    notificationId = n.NotificationId,
                    title = n.Title,
                    message = n.Message,
                    type = n.Type,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt
                })
                .ToListAsync();

            var totalCount = await _context.Notifications.CountAsync(n => n.UserId == userId);
            var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(new
            {
                totalCount,
                unreadCount,
                notifications
            });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
                return NotFound(new { message = "Notification not found" });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification marked as read" });
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "All notifications marked as read" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
                return NotFound(new { message = "Notification not found" });

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification deleted successfully" });
        }
    }
}