using NurseNow.Application.Repositories;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminNotificationsListService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notifications;
    public AdminNotificationsListService(IUnitOfWork unitOfWork, INotificationRepository notifications, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<OperationResult> GetNotifications(string? search = "", string? type = "", string? status = "")
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var notifications = await _notifications.SearchForAdminAsync(search, type, status, userId, _requestContext.CancellationToken);
        var totalCount = await _notifications.CountForUserAsync(userId, _requestContext.CancellationToken);
        var unreadCount = await _notifications.CountUnreadAsync(userId, _requestContext.CancellationToken);
        return Ok(new { totalCount, unreadCount, notifications });
    }

    public async Task<OperationResult> MarkAsRead(int id)
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var notification = await _notifications.GetForUserAsync(id, userId, _requestContext.CancellationToken);
        if (notification == null)
            return NotFound(new { message = "Notification not found" });
        notification.IsRead = true;
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Notification marked as read" });
    }

    public async Task<OperationResult> MarkAllAsRead()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await using var transaction = await _unitOfWork.BeginTransactionAsync(_requestContext.CancellationToken);
        await _notifications.MarkAllReadAsync(userId, _requestContext.CancellationToken);
        await transaction.CommitAsync(_requestContext.CancellationToken);
        return Ok(new { message = "All notifications marked as read" });
    }

    public async Task<OperationResult> DeleteNotification(int id)
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var notification = await _notifications.GetForUserAsync(id, userId, _requestContext.CancellationToken);
        if (notification == null)
            return NotFound(new { message = "Notification not found" });
        _notifications.Remove(notification);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Notification deleted successfully" });
    }
}
