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
public interface INurseDocumentRepository
{
    Task<StoredFileOwner?> FindFileOwnerAsync(string path, string legacyPath, CancellationToken cancellationToken);
    void Add(NurseDocument entity);
    void Remove(NurseDocument entity);
    Task<List<NurseDocumentItem>> ListForNurseAsync(NurseProfile nurse, CancellationToken cancellationToken);
}

public sealed record StoredFileOwner(string UserId, bool IsProfileImage);
