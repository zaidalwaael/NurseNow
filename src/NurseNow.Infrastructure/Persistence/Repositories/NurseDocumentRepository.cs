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
public sealed class NurseDocumentRepository(ApplicationDbContext context) : INurseDocumentRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(NurseDocument entity) => _context.NurseDocuments.Add(entity);
    public void Remove(NurseDocument entity) => _context.NurseDocuments.Remove(entity);
    public async Task<List<NurseDocumentItem>> ListForNurseAsync(NurseProfile nurse, CancellationToken cancellationToken)
    {
        return await _context.NurseDocuments.AsNoTracking().Where(d => d.NurseId == nurse.UserId).Select(d => new NurseDocumentItem
        {
            id = d.Id,
            documentName = d.DocumentName,
            fileUrl = d.FileUrl
        }).ToListAsync(cancellationToken);
    }
    public async Task<StoredFileOwner?> FindFileOwnerAsync(string path, string legacyPath, CancellationToken cancellationToken)
    {
        var profile = await _context.NurseProfiles.AsNoTracking()

            .Where(n => n.ProfileImagePath == path || n.CertificatePath == path || n.NationalIdImagePath == path || n.ProfileImagePath == legacyPath || n.CertificatePath == legacyPath || n.NationalIdImagePath == legacyPath)
            .Select(n => new StoredFileOwner(n.UserId, n.ProfileImagePath == path || n.ProfileImagePath == legacyPath))
            .FirstOrDefaultAsync(cancellationToken);
        if (profile != null) return profile;
        return await _context.NurseDocuments.AsNoTracking()
            .Where(d => d.FileUrl == path || d.FileUrl == legacyPath)
            .Select(d => new StoredFileOwner(d.NurseId, false)).FirstOrDefaultAsync(cancellationToken);
    }
}
