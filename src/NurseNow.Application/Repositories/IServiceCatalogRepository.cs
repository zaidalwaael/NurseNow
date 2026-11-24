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
public interface IServiceCatalogRepository
{
    void Add(ServiceCatalog entity);
    void Remove(ServiceCatalog entity);
    Task<List<ServiceCatalogItem>> ListAsync(CancellationToken cancellationToken);
    Task<ServiceCatalog?> FindCatalogForServiceAsync(SaveNurseServiceDto model, CancellationToken cancellationToken);
}
