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
public sealed class ServiceCatalogRepository(ApplicationDbContext context) : IServiceCatalogRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(ServiceCatalog entity) => _context.ServiceCatalogs.Add(entity);
    public void Remove(ServiceCatalog entity) => _context.ServiceCatalogs.Remove(entity);
    public async Task<List<ServiceCatalogItem>> ListAsync(CancellationToken cancellationToken)
    {
        return await _context.ServiceCatalogs.AsNoTracking().Select(s => new ServiceCatalogItem
        {
            ServiceCatalogId = s.ServiceCatalogId,
            Name = s.Name,
            DefaultDurationInMinutes = s.DefaultDurationInMinutes
        }).ToListAsync(cancellationToken);
    }

    public async Task<ServiceCatalog?> FindCatalogForServiceAsync(SaveNurseServiceDto model, CancellationToken cancellationToken)
    {
        return await _context.ServiceCatalogs.FirstOrDefaultAsync(s => s.ServiceCatalogId == model.ServiceCatalogId, cancellationToken);
    }

}
