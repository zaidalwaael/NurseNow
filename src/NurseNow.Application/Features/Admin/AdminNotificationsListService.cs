using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminNotificationsListService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public AdminNotificationsListService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    public async Task<OperationResult> GetNotifications(string? search = "", string? type = "", string? status = "")
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var query = _context.Notifications.AsNoTracking().Where(n => n.UserId == userId).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(n => n.Title.Contains(search) || n.Message.Contains(search));
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

        var notifications = await query.OrderByDescending(n => n.CreatedAt).Select(n => new { notificationId = n.NotificationId, title = n.Title, message = n.Message, type = n.Type, isRead = n.IsRead, createdAt = n.CreatedAt }).ToListAsync(_requestContext.CancellationToken);
        var totalCount = await _context.Notifications.AsNoTracking().CountAsync(n => n.UserId == userId, _requestContext.CancellationToken);
        var unreadCount = await _context.Notifications.AsNoTracking().CountAsync(n => n.UserId == userId && !n.IsRead, _requestContext.CancellationToken);
        return Ok(new { totalCount, unreadCount, notifications });
    }

    public async Task<OperationResult> MarkAsRead(int id)
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId, _requestContext.CancellationToken);
        if (notification == null)
            return NotFound(new { message = "Notification not found" });
        notification.IsRead = true;
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Notification marked as read" });
    }

    public async Task<OperationResult> MarkAllAsRead()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), _requestContext.CancellationToken);
        return Ok(new { message = "All notifications marked as read" });
    }

    public async Task<OperationResult> DeleteNotification(int id)
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId, _requestContext.CancellationToken);
        if (notification == null)
            return NotFound(new { message = "Notification not found" });
        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Notification deleted successfully" });
    }
}
