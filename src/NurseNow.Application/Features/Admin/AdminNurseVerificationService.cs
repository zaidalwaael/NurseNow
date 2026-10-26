using Microsoft.EntityFrameworkCore;
using NurseNow.Models;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminNurseVerificationService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public AdminNurseVerificationService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    public async Task<OperationResult> GetPendingNurses(string? search)
    {
        var query = _context.NurseProfiles.AsNoTracking().Include(n => n.User).Where(n => n.VerificationStatus == "Pending").AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(n => n.User.FullName.Contains(search) || (n.User.Email != null && n.User.Email.Contains(search)));
        }

        var nurses = await query.OrderByDescending(n => n.NurseProfileId).Select(n => new { nurseProfileId = n.NurseProfileId, nurseId = n.UserId, name = n.User.FullName, email = n.User.Email, phone = n.PhoneNumber, registrationDate = n.User.CreatedAt, status = n.VerificationStatus }).ToListAsync(_requestContext.CancellationToken);
        return Ok(nurses);
    }

    public async Task<OperationResult> GetNurseDetails(int nurseProfileId)
    {
        var nurse = await _context.NurseProfiles.AsNoTracking().Include(n => n.User).FirstOrDefaultAsync(n => n.NurseProfileId == nurseProfileId, _requestContext.CancellationToken);
        if (nurse == null)
            return NotFound(new { message = "Nurse not found" });
        var documents = await _context.NurseDocuments.AsNoTracking().Where(d => d.NurseId == nurse.UserId).Select(d => new { id = d.Id, documentName = d.DocumentName, fileUrl = d.FileUrl }).ToListAsync(_requestContext.CancellationToken);
        return Ok(new { nurseProfileId = nurse.NurseProfileId, nurseId = nurse.UserId, fullName = nurse.User.FullName, email = nurse.User.Email, phoneNumber = nurse.PhoneNumber, location = nurse.Location, address = nurse.Address, specialization = nurse.Specialization, experienceYears = nurse.ExperienceYears, licenseNumber = nurse.LicenseNumber, nationalId = nurse.NationalId, bio = nurse.Bio, status = nurse.VerificationStatus, rejectionReason = nurse.RejectionReason, registrationDate = nurse.User.CreatedAt, profileImagePath = nurse.ProfileImagePath, certificatePath = nurse.CertificatePath, nationalIdImagePath = nurse.NationalIdImagePath, documents });
    }

    public async Task<OperationResult> ApproveNurse(int nurseProfileId)
    {
        var nurse = await _context.NurseProfiles.Include(n => n.User).FirstOrDefaultAsync(n => n.NurseProfileId == nurseProfileId, _requestContext.CancellationToken);
        if (nurse == null)
            return NotFound(new { message = "Nurse not found" });
        nurse.VerificationStatus = "Approved";
        nurse.RejectionReason = null;
        _context.AdminActivityLogs.Add(new AdminActivityLog { Title = "Nurse verification approved", Description = $"{nurse.User.FullName} has been approved.", ActivityType = "Verification", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Nurse approved successfully" });
    }

    public async Task<OperationResult> RejectNurse(int nurseProfileId, RejectNurseDto dto)
    {
        var nurse = await _context.NurseProfiles.Include(n => n.User).FirstOrDefaultAsync(n => n.NurseProfileId == nurseProfileId, _requestContext.CancellationToken);
        if (nurse == null)
            return NotFound(new { message = "Nurse not found" });
        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            return BadRequest(new { message = "Rejection reason is required" });
        nurse.VerificationStatus = "Rejected";
        nurse.RejectionReason = dto.RejectionReason;
        _context.AdminActivityLogs.Add(new AdminActivityLog { Title = "Nurse verification rejected", Description = $"{nurse.User.FullName} has been rejected. Reason: {dto.RejectionReason}", ActivityType = "Verification", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Nurse rejected successfully" });
    }
}

public class RejectNurseDto
{
    public string RejectionReason { get; set; } = "";
}
