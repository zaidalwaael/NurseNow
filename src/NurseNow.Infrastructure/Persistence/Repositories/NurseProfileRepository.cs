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
public sealed class NurseProfileRepository(ApplicationDbContext context) : INurseProfileRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(NurseProfile entity) => _context.NurseProfiles.Add(entity);
    public void Remove(NurseProfile entity) => _context.NurseProfiles.Remove(entity);
    public async Task<NurseProfile?> GetWithUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles
            .AsNoTracking()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken);
    }

    public async Task<NurseProfile?> GetForStatusUpdateAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles.Include(n => n.User).FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken);
    }

    public async Task<List<PendingNurseSummary>> ListPendingSummariesAsync(CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles
            .AsNoTracking()
            .Include(n => n.User)
            .Where(n => n.VerificationStatus == "Pending")
            .Select(n => new PendingNurseSummary
            {
                UserId = n.UserId,
                Specialization = n.Specialization,
                ExperienceYears = n.ExperienceYears,
                VerificationStatus = n.VerificationStatus,
                FullName = n.User.FullName,
                Email = n.User.Email
            }).ToListAsync(cancellationToken);
    }

    public async Task<NurseProfile?> GetByUserAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken);
    }

    public async Task<(int totalCount, List<NurseSearchItem> nurses)> SearchApprovedAsync(BrowseNursesQueryDto query, string publicBaseUrl, CancellationToken cancellationToken)
    {
        var nursesQuery = _context.NurseProfiles
            .AsNoTracking()
            .Include(n => n.User)
            .Where(n => n.VerificationStatus == "Approved").AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            nursesQuery = nursesQuery
            .Where(n => n.User.FullName.ToLower().Contains(search) || (n.Specialization != null && n.Specialization.ToLower().Contains(search)) || (n.Location != null && n.Location.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var location = query.Location.Trim().ToLower();
            nursesQuery = nursesQuery.Where(n => n.Location != null && n.Location.ToLower() == location);
        }

        if (query.ServiceCatalogId.HasValue)
        {
            nursesQuery = nursesQuery
            .Where(n => _context.Services
            .AsNoTracking().Any(s => s.NurseId == n.UserId && s.ServiceCatalogId == query.ServiceCatalogId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            nursesQuery = nursesQuery.OrderBy(n => n.Address).ThenBy(n => n.UserId);
        }
        else
        {
            nursesQuery = nursesQuery.OrderByDescending(n => n.ExperienceYears).ThenBy(n => n.UserId);
        }

        var totalCount = await nursesQuery.CountAsync(cancellationToken);
        var baseUrl = publicBaseUrl;
        var nurses = await nursesQuery.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).Select(n => new NurseSearchItem
        {
            nurseId = n.UserId,
            fullName = n.User.FullName,
            specialization = n.Specialization,
            location = n.Location,
            address = n.Address,
            experienceYears = n.ExperienceYears,
            profileImageUrl = n.ProfileImagePath != null ? $"{baseUrl}/{n.ProfileImagePath}" : null,
            rating = Math.Round(_context.Reviews.AsNoTracking().Where(r => r.NurseId == n.UserId).Select(r => (double?)r.Rating).Average() ?? 0.0, 1),
            reviewsCount = _context.Reviews.AsNoTracking().Count(r => r.NurseId == n.UserId),
            price = query.ServiceCatalogId.HasValue ? _context.Services
            .AsNoTracking()
            .Where(s => s.NurseId == n.UserId && s.ServiceCatalogId == query.ServiceCatalogId.Value)
            .Select(s => (decimal?)s.Price).FirstOrDefault() : _context.Services
            .AsNoTracking()
            .Where(s => s.NurseId == n.UserId)
            .Select(s => (decimal?)s.Price).Min(),
            availabilityLabel = _context.WeeklyAvailabilities
            .AsNoTracking().Any(w => w.NurseId == n.UserId && w.IsActive) ? "Available This Week" : "Unavailable"
        }).ToListAsync(cancellationToken);
        return (totalCount, nurses);
    }

    public async Task<NurseProfile?> GetApprovedWithUserAsync(string nurseId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles
            .AsNoTracking()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved", cancellationToken);
    }

    public async Task<bool> IsApprovedAsync(string nurseId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles
            .AsNoTracking().AnyAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved", cancellationToken);
    }

    public async Task<bool> IsBookingNurseApprovedAsync(CreateBookingRequestDto model, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles.AnyAsync(n => n.UserId == model.NurseId && n.VerificationStatus == "Approved", cancellationToken);
    }

    public async Task<Dictionary<string, string?>> GetImagePathsAsync(List<string> nurseIds, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles
            .AsNoTracking()
            .Where(n => nurseIds.Contains(n.UserId))
            .ToDictionaryAsync(n => n.UserId, n => n.ProfileImagePath, cancellationToken);
    }

    public async Task<NurseProfile?> GetBookingNurseProfileAsync(Booking booking, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == booking.NurseId, cancellationToken);
    }

    public async Task<NurseProfile?> GetProfileForUpdateAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles.FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken);
    }

    public async Task<NurseProfile?> GetFullProfileAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles
            .AsNoTracking()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken);
    }

    public async Task<NurseProfile?> GetForUpdateAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles.Include(n => n.User).FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken);
    }

    public async Task<NurseAccountDetails?> GetAccountDetailsAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles.AsNoTracking().Where(n => n.UserId == userId).Select(n => new NurseAccountDetails
        {
            PhoneNumber = n.PhoneNumber,
            Location = n.Location,
            Address = n.Address,
            Specialization = n.Specialization,
            ExperienceYears = n.ExperienceYears,
            LicenseNumber = n.LicenseNumber,
            VerificationStatus = n.VerificationStatus,
            Bio = n.Bio
        }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountPendingAsync(CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles.AsNoTracking().CountAsync(n => n.VerificationStatus == "Pending", cancellationToken);
    }

    public async Task<List<NurseVerificationItem>> SearchPendingAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _context.NurseProfiles.AsNoTracking().Include(n => n.User).Where(n => n.VerificationStatus == "Pending").AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(n => n.User.FullName.Contains(search) || (n.User.Email != null && n.User.Email.Contains(search)));
        }

        var nurses = await query.OrderByDescending(n => n.NurseProfileId).Select(n => new NurseVerificationItem
        {
            nurseProfileId = n.NurseProfileId,
            nurseId = n.UserId,
            name = n.User.FullName,
            email = n.User.Email,
            phone = n.PhoneNumber,
            registrationDate = n.User.CreatedAt,
            status = n.VerificationStatus
        }).ToListAsync(cancellationToken);
        return nurses;
    }

    public async Task<NurseProfile?> GetDetailsAsync(int nurseProfileId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles
            .AsNoTracking()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.NurseProfileId == nurseProfileId, cancellationToken);
    }

    public async Task<NurseProfile?> GetForVerificationAsync(int nurseProfileId, CancellationToken cancellationToken)
    {
        return await _context.NurseProfiles
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.NurseProfileId == nurseProfileId, cancellationToken);
    }

}
