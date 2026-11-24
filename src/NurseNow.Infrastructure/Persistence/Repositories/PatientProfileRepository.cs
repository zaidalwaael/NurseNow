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
public sealed class PatientProfileRepository(ApplicationDbContext context) : IPatientProfileRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(PatientProfile entity) => _context.PatientProfiles.Add(entity);
    public void Remove(PatientProfile entity) => _context.PatientProfiles.Remove(entity);
    public async Task<PatientProfile?> GetWithUserAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.PatientProfiles
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<PatientProfile?> GetForUpdateAsync(string? userId, CancellationToken cancellationToken)
    {
        return await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<PatientAccountDetails?> GetAccountDetailsAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.PatientProfiles.AsNoTracking().Where(p => p.UserId == userId).Select(p => new PatientAccountDetails
        {
            PhoneNumber = p.PhoneNumber,
            Gender = p.Gender,
            DateOfBirth = p.DateOfBirth,
            BloodType = p.BloodType,
            Governorate = p.Governorate,
            Area = p.Area,
            Address = p.Address,
            Conditions = p.Conditions,
            Allergies = p.Allergies,
            Notes = p.Notes
        }).FirstOrDefaultAsync(cancellationToken);
    }
}
