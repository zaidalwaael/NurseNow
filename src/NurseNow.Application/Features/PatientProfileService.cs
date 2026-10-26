using Microsoft.EntityFrameworkCore;
using NurseNow.DTOs;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class PatientProfileService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public PatientProfileService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    private string? GetCurrentUserId()
    {
        return _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<OperationResult> GetPatientProfile()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var profile = await _context.PatientProfiles.AsNoTracking().Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == userId, _requestContext.CancellationToken);
        if (profile == null)
            return NotFound("Patient profile not found.");
        return Ok(new { fullName = profile.User.FullName, email = profile.User.Email, phoneNumber = profile.User.PhoneNumber, gender = profile.Gender, dateOfBirth = profile.DateOfBirth, bloodType = profile.BloodType, governorate = profile.Governorate, area = profile.Area, address = profile.Address, conditions = profile.Conditions, allergies = profile.Allergies, notes = profile.Notes });
    }

    public async Task<OperationResult> UpdatePersonalInfo(UpdatePatientPersonalInfoDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId, _requestContext.CancellationToken);
        if (profile == null)
            return NotFound("Patient profile not found.");
        profile.Gender = model.Gender;
        profile.DateOfBirth = model.DateOfBirth;
        profile.BloodType = model.BloodType;
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok("Personal info updated successfully.");
    }

    public async Task<OperationResult> UpdateAddress(UpdatePatientAddressDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId, _requestContext.CancellationToken);
        if (profile == null)
            return NotFound("Patient profile not found.");
        profile.Governorate = model.Governorate;
        profile.Area = model.Area;
        profile.Address = model.Address;
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok("Address updated successfully.");
    }

    public async Task<OperationResult> UpdateMedicalInfo(UpdatePatientMedicalInfoDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId, _requestContext.CancellationToken);
        if (profile == null)
            return NotFound("Patient profile not found.");
        profile.Conditions = model.Conditions;
        profile.Allergies = model.Allergies;
        profile.Notes = model.Notes;
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok("Medical info updated successfully.");
    }
}
