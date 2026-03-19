using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("nurses/browse")]
        public async Task<IActionResult> BrowseNurses([FromQuery] BrowseNursesQueryDto query)
        {
            var nursesQuery = _context.NurseProfiles
                .Include(n => n.User)
                .Where(n => n.VerificationStatus == "Approved")
                .AsQueryable();

            // Search: Name or Specialty or Location
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();

                nursesQuery = nursesQuery.Where(n =>
                    n.User.FullName.ToLower().Contains(search) ||
                    (n.Specialization != null && n.Specialization.ToLower().Contains(search)) ||
                    (n.Location != null && n.Location.ToLower().Contains(search))
                );
            }

            // Filter by location (Governorate)
            if (!string.IsNullOrWhiteSpace(query.Location))
            {
                var location = query.Location.Trim().ToLower();

                nursesQuery = nursesQuery.Where(n =>
                    n.Location != null && n.Location.ToLower() == location);
            }

            // Filter by service
            if (query.ServiceCatalogId.HasValue)
            {
                nursesQuery = nursesQuery.Where(n =>
                    _context.Services.Any(s =>
                        s.NurseId == n.UserId &&
                        s.ServiceCatalogId == query.ServiceCatalogId.Value));
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(query.Location))
            {
                nursesQuery = nursesQuery.OrderBy(n => n.Address);
            }
            else
            {
                // مؤقتًا لعدم وجود rating حقيقي بعد
                nursesQuery = nursesQuery.OrderByDescending(n => n.ExperienceYears);
            }
            var totalCount = await nursesQuery.CountAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var nurses = await nursesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(n => new
                {
                    nurseId = n.UserId,
                    fullName = n.User.FullName,
                    specialization = n.Specialization,
                    location = n.Location,
                    address = n.Address,
                    experienceYears = n.ExperienceYears,
                    profileImageUrl = n.ProfileImagePath != null
                        ? $"{baseUrl}/{n.ProfileImagePath}"
                        : null,

                    rating = 0.0,
                    reviewsCount = 0,

                    price = query.ServiceCatalogId.HasValue
                        ? _context.Services
                            .Where(s => s.NurseId == n.UserId && s.ServiceCatalogId == query.ServiceCatalogId.Value)
                            .Select(s => (decimal?)s.Price)
                            .FirstOrDefault()
                        : _context.Services
                            .Where(s => s.NurseId == n.UserId)
                            .Select(s => (decimal?)s.Price)
                            .Min(),

                    availabilityLabel = _context.WeeklyAvailabilities.Any(w =>
                        w.NurseId == n.UserId && w.IsActive)
                        ? "Available This Week"
                        : "Unavailable"
                })
                .ToListAsync();

            return Ok(new
            {
                pageNumber = query.PageNumber,
                pageSize = query.PageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                hasNextPage = query.PageNumber * query.PageSize < totalCount,
                items = nurses
            });
        }






    }
}