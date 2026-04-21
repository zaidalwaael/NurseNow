using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // SUMMARY
        // =========================================
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var totalPatients = await _context.Users
                .Where(u => u.RoleType == "Patient")
                .CountAsync();

            var totalNurses = await _context.Users
                .Where(u => u.RoleType == "Nurse")
                .CountAsync();

            var pendingVerifications = await _context.Users
                .Where(u => u.RoleType == "Nurse" && u.AccountStatus == "Pending")
                .CountAsync();

            var bookings = await _context.Bookings.ToListAsync();

            var todaysRequests = bookings.Count(b =>
                b.BookingDate >= today && b.BookingDate < tomorrow);

            return Ok(new
            {
                totalPatients,
                totalNurses,
                pendingVerifications,
                todaysRequests
            });
        }

        // =========================================
        // REQUEST STATUS DISTRIBUTION
        // =========================================
        [HttpGet("request-status-distribution")]
        public async Task<IActionResult> GetRequestStatusDistribution()
        {
            var data = await _context.Bookings
                .GroupBy(b => b.Status)
                .Select(g => new
                {
                    status = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        // =========================================
        // WEEKLY ACTIVITY
        // =========================================
        [HttpGet("weekly-activity")]
        public async Task<IActionResult> GetWeeklyActivity()
        {
            var last7Days = DateTime.UtcNow.Date.AddDays(-6);

            var bookings = await _context.Bookings
                .Where(b => b.BookingDate >= last7Days)
                .ToListAsync();

            var data = bookings
                .GroupBy(b => b.BookingDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    day = g.Key.ToString("ddd"),
                    requests = g.Count(),
                    completed = g.Count(x => x.Status == "Completed")
                })
                .ToList();

            return Ok(data);
        }

        // =========================================
        // RECENT ACTIVITY
        // =========================================
        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity(int limit = 5)
        {
            var data = await _context.AdminActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return Ok(data);
        }
    }
}