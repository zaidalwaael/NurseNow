using NurseNow.Application.Repositories;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminDashboardService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IAdminActivityLogRepository _adminActivityLogs;
    private readonly IBookingRepository _bookings;
    private readonly INurseProfileRepository _nurseProfiles;
    private readonly IApplicationUserRepository _users;
    public AdminDashboardService(IAdminActivityLogRepository adminActivityLogs, IBookingRepository bookings, INurseProfileRepository nurseProfiles, IApplicationUserRepository users, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _adminActivityLogs = adminActivityLogs;
        _bookings = bookings;
        _nurseProfiles = nurseProfiles;
        _users = users;
    }

    public async Task<OperationResult> GetDashboardOverview()
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-6);
        var totalPatients = await _users.CountPatientsAsync(_requestContext.CancellationToken);
        var totalNurses = await _users.CountNursesAsync(_requestContext.CancellationToken);
        var pendingVerifications = await _nurseProfiles.CountPendingAsync(_requestContext.CancellationToken);
        var todaysRequests = await _bookings.CountCreatedOnAsync(today, _requestContext.CancellationToken);
        var requestStatusDistribution = await _bookings.GetStatusCountsAsync(_requestContext.CancellationToken);
        var weeklyActivity = await _bookings.GetWeeklyActivityAsync(today, weekStart, _requestContext.CancellationToken);
        var recentActivity = await _adminActivityLogs.ListRecentAsync(_requestContext.CancellationToken);
        return Ok(new { totalPatients, totalNurses, pendingVerifications, todaysRequests, requestStatusDistribution, weeklyActivity, recentActivity });
    }
}
