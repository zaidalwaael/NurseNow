using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class NotificationService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public NotificationService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    public async Task<OperationResult> GetNotifications()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var notifications = await _context.Notifications.AsNoTracking().Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).Select(n => new { n.NotificationId, n.Title, n.Message, n.Type, n.BookingId, n.IsRead, n.CreatedAt, targetScreen = n.Type == "Request" ? "RequestDetails" : n.Type == "Payment" ? "AppointmentDetails" : n.Type == "Booking" ? "AppointmentDetails" : n.Type == "Account" ? null : null }).ToListAsync(_requestContext.CancellationToken);
        return Ok(notifications);
    }

    public async Task<OperationResult> MarkAsRead(int id)
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId, _requestContext.CancellationToken);
        if (notification == null)
            return NotFound("Notification not found.");
        notification.IsRead = true;
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Notification marked as read." });
    }

    public async Task<OperationResult> MarkAllAsRead()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), _requestContext.CancellationToken);
        return Ok(new { message = "All notifications marked as read." });
    }
}
