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
public sealed class ApplicationUserRepository(ApplicationDbContext context) : IApplicationUserRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(ApplicationUser entity) => _context.Users.Add(entity);
    public void Remove(ApplicationUser entity) => _context.Users.Remove(entity);
    public async Task<ApplicationUser?> GetProfileAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<List<AdminUserItem>> SearchUsersAsync(string type, string? search, string? status, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();
        if (type == "Nurse")
        {
            query = query
            .Where(u => u.RoleType == "Nurse" && _context.NurseProfiles
            .AsNoTracking().Any(n => n.UserId == u.Id && n.VerificationStatus == "Approved"));
        }
        else
        {
            query = query.Where(u => u.RoleType == "Patient");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.FullName.Contains(search) || (u.Email != null && u.Email.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            query = query.Where(u => u.AccountStatus == status);
        }

        var users = await query.OrderByDescending(u => u.CreatedAt).Select(u => new AdminUserItem
        {
            userId = u.Id,
            name = u.FullName,
            email = u.Email,
            phone = u.PhoneNumber,
            joinDate = u.CreatedAt,
            status = u.AccountStatus,
            roleType = u.RoleType
        }).ToListAsync(cancellationToken);
        return users;
    }

    public async Task<ApplicationUser?> GetDetailsAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<int> CountNursesAsync(CancellationToken cancellationToken)
    {
        return await _context.Users.AsNoTracking().CountAsync(u => u.RoleType == "Nurse", cancellationToken);
    }

    public async Task<int> CountPatientsAsync(CancellationToken cancellationToken)
    {
        return await _context.Users.AsNoTracking().CountAsync(u => u.RoleType == "Patient", cancellationToken);
    }

    public async Task<List<AdminAccountItem>> ListAdminsAsync(CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.RoleType == "Administrator")
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminAccountItem
            {
                adminId = u.Id,
                fullName = u.FullName,
                email = u.Email,
                createdDate = u.CreatedAt
            }).ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationRecipient>> ListRecipientsAsync(NurseNow.Application.Features.Admin.SendAdminNotificationDto dto, CancellationToken cancellationToken)
    {
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

        var users = await usersQuery.Select(u => new NotificationRecipient
        {
            Id = u.Id
        }).ToListAsync(cancellationToken);
        return users;
    }
}
