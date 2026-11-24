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
public sealed class AdminActivityLogRepository(ApplicationDbContext context) : IAdminActivityLogRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(AdminActivityLog entity) => _context.AdminActivityLogs.Add(entity);
    public void Remove(AdminActivityLog entity) => _context.AdminActivityLogs.Remove(entity);
    public async Task<List<RecentActivityItem>> ListRecentAsync(CancellationToken cancellationToken)
    {
        return await _context.AdminActivityLogs
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new RecentActivityItem
            {
                title = a.Title,
                description = a.Description,
                activityType = a.ActivityType,
                createdAt = a.CreatedAt
            }).ToListAsync(cancellationToken);
    }
}
