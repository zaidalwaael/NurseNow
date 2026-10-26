using Microsoft.EntityFrameworkCore;
using NurseNow.Models;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminNotificationsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public AdminNotificationsService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    public async Task<OperationResult> GetAudienceCounts()
    {
        var patientsCount = await _context.Users.AsNoTracking().CountAsync(u => u.RoleType == "Patient", _requestContext.CancellationToken);
        var nursesCount = await _context.Users.AsNoTracking().CountAsync(u => u.RoleType == "Nurse", _requestContext.CancellationToken);
        return Ok(new { allUsers = patientsCount + nursesCount, nursesOnly = nursesCount, patientsOnly = patientsCount });
    }

    public async Task<OperationResult> SendNotification(SendAdminNotificationDto dto)
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

        var users = await usersQuery.Select(u => new { u.Id }).ToListAsync(_requestContext.CancellationToken);
        foreach (var user in users)
        {
            _context.Notifications.Add(new Notification { UserId = user.Id, Title = dto.Title, Message = dto.Message, Type = "Account", TargetAudience = dto.TargetAudience, IsRead = false, CreatedAt = DateTime.UtcNow });
        }

        _context.AdminActivityLogs.Add(new AdminActivityLog { Title = "Notification sent", Description = $"Admin sent notification '{dto.Title}' to {dto.TargetAudience}.", ActivityType = "Notification", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Notification sent successfully", sentTo = users.Count });
    }
}

public class SendAdminNotificationDto
{
    public string TargetAudience { get; set; } = "All";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
}
