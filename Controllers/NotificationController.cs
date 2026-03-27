using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using System.Security.Claims;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.BookingId,
                    n.IsRead,
                    n.CreatedAt,
                    targetScreen =
                        n.Type == "Request" ? "RequestDetails" :
                        n.Type == "Payment" ? "AppointmentDetails" :
                        n.Type == "Booking" ? "AppointmentDetails" :
                        n.Type == "Account" ? null :
                        null
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
                return NotFound("Notification not found.");

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Notification marked as read."
            });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "All notifications marked as read."
            });
        }



    }
}