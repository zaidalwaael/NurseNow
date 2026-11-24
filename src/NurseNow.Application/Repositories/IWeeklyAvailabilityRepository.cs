using NurseNow.Application.DTOs.Persistence;
using NurseNow.Models;
using NurseNow.DTOs;
using NurseNow.DTOs.Nurse;
using NurseNow.DTOs.Patient;
using NurseNow.Application.Features.Admin;

namespace NurseNow.Application.Repositories;

/// <summary>Persistence operations; Add/Remove stage changes for IUnitOfWork.
/// Queries ending in ForUpdate (and verification/status update queries) return tracked entities.
/// Read projections are materialized inside Infrastructure.</summary>
public interface IWeeklyAvailabilityRepository
{
    void Add(WeeklyAvailability entity);
    void Remove(WeeklyAvailability entity);
    Task<bool> HasActiveScheduleAsync(string nurseId, CancellationToken cancellationToken);
    Task<bool> HasActiveDayAsync(string nurseId, string? todayDayName, CancellationToken cancellationToken);
    Task<List<string>> ListActiveDaysAsync(string nurseId, CancellationToken cancellationToken);
    Task<WeeklyAvailability?> GetActiveDayAsync(string nurseId, string? dayName, CancellationToken cancellationToken);
    Task<WeeklyAvailability?> GetBookingScheduleAsync(CreateBookingRequestDto model, string? dayName, CancellationToken cancellationToken);
    Task<List<WeeklyScheduleItem>> ListScheduleAsync(string? userId, CancellationToken cancellationToken);
    Task<bool> ActiveDayExistsAsync(AddWeeklyAvailabilityDto model, string? userId, CancellationToken cancellationToken);
    Task<WeeklyAvailability?> GetOwnedForUpdateAsync(int id, string? userId, CancellationToken cancellationToken);
    Task<DayScheduleDetails?> GetDayDetailsAsync(string? userId, string? dayOfWeek, CancellationToken cancellationToken);
}
