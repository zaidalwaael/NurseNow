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
public sealed class ReviewRepository(ApplicationDbContext context) : IReviewRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(Review entity) => _context.Reviews.Add(entity);
    public void Remove(Review entity) => _context.Reviews.Remove(entity);
    public async Task<int> CountForNurseAsync(NurseProfile nurseProfile, CancellationToken cancellationToken)
    {
        return await _context.Reviews.AsNoTracking().CountAsync(r => r.NurseId == nurseProfile.UserId, cancellationToken);
    }

    public async Task<Nullable<double>> GetAverageRatingAsync(NurseProfile nurseProfile, CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.NurseId == nurseProfile.UserId)
            .Select(r => (double?)r.Rating).AverageAsync(cancellationToken);
    }

    public async Task<List<PatientReviewItem>> ListPublicReviewsAsync(string nurseId, CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Patient)
            .Where(r => r.NurseId == nurseId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new PatientReviewItem
            {
                reviewId = r.ReviewId,
                patientName = r.Patient.FullName,
                rating = r.Rating,
                comment = r.Comment,
                createdAt = r.CreatedAt
            }).ToListAsync(cancellationToken);
    }

    public async Task<List<PatientReviewItem>> ListForNurseAsync(string? nurseId, CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Patient)
            .Where(r => r.NurseId == nurseId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new PatientReviewItem
            {
                reviewId = r.ReviewId,
                patientName = r.Patient.FullName,
                rating = r.Rating,
                comment = r.Comment,
                createdAt = r.CreatedAt
            }).ToListAsync(cancellationToken);
    }
}
