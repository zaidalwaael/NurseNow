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
public interface IServiceRepository
{
    void Add(NurseNow.Models.Service entity);
    void Remove(NurseNow.Models.Service entity);
    Task<List<string>> ListServiceNamesAsync(string nurseId, CancellationToken cancellationToken);
    Task<List<NurseServiceItem>> ListPublicServicesAsync(string nurseId, CancellationToken cancellationToken);
    Task<NurseNow.Models.Service?> GetForNurseAsync(string nurseId, int serviceId, CancellationToken cancellationToken);
    Task<NurseNow.Models.Service?> GetBookingServiceAsync(CreateBookingRequestDto model, CancellationToken cancellationToken);
    Task<List<NurseProfileServiceItem>> ListProfileServicesAsync(string? userId, CancellationToken cancellationToken);
    Task<bool> CatalogServiceExistsAsync(SaveNurseServiceDto model, string? userId, CancellationToken cancellationToken);
    Task<NurseNow.Models.Service?> GetOwnedForUpdateAsync(int id, string? userId, CancellationToken cancellationToken);
}
