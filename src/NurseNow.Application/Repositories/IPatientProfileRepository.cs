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
public interface IPatientProfileRepository
{
    void Add(PatientProfile entity);
    void Remove(PatientProfile entity);
    Task<PatientProfile?> GetWithUserAsync(string? userId, CancellationToken cancellationToken);
    Task<PatientProfile?> GetForUpdateAsync(string? userId, CancellationToken cancellationToken);
    Task<PatientAccountDetails?> GetAccountDetailsAsync(string userId, CancellationToken cancellationToken);
}
