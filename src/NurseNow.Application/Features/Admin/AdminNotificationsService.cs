using NurseNow.Application.Repositories;
using NurseNow.Models;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminNotificationsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationUserRepository _users;
    private readonly INotificationRepository _notifications;
    private readonly IAdminActivityLogRepository _adminActivityLogs;
    public AdminNotificationsService(IUnitOfWork unitOfWork, IApplicationUserRepository users, INotificationRepository notifications, IAdminActivityLogRepository adminActivityLogs, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _users = users;
        _notifications = notifications;
        _adminActivityLogs = adminActivityLogs;
    }

    public async Task<OperationResult> GetAudienceCounts()
    {
        var patientsCount = await _users.CountPatientsAsync(_requestContext.CancellationToken);
        var nursesCount = await _users.CountNursesAsync(_requestContext.CancellationToken);
        return Ok(new { allUsers = patientsCount + nursesCount, nursesOnly = nursesCount, patientsOnly = patientsCount });
    }

    public async Task<OperationResult> SendNotification(SendAdminNotificationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Notification title is required" });
        if (string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(new { message = "Notification message is required" });
        if (dto.TargetAudience != "Nurses" && dto.TargetAudience != "Patients" && dto.TargetAudience != "All")
            return BadRequest(new { message = "Invalid target audience" });
        var users = await _users.ListRecipientsAsync(dto, _requestContext.CancellationToken);
        foreach (var user in users)
        {
            _notifications.Add(new Notification { UserId = user.Id, Title = dto.Title, Message = dto.Message, Type = "Account", TargetAudience = dto.TargetAudience, IsRead = false, CreatedAt = DateTime.UtcNow });
        }

        _adminActivityLogs.Add(new AdminActivityLog { Title = "Notification sent", Description = $"Admin sent notification '{dto.Title}' to {dto.TargetAudience}.", ActivityType = "Notification", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Notification sent successfully", sentTo = users.Count });
    }
}

public class SendAdminNotificationDto
{
    public string TargetAudience { get; set; } = "All";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
}
