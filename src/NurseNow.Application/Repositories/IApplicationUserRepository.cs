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
public interface IApplicationUserRepository
{
    void Add(ApplicationUser entity);
    void Remove(ApplicationUser entity);
    Task<ApplicationUser?> GetProfileAsync(string? userId, CancellationToken cancellationToken);
    Task<List<AdminUserItem>> SearchUsersAsync(string type, string? search, string? status, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetDetailsAsync(string userId, CancellationToken cancellationToken);
    Task<int> CountNursesAsync(CancellationToken cancellationToken);
    Task<int> CountPatientsAsync(CancellationToken cancellationToken);
    Task<List<AdminAccountItem>> ListAdminsAsync(CancellationToken cancellationToken);
    Task<List<NotificationRecipient>> ListRecipientsAsync(NurseNow.Application.Features.Admin.SendAdminNotificationDto dto, CancellationToken cancellationToken);
}
