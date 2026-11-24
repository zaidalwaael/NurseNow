using NurseNow.Application.Repositories;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class NotificationService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notifications;
    public NotificationService(IUnitOfWork unitOfWork, INotificationRepository notifications, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<OperationResult> GetNotifications()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var notifications = await _notifications.ListForUserAsync(userId, _requestContext.CancellationToken);
        return Ok(notifications);
    }

    public async Task<OperationResult> MarkAsRead(int id)
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var notification = await _notifications.GetForUserAsync(id, userId, _requestContext.CancellationToken);
        if (notification == null)
            return NotFound("Notification not found.");
        notification.IsRead = true;
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Notification marked as read." });
    }

    public async Task<OperationResult> MarkAllAsRead()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        await using var transaction = await _unitOfWork.BeginTransactionAsync(_requestContext.CancellationToken);
        await _notifications.MarkAllReadAsync(userId, _requestContext.CancellationToken);
        await transaction.CommitAsync(_requestContext.CancellationToken);
        return Ok(new { message = "All notifications marked as read." });
    }
}
