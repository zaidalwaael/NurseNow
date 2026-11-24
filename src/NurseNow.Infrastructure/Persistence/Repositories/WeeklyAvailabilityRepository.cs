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
public sealed class WeeklyAvailabilityRepository(ApplicationDbContext context) : IWeeklyAvailabilityRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(WeeklyAvailability entity) => _context.WeeklyAvailabilities.Add(entity);
    public void Remove(WeeklyAvailability entity) => _context.WeeklyAvailabilities.Remove(entity);
    public async Task<bool> HasActiveScheduleAsync(string nurseId, CancellationToken cancellationToken)
    {
        return await _context.WeeklyAvailabilities.AsNoTracking().AnyAsync(w => w.NurseId == nurseId && w.IsActive, cancellationToken);
    }

    public async Task<bool> HasActiveDayAsync(string nurseId, string? todayDayName, CancellationToken cancellationToken)
    {
        return await _context.WeeklyAvailabilities
            .AsNoTracking().AnyAsync(w => w.NurseId == nurseId && w.DayOfWeek == todayDayName && w.IsActive, cancellationToken);
    }

    public async Task<List<string>> ListActiveDaysAsync(string nurseId, CancellationToken cancellationToken)
    {
        return await _context.WeeklyAvailabilities
            .AsNoTracking()
            .Where(w => w.NurseId == nurseId && w.IsActive && w.StartTime < w.EndTime)
            .Select(w => w.DayOfWeek).Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<WeeklyAvailability?> GetActiveDayAsync(string nurseId, string? dayName, CancellationToken cancellationToken)
    {
        return await _context.WeeklyAvailabilities
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.NurseId == nurseId && w.DayOfWeek == dayName && w.IsActive, cancellationToken);
    }

    public async Task<WeeklyAvailability?> GetBookingScheduleAsync(CreateBookingRequestDto model, string? dayName, CancellationToken cancellationToken)
    {
        return await _context.WeeklyAvailabilities
            .FirstOrDefaultAsync(w => w.NurseId == model.NurseId && w.DayOfWeek == dayName && w.IsActive, cancellationToken);
    }

    public async Task<List<WeeklyScheduleItem>> ListScheduleAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.WeeklyAvailabilities
            .AsNoTracking()
            .Where(w => w.NurseId == userId)
            .OrderBy(w => w.WeeklyAvailabilityId)
            .Select(w => new WeeklyScheduleItem
            {
                WeeklyAvailabilityId = w.WeeklyAvailabilityId,
                DayOfWeek = w.DayOfWeek,
                startTime = w.StartTime.ToString(@"hh\:mm"),
                endTime = w.EndTime.ToString(@"hh\:mm"),
                IsActive = w.IsActive
            }).ToListAsync(cancellationToken);
    }

    public async Task<bool> ActiveDayExistsAsync(AddWeeklyAvailabilityDto model, string? userId, CancellationToken cancellationToken)
    {
        return await _context.WeeklyAvailabilities.AnyAsync(w => w.NurseId == userId && w.DayOfWeek == model.DayOfWeek && w.StartTime == model.StartTime && w.EndTime == model.EndTime, cancellationToken);
    }

    public async Task<WeeklyAvailability?> GetOwnedForUpdateAsync(int id, string? userId, CancellationToken cancellationToken)
    {
        return await _context.WeeklyAvailabilities
            .FirstOrDefaultAsync(w => w.WeeklyAvailabilityId == id && w.NurseId == userId, cancellationToken);
    }

    public async Task<DayScheduleDetails?> GetDayDetailsAsync(string? userId, string? dayOfWeek, CancellationToken cancellationToken)
    {
        return await _context.WeeklyAvailabilities
            .AsNoTracking()
            .Where(w => w.NurseId == userId && w.DayOfWeek == dayOfWeek && w.IsActive)
            .Select(w => new DayScheduleDetails
            {
                WeeklyAvailabilityId = w.WeeklyAvailabilityId,
                DayOfWeek = w.DayOfWeek,
                startTime = w.StartTime.ToString(@"hh\:mm"),
                endTime = w.EndTime.ToString(@"hh\:mm")
            }).FirstOrDefaultAsync(cancellationToken);
    }
}
