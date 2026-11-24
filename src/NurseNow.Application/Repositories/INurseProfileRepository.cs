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
public interface INurseProfileRepository
{
    void Add(NurseProfile entity);
    void Remove(NurseProfile entity);
    Task<NurseProfile?> GetWithUserAsync(string userId, CancellationToken cancellationToken);
    Task<NurseProfile?> GetForStatusUpdateAsync(string userId, CancellationToken cancellationToken);
    Task<List<PendingNurseSummary>> ListPendingSummariesAsync(CancellationToken cancellationToken);
    Task<NurseProfile?> GetByUserAsync(string? userId, CancellationToken cancellationToken);
    Task<(int totalCount, List<NurseSearchItem> nurses)> SearchApprovedAsync(BrowseNursesQueryDto query, string publicBaseUrl, CancellationToken cancellationToken);
    Task<NurseProfile?> GetApprovedWithUserAsync(string nurseId, CancellationToken cancellationToken);
    Task<bool> IsApprovedAsync(string nurseId, CancellationToken cancellationToken);
    Task<bool> IsBookingNurseApprovedAsync(CreateBookingRequestDto model, CancellationToken cancellationToken);
    Task<Dictionary<string, string?>> GetImagePathsAsync(List<string> nurseIds, CancellationToken cancellationToken);
    Task<NurseProfile?> GetBookingNurseProfileAsync(Booking booking, CancellationToken cancellationToken);
    Task<NurseProfile?> GetProfileForUpdateAsync(string? userId, CancellationToken cancellationToken);
    Task<NurseProfile?> GetFullProfileAsync(string? userId, CancellationToken cancellationToken);
    Task<NurseProfile?> GetForUpdateAsync(string? userId, CancellationToken cancellationToken);
    Task<NurseAccountDetails?> GetAccountDetailsAsync(string userId, CancellationToken cancellationToken);
    Task<int> CountPendingAsync(CancellationToken cancellationToken);
    Task<List<NurseVerificationItem>> SearchPendingAsync(string? search, CancellationToken cancellationToken);
    Task<NurseProfile?> GetDetailsAsync(int nurseProfileId, CancellationToken cancellationToken);
    Task<NurseProfile?> GetForVerificationAsync(int nurseProfileId, CancellationToken cancellationToken);
}
