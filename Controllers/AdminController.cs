using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Models;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("nurse-details/{userId}")]
        public async Task<IActionResult> GetNurseDetails(string userId)
        {
            var nurseProfile = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse not found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                nurseProfile.UserId,
                nurseProfile.User.FullName,
                nurseProfile.User.Email,

                nurseProfile.Specialization,
                nurseProfile.ExperienceYears,
                nurseProfile.PhoneNumber,
                nurseProfile.Address,
                nurseProfile.Location,
                nurseProfile.NationalId,
                nurseProfile.VerificationStatus,

                ProfileImageUrl = nurseProfile.ProfileImagePath != null
                    ? $"{baseUrl}/{nurseProfile.ProfileImagePath}"
                    : null,

                CertificateUrl = nurseProfile.CertificatePath != null
                    ? $"{baseUrl}/{nurseProfile.CertificatePath}"
                    : null,

                NationalIdImageUrl = nurseProfile.NationalIdImagePath != null
                    ? $"{baseUrl}/{nurseProfile.NationalIdImagePath}"
                    : null
            });
        }

        [HttpPut("verify-nurse/{userId}")]
        public async Task<IActionResult> UpdateNurseStatus(string userId, [FromBody] UpdateNurseStatusDto model)
        {
            var nurseProfile = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found");

            model.Status = model.Status?.Trim();

            if (model.Status != "Approved" && model.Status != "Rejected")
                return BadRequest("Invalid status");

            if (model.Status == "Approved")
            {
                if (string.IsNullOrEmpty(nurseProfile.Specialization) ||
                    nurseProfile.ExperienceYears == null ||
                    string.IsNullOrEmpty(nurseProfile.PhoneNumber) ||
                    string.IsNullOrEmpty(nurseProfile.Address) ||
                    string.IsNullOrEmpty(nurseProfile.Location) ||
                    string.IsNullOrEmpty(nurseProfile.NationalId) ||
                    string.IsNullOrEmpty(nurseProfile.NationalIdImagePath) ||
                    string.IsNullOrEmpty(nurseProfile.CertificatePath) ||
                    string.IsNullOrEmpty(nurseProfile.LicenseNumber))
                {
                    return BadRequest("Nurse profile is incomplete. Cannot approve.");
                }
            }

            nurseProfile.VerificationStatus = model.Status;

            var notificationMessage = model.Status == "Approved"
                ? "Your account has been approved."
                : "Your account has been rejected.";
            _context.Notifications.Add(new Notification
            {
                UserId = nurseProfile.UserId,
                Title = model.Status == "Approved" ? "Account Approved" : "Account Rejected",
                Message = notificationMessage,
                Type = "Account",
                CreatedAt = DateTime.UtcNow
            });

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = model.Status == "Approved"
                    ? "Nurse verification approved"
                    : "Nurse verification rejected",
                Description = $"{nurseProfile.User.FullName} verification was updated to {model.Status}.",
                ActivityType = "Verification",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok($"Nurse status updated to {model.Status}");
        }

        [HttpGet("pending-nurses")]
        public async Task<IActionResult> GetPendingNurses()
        {
            var pendingNurses = await _context.NurseProfiles
                .Include(n => n.User)
                .Where(n => n.VerificationStatus == "Pending")
                .Select(n => new
                {
                    n.UserId,
                    n.Specialization,
                    n.ExperienceYears,
                    n.VerificationStatus,
                    FullName = n.User.FullName,
                    Email = n.User.Email
                })
                .ToListAsync();

            return Ok(pendingNurses);
        }


        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var today = DateTime.Today;

            var totalPatients = await _context.Users
                .CountAsync(u => u.RoleType == "Patient");

            var totalNurses = await _context.NurseProfiles
                .CountAsync(n => n.VerificationStatus == "Approved");

            var pendingVerifications = await _context.NurseProfiles
                .CountAsync(n => n.VerificationStatus == "Pending");

            var todaysRequests = await _context.Bookings
                .CountAsync(b => b.BookingDate.Date == today);

            return Ok(new
            {
                totalPatients,
                totalNurses,
                pendingVerifications,
                todaysRequests
            });
        }



        [HttpGet("request-status-distribution")]
        public async Task<IActionResult> GetRequestStatusDistribution()
        {
            var statusCounts = await _context.Bookings
                .GroupBy(b => b.Status)
                .Select(g => new
                {
                    status = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            var total = statusCounts.Sum(x => x.count);

            var result = statusCounts.Select(x => new
            {
                status = x.status,
                count = x.count,
                percentage = total > 0 ? Math.Round((x.count * 100.0) / total, 1) : 0
            });

            return Ok(result);
        }



        [HttpGet("weekly-activity")]
        public async Task<IActionResult> GetWeeklyActivity()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-6);

            var bookings = await _context.Bookings
                .Where(b => b.BookingDate.Date >= startDate && b.BookingDate.Date <= today)
                .ToListAsync();

            var result = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var currentDate = startDate.AddDays(i);

                    var requestsCount = bookings.Count(b => b.BookingDate.Date == currentDate.Date);
                    var completedCount = bookings.Count(b =>
                        b.BookingDate.Date == currentDate.Date &&
                        b.Status == "Completed");

                    return new
                    {
                        day = currentDate.ToString("ddd"),
                        date = currentDate.ToString("yyyy-MM-dd"),
                        requests = requestsCount,
                        completed = completedCount
                    };
                })
                .ToList();

            return Ok(result);
        }


        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 5)
        {
            if (limit <= 0)
                limit = 5;

            var activities = await _context.AdminActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .Select(a => new
                {
                    activityId = a.AdminActivityLogId,
                    title = a.Title,
                    description = a.Description,
                    activityType = a.ActivityType,
                    createdAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(activities);
        }


        [HttpGet("nurses")]
        public async Task<IActionResult> GetAllNurses(
    [FromQuery] string? search,
    [FromQuery] string? status)
        {
            var query = _context.NurseProfiles
                .Include(n => n.User)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(n => n.VerificationStatus == status);
            }

            // Search (name or email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower();

                query = query.Where(n =>
                    n.User.FullName.ToLower().Contains(keyword) ||
                    n.User.Email.ToLower().Contains(keyword));
            }

            var result = await query
                .OrderByDescending(n => n.User.CreatedAt)
                .Select(n => new
                {
                    nurseId = n.UserId,
                    name = n.User.FullName,
                    email = n.User.Email,
                    phone = n.PhoneNumber,
                    registrationDate = n.User.CreatedAt,
                    status = n.VerificationStatus
                })
                .ToListAsync();

            return Ok(result);
        }



        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
    [FromQuery] string? role,
    [FromQuery] string? search,
    [FromQuery] string? status)
        {
            var query = _context.Users.AsQueryable();

            // Filter by role
            if (!string.IsNullOrWhiteSpace(role) && role != "All")
            {
                query = query.Where(u => u.RoleType == role);
            }

            // Filter by account status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(u => u.AccountStatus == status);
            }

            // Search by name or email
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower();

                query = query.Where(u =>
                    u.FullName.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword));
            }

            var result = await query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    userId = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    phone = u.PhoneNumber,
                    joinDate = u.CreatedAt,
                    role = u.RoleType,
                    status = u.AccountStatus
                })
                .ToListAsync();

            return Ok(result);
        }



        [HttpPut("users/{userId}/status")]
        public async Task<IActionResult> UpdateUserStatus(string userId, [FromBody] UpdateUserStatusDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found.");

            if (model.Status != "Active" && model.Status != "Suspended")
                return BadRequest("Invalid status.");

            user.AccountStatus = model.Status;

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = model.Status == "Active" ? "User account activated" : "User account suspended",
                Description = $"{user.FullName} account status changed to {model.Status}.",
                ActivityType = "UserManagement",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"User status updated to {model.Status}."
            });
        }



        [HttpGet("service-requests")]
        public async Task<IActionResult> GetServiceRequests(
            [FromQuery] string? search,
            [FromQuery] string? status)
        {
            var query = _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(b => b.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower();

                query = query.Where(b =>
                    b.Patient.FullName.ToLower().Contains(keyword));
            }

            var result = await query
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new
                {
                    requestId = b.BookingId,
                    patientName = b.Patient.FullName,
                    assignedNurse = b.Nurse != null ? b.Nurse.FullName : "Unassigned",
                    serviceType = b.Service.ServiceCatalog.Name,
                    date = b.BookingDate.ToString("yyyy-MM-dd"),
                    time = b.StartTime.ToString(@"hh\:mm"),
                    status = b.Status
                })
                .ToListAsync();

            return Ok(result);
        }


        [HttpGet("service-requests/{bookingId}")]
        public async Task<IActionResult> GetRequestDetails(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound("Request not found.");

            return Ok(new
            {
                bookingId = booking.BookingId,
                patientName = booking.Patient.FullName,
                nurseName = booking.Nurse != null ? booking.Nurse.FullName : null,
                serviceType = booking.Service.ServiceCatalog.Name,
                date = booking.BookingDate.ToString("yyyy-MM-dd"),
                time = booking.StartTime.ToString(@"hh\:mm"),
                status = booking.Status,
                address = booking.ServiceAddress,
                notes = booking.AdditionalNotes
            });
        }


        [HttpPut("service-requests/{bookingId}/assign")]
        public async Task<IActionResult> AssignNurse(int bookingId, [FromBody] AssignNurseDto model)
        {
            var booking = await _context.Bookings
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound("Request not found");

            var nurse = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.NurseId);

            if (nurse == null || nurse.RoleType != "Nurse")
                return BadRequest("Invalid nurse");

            booking.NurseId = model.NurseId;
            booking.Status = "Assigned";

            _context.Notifications.Add(new Notification
            {
                UserId = model.NurseId,
                Title = "New Assigned Request",
                Message = $"You have been assigned to a new service request for {booking.Service.ServiceCatalog.Name}.",
                Type = "Booking",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            });

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Nurse assigned to request",
                Description = $"{nurse.FullName} was assigned to booking #{booking.BookingId}.",
                ActivityType = "Booking",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Nurse assigned successfully"
            });
        }


        [HttpPut("service-requests/{bookingId}/cancel")]
        public async Task<IActionResult> CancelRequest(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound("Request not found");

            if (booking.Status == "Completed" || booking.Status == "Cancelled")
                return BadRequest("This request cannot be cancelled.");

            booking.Status = "Cancelled";

            _context.Notifications.Add(new Notification
            {
                UserId = booking.PatientId,
                Title = "Service Request Cancelled",
                Message = $"Your service request for {booking.Service.ServiceCatalog.Name} was cancelled by admin.",
                Type = "Booking",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            });

            if (!string.IsNullOrEmpty(booking.NurseId))
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = booking.NurseId,
                    Title = "Assigned Request Cancelled",
                    Message = $"A service request assigned to you for {booking.Service.ServiceCatalog.Name} was cancelled by admin.",
                    Type = "Booking",
                    BookingId = booking.BookingId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "Service request cancelled",
                Description = $"Booking #{booking.BookingId} was cancelled by admin.",
                ActivityType = "Booking",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Request cancelled"
            });
        }






























    }
}