using NurseNow.Application.DTOs.Persistence;
using NurseNow.Models;
using NurseNow.DTOs;
using NurseNow.DTOs.Nurse;
using NurseNow.DTOs.Patient;
using NurseNow.Application.Features.Admin;

namespace NurseNow.Application.Repositories;

/// <summary>Persistence operations; Add/Remove stage changes for IUnitOfWork.
/// Queries ending in ForUpdate (and verification/status update queries) return tracked entities.
/// Read projections are materialized inside Infrastructure.</summary>
public interface INotificationRepository
{
    void Add(Notification entity);
    void Remove(Notification entity);
    Task<List<NotificationItem>> ListForUserAsync(string? userId, CancellationToken cancellationToken);
    Task<Notification?> GetForUserAsync(int id, string? userId, CancellationToken cancellationToken);
    /// <summary>Immediate set-based update. Call inside an IUnitOfWork transaction.</summary>
    Task<int> MarkAllReadAsync(string? userId, CancellationToken cancellationToken);
    Task<int> CountUnreadAsync(string? userId, CancellationToken cancellationToken);
    Task<int> CountForUserAsync(string? userId, CancellationToken cancellationToken);
    Task<List<AdminNotificationItem>> SearchForAdminAsync(string? search, string? type, string? status, string? userId, CancellationToken cancellationToken);
}
