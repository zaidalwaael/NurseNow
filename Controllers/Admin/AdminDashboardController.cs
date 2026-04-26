using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-6);

            var totalPatients = await _context.Users
                .CountAsync(u => u.RoleType == "Patient");

            var totalNurses = await _context.Users
                .CountAsync(u => u.RoleType == "Nurse");

            var pendingVerifications = await _context.NurseProfiles
                .CountAsync(n => n.VerificationStatus == "Pending");

            var todaysRequests = await _context.Bookings
                .CountAsync(b => b.CreatedAt.Date == today);

            var requestStatusDistribution = await _context.Bookings
                .GroupBy(b => b.Status)
                .Select(g => new
                {
                    status = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            var weeklyActivity = await _context.Bookings
                .Where(b => b.CreatedAt.Date >= weekStart && b.CreatedAt.Date <= today)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new
                {
                    date = g.Key,
                    requests = g.Count(),
                    completed = g.Count(b => b.Status == "Completed")
                })
                .OrderBy(x => x.date)
                .ToListAsync();

            var recentActivity = await _context.AdminActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new
                {
                    title = a.Title,
                    description = a.Description,
                    activityType = a.ActivityType,
                    createdAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                totalPatients,
                totalNurses,
                pendingVerifications,
                todaysRequests,
                requestStatusDistribution,
                weeklyActivity,
                recentActivity
            });
        }
    }
}