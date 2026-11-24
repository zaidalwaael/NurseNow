using NurseNow.Application.DTOs.Persistence;
using NurseNow.Models;
using NurseNow.DTOs;
using NurseNow.DTOs.Nurse;
using NurseNow.DTOs.Patient;
using NurseNow.Application.Features.Admin;
using Microsoft.EntityFrameworkCore;
using NurseNow.Application.Repositories;
using NurseNow.Data;

namespace NurseNow.Infrastructure.Persistence.Repositories;
public sealed class NotificationRepository(ApplicationDbContext context) : INotificationRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(Notification entity) => _context.Notifications.Add(entity);
    public void Remove(Notification entity) => _context.Notifications.Remove(entity);
    public async Task<List<NotificationItem>> ListForUserAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationItem
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                BookingId = n.BookingId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                targetScreen = n.Type == "Request" ? "RequestDetails" : n.Type == "Payment" ? "AppointmentDetails" : n.Type == "Booking" ? "AppointmentDetails" : n.Type == "Account" ? null : null
            }).ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetForUserAsync(int id, string? userId, CancellationToken cancellationToken)
    {
        return await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId, cancellationToken);
    }

    public async Task<int> MarkAllReadAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead).ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), cancellationToken);
    }

    public async Task<int> CountUnreadAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.Notifications.AsNoTracking().CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task<int> CountForUserAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.Notifications.AsNoTracking().CountAsync(n => n.UserId == userId, cancellationToken);
    }

    public async Task<List<AdminNotificationItem>> SearchForAdminAsync(string? search, string? type, string? status, string? userId, CancellationToken cancellationToken)
    {
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

        var notifications = await query.OrderByDescending(n => n.CreatedAt).Select(n => new AdminNotificationItem
        {
            notificationId = n.NotificationId,
            title = n.Title,
            message = n.Message,
            type = n.Type,
            isRead = n.IsRead,
            createdAt = n.CreatedAt
        }).ToListAsync(cancellationToken);
        return notifications;
    }

}
