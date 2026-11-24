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
public sealed class ComplaintRepository(ApplicationDbContext context) : IComplaintRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(Complaint entity) => _context.Complaints.Add(entity);
    public void Remove(Complaint entity) => _context.Complaints.Remove(entity);
    public async Task<List<PatientIssueItemDto>> ListPatientIssuesAsync(string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Complaints
            .AsNoTracking()
            .Where(c => c.UserId == patientId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new PatientIssueItemDto
            {
                ComplaintId = c.ComplaintId,
                Category = c.Category,
                Subject = c.Subject,
                Status = c.Status,
                IsUrgent = c.IsUrgent,
                CreatedAt = c.CreatedAt
            }).ToListAsync(cancellationToken);
    }

    public async Task<PatientIssueDetailsDto?> GetPatientIssueAsync(int id, string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Complaints
            .AsNoTracking()
            .Where(c => c.ComplaintId == id && c.UserId == patientId)
            .Select(c => new PatientIssueDetailsDto
            {
                ComplaintId = c.ComplaintId,
                Category = c.Category,
                Subject = c.Subject,
                Description = c.Description,
                IsUrgent = c.IsUrgent,
                Status = c.Status,
                AdminResponse = c.AdminResponse,
                CreatedAt = c.CreatedAt,
                RespondedAt = c.RespondedAt
            }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<NurseProblemItemDto>> ListNurseProblemsAsync(string? nurseId, CancellationToken cancellationToken)
    {
        return await _context.Complaints
            .AsNoTracking()
            .Where(c => c.UserId == nurseId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new NurseProblemItemDto
            {
                ComplaintId = c.ComplaintId,
                Category = c.Category,
                Subject = c.Subject,
                Status = c.Status,
                IsUrgent = c.IsUrgent,
                CreatedAt = c.CreatedAt
            }).ToListAsync(cancellationToken);
    }

    public async Task<NurseProblemDetailsDto?> GetNurseProblemAsync(int id, string? nurseId, CancellationToken cancellationToken)
    {
        return await _context.Complaints
            .AsNoTracking()
            .Where(c => c.ComplaintId == id && c.UserId == nurseId)
            .Select(c => new NurseProblemDetailsDto
            {
                ComplaintId = c.ComplaintId,
                Category = c.Category,
                Subject = c.Subject,
                Description = c.Description,
                IsUrgent = c.IsUrgent,
                Status = c.Status,
                AdminResponse = c.AdminResponse,
                CreatedAt = c.CreatedAt,
                RespondedAt = c.RespondedAt
            }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ComplaintListItem>> SearchComplaintsAsync(string? search, string? status, CancellationToken cancellationToken)
    {
        var query = _context.Complaints.AsNoTracking().Include(c => c.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query
            .Where(c => c.User.FullName.Contains(search) || (c.User.Email != null && c.User.Email.Contains(search)) || c.Subject.Contains(search) || c.Category.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            query = query.Where(c => c.Status == status);
        }

        var complaints = await query.OrderByDescending(c => c.CreatedAt).Select(c => new ComplaintListItem
        {
            complaintId = c.ComplaintId,
            submittedBy = c.User.FullName,
            userRole = c.User.RoleType,
            category = c.Category,
            subject = c.Subject,
            date = c.CreatedAt,
            status = c.Status,
            isUrgent = c.IsUrgent
        }).ToListAsync(cancellationToken);
        return complaints;
    }

    public async Task<Complaint?> GetDetailsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Complaints
            .AsNoTracking()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.ComplaintId == id, cancellationToken);
    }

    public async Task<Complaint?> GetForUpdateAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Complaints.Include(c => c.User).FirstOrDefaultAsync(c => c.ComplaintId == id, cancellationToken);
    }

}
