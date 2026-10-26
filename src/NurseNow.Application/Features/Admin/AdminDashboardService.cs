using Microsoft.EntityFrameworkCore;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminDashboardService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public AdminDashboardService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    public async Task<OperationResult> GetDashboardOverview()
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-6);
        var totalPatients = await _context.Users.AsNoTracking().CountAsync(u => u.RoleType == "Patient", _requestContext.CancellationToken);
        var totalNurses = await _context.Users.AsNoTracking().CountAsync(u => u.RoleType == "Nurse", _requestContext.CancellationToken);
        var pendingVerifications = await _context.NurseProfiles.AsNoTracking().CountAsync(n => n.VerificationStatus == "Pending", _requestContext.CancellationToken);
        var todaysRequests = await _context.Bookings.AsNoTracking().CountAsync(b => b.CreatedAt >= today && b.CreatedAt < today.AddDays(1), _requestContext.CancellationToken);
        var requestStatusDistribution = await _context.Bookings.AsNoTracking().GroupBy(b => b.Status).Select(g => new { status = g.Key, count = g.Count() }).ToListAsync(_requestContext.CancellationToken);
        var weeklyActivity = await _context.Bookings.AsNoTracking().Where(b => b.CreatedAt >= weekStart && b.CreatedAt < today.AddDays(1)).GroupBy(b => b.CreatedAt.Date).Select(g => new { date = g.Key, requests = g.Count(), completed = g.Count(b => b.Status == "Completed") }).OrderBy(x => x.date).ToListAsync(_requestContext.CancellationToken);
        var recentActivity = await _context.AdminActivityLogs.AsNoTracking().OrderByDescending(a => a.CreatedAt).Take(5).Select(a => new { title = a.Title, description = a.Description, activityType = a.ActivityType, createdAt = a.CreatedAt }).ToListAsync(_requestContext.CancellationToken);
        return Ok(new { totalPatients, totalNurses, pendingVerifications, todaysRequests, requestStatusDistribution, weeklyActivity, recentActivity });
    }
}
