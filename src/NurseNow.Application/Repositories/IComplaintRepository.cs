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
public interface IComplaintRepository
{
    void Add(Complaint entity);
    void Remove(Complaint entity);
    Task<List<PatientIssueItemDto>> ListPatientIssuesAsync(string? patientId, CancellationToken cancellationToken);
    Task<PatientIssueDetailsDto?> GetPatientIssueAsync(int id, string? patientId, CancellationToken cancellationToken);
    Task<List<NurseProblemItemDto>> ListNurseProblemsAsync(string? nurseId, CancellationToken cancellationToken);
    Task<NurseProblemDetailsDto?> GetNurseProblemAsync(int id, string? nurseId, CancellationToken cancellationToken);
    Task<List<ComplaintListItem>> SearchComplaintsAsync(string? search, string? status, CancellationToken cancellationToken);
    Task<Complaint?> GetDetailsAsync(int id, CancellationToken cancellationToken);
    Task<Complaint?> GetForUpdateAsync(int id, CancellationToken cancellationToken);
}
