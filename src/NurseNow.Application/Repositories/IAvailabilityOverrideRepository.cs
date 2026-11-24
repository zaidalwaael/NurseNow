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
public interface IAvailabilityOverrideRepository
{
    void Add(AvailabilityOverride entity);
    void Remove(AvailabilityOverride entity);
    Task<bool> IsBlockedAsync(string nurseId, DateTime today, CancellationToken cancellationToken);
    Task<bool> HasAvailableOverrideAsync(string nurseId, DateTime today, CancellationToken cancellationToken);
    Task<List<AvailabilityOverride>> ListInRangeAsync(string nurseId, DateTime today, DateTime endDate, CancellationToken cancellationToken);
    Task<AvailabilityOverride?> GetForDateAsync(string nurseId, DateTime date, CancellationToken cancellationToken);
    Task<AvailabilityOverride?> GetBookingOverrideAsync(CreateBookingRequestDto model, CancellationToken cancellationToken);
    Task<DayOverrideDetails?> GetDayDetailsAsync(DateTime date, string? userId, CancellationToken cancellationToken);
    Task<AvailabilityOverride?> FindForOverrideAsync(OverrideDayAvailabilityDto model, string? userId, CancellationToken cancellationToken);
    Task<AvailabilityOverride?> FindForBlockAsync(BlockDayDto model, string? userId, CancellationToken cancellationToken);
    Task<AvailabilityOverride?> GetForUpdateAsync(DateTime date, string? userId, CancellationToken cancellationToken);
}
