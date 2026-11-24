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
public sealed class ServiceRepository(ApplicationDbContext context) : IServiceRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(NurseNow.Models.Service entity) => _context.Services.Add(entity);
    public void Remove(NurseNow.Models.Service entity) => _context.Services.Remove(entity);
    public async Task<List<string>> ListServiceNamesAsync(string nurseId, CancellationToken cancellationToken)
    {
        return await _context.Services
            .AsNoTracking()
            .Include(s => s.ServiceCatalog)
            .Where(s => s.NurseId == nurseId)
            .Select(s => s.ServiceCatalog.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NurseServiceItem>> ListPublicServicesAsync(string nurseId, CancellationToken cancellationToken)
    {
        return await _context.Services
            .AsNoTracking()
            .Include(s => s.ServiceCatalog)
            .Where(s => s.NurseId == nurseId)
            .Select(s => new NurseServiceItem
            {
                serviceId = s.ServiceId,
                serviceCatalogId = s.ServiceCatalogId,
                serviceName = s.ServiceCatalog.Name,
                durationInMinutes = s.ServiceCatalog.DefaultDurationInMinutes,
                price = s.Price
            }).ToListAsync(cancellationToken);
    }

    public async Task<NurseNow.Models.Service?> GetForNurseAsync(string nurseId, int serviceId, CancellationToken cancellationToken)
    {
        return await _context.Services
            .AsNoTracking()
            .Include(s => s.ServiceCatalog)
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.NurseId == nurseId, cancellationToken);
    }

    public async Task<NurseNow.Models.Service?> GetBookingServiceAsync(CreateBookingRequestDto model, CancellationToken cancellationToken)
    {
        return await _context.Services
            .Include(s => s.ServiceCatalog)
            .Include(s => s.Nurse)
            .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && s.NurseId == model.NurseId, cancellationToken);
    }

    public async Task<List<NurseProfileServiceItem>> ListProfileServicesAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.Services
            .AsNoTracking()
            .Include(s => s.ServiceCatalog)
            .Where(s => s.NurseId == userId)
            .Select(s => new NurseProfileServiceItem
            {
                ServiceId = s.ServiceId,
                ServiceCatalogId = s.ServiceCatalogId,
                serviceName = s.ServiceCatalog.Name,
                durationInMinutes = s.ServiceCatalog.DefaultDurationInMinutes,
                Price = s.Price
            }).ToListAsync(cancellationToken);
    }

    public async Task<bool> CatalogServiceExistsAsync(SaveNurseServiceDto model, string? userId, CancellationToken cancellationToken)
    {
        return await _context.Services.AnyAsync(s => s.NurseId == userId && s.ServiceCatalogId == model.ServiceCatalogId, cancellationToken);
    }

    public async Task<NurseNow.Models.Service?> GetOwnedForUpdateAsync(int id, string? userId, CancellationToken cancellationToken)
    {
        return await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == id && s.NurseId == userId, cancellationToken);
    }

}
