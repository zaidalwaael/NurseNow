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
public sealed class AvailabilityOverrideRepository(ApplicationDbContext context) : IAvailabilityOverrideRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(AvailabilityOverride entity) => _context.AvailabilityOverrides.Add(entity);
    public void Remove(AvailabilityOverride entity) => _context.AvailabilityOverrides.Remove(entity);
    public async Task<bool> IsBlockedAsync(string nurseId, DateTime today, CancellationToken cancellationToken)
    {
        return await _context.AvailabilityOverrides
            .AsNoTracking().AnyAsync(o => o.NurseId == nurseId && o.Date.Date == today.Date && o.IsBlocked, cancellationToken);
    }

    public async Task<bool> HasAvailableOverrideAsync(string nurseId, DateTime today, CancellationToken cancellationToken)
    {
        return await _context.AvailabilityOverrides
            .AsNoTracking().AnyAsync(o => o.NurseId == nurseId && o.Date.Date == today.Date && !o.IsBlocked, cancellationToken);
    }

    public async Task<List<AvailabilityOverride>> ListInRangeAsync(string nurseId, DateTime today, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _context.AvailabilityOverrides
            .AsNoTracking()
            .Where(o => o.NurseId == nurseId && o.Date >= today && o.Date < endDate)
            .OrderBy(o => o.AvailabilityOverrideId)
            .ToListAsync(cancellationToken);
    }

    public async Task<AvailabilityOverride?> GetForDateAsync(string nurseId, DateTime date, CancellationToken cancellationToken)
    {
        return await _context.AvailabilityOverrides
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.NurseId == nurseId && o.Date.Date == date.Date, cancellationToken);
    }

    public async Task<AvailabilityOverride?> GetBookingOverrideAsync(CreateBookingRequestDto model, CancellationToken cancellationToken)
    {
        return await _context.AvailabilityOverrides
            .FirstOrDefaultAsync(o => o.NurseId == model.NurseId && o.Date.Date == model.Date.Date, cancellationToken);
    }

    public async Task<DayOverrideDetails?> GetDayDetailsAsync(DateTime date, string? userId, CancellationToken cancellationToken)
    {
        return await _context.AvailabilityOverrides
            .AsNoTracking()
            .Where(o => o.NurseId == userId && o.Date.Date == date.Date)
            .Select(o => new DayOverrideDetails
            {
                AvailabilityOverrideId = o.AvailabilityOverrideId,
                date = o.Date.ToString("yyyy-MM-dd"),
                startTime = o.StartTime.HasValue ? o.StartTime.Value.ToString(@"hh\:mm") : null,
                endTime = o.EndTime.HasValue ? o.EndTime.Value.ToString(@"hh\:mm") : null,
                IsBlocked = o.IsBlocked
            }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AvailabilityOverride?> FindForOverrideAsync(OverrideDayAvailabilityDto model, string? userId, CancellationToken cancellationToken)
    {
        return await _context.AvailabilityOverrides
            .FirstOrDefaultAsync(o => o.NurseId == userId && o.Date.Date == model.Date.Date, cancellationToken);
    }

    public async Task<AvailabilityOverride?> FindForBlockAsync(BlockDayDto model, string? userId, CancellationToken cancellationToken)
    {
        return await _context.AvailabilityOverrides
            .FirstOrDefaultAsync(o => o.NurseId == userId && o.Date.Date == model.Date.Date, cancellationToken);
    }

    public async Task<AvailabilityOverride?> GetForUpdateAsync(DateTime date, string? userId, CancellationToken cancellationToken)
    {
        return await _context.AvailabilityOverrides
            .FirstOrDefaultAsync(o => o.NurseId == userId && o.Date.Date == date.Date, cancellationToken);
    }
}
