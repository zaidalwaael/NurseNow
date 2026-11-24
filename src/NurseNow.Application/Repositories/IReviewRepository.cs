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
public interface IReviewRepository
{
    void Add(Review entity);
    void Remove(Review entity);
    Task<int> CountForNurseAsync(NurseProfile nurseProfile, CancellationToken cancellationToken);
    Task<Nullable<double>> GetAverageRatingAsync(NurseProfile nurseProfile, CancellationToken cancellationToken);
    Task<List<PatientReviewItem>> ListPublicReviewsAsync(string nurseId, CancellationToken cancellationToken);
    Task<List<PatientReviewItem>> ListForNurseAsync(string? nurseId, CancellationToken cancellationToken);
}
