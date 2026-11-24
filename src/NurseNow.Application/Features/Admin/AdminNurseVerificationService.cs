using NurseNow.Application.Repositories;
using NurseNow.Models;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminNurseVerificationService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INurseProfileRepository _nurseProfiles;
    private readonly INurseDocumentRepository _nurseDocuments;
    private readonly IAdminActivityLogRepository _adminActivityLogs;
    public AdminNurseVerificationService(IUnitOfWork unitOfWork, INurseProfileRepository nurseProfiles, INurseDocumentRepository nurseDocuments, IAdminActivityLogRepository adminActivityLogs, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _nurseProfiles = nurseProfiles;
        _nurseDocuments = nurseDocuments;
        _adminActivityLogs = adminActivityLogs;
    }

    public async Task<OperationResult> GetPendingNurses(string? search)
    {
        var nurses = await _nurseProfiles.SearchPendingAsync(search, _requestContext.CancellationToken);
        return Ok(nurses);
    }

    public async Task<OperationResult> GetNurseDetails(int nurseProfileId)
    {
        var nurse = await _nurseProfiles.GetDetailsAsync(nurseProfileId, _requestContext.CancellationToken);
        if (nurse == null)
            return NotFound(new { message = "Nurse not found" });
        var documents = await _nurseDocuments.ListForNurseAsync(nurse, _requestContext.CancellationToken);
        return Ok(new { nurseProfileId = nurse.NurseProfileId, nurseId = nurse.UserId, fullName = nurse.User.FullName, email = nurse.User.Email, phoneNumber = nurse.PhoneNumber, location = nurse.Location, address = nurse.Address, specialization = nurse.Specialization, experienceYears = nurse.ExperienceYears, licenseNumber = nurse.LicenseNumber, nationalId = nurse.NationalId, bio = nurse.Bio, status = nurse.VerificationStatus, rejectionReason = nurse.RejectionReason, registrationDate = nurse.User.CreatedAt, profileImagePath = nurse.ProfileImagePath, certificatePath = nurse.CertificatePath, nationalIdImagePath = nurse.NationalIdImagePath, documents });
    }

    public async Task<OperationResult> ApproveNurse(int nurseProfileId)
    {
        var nurse = await _nurseProfiles.GetForVerificationAsync(nurseProfileId, _requestContext.CancellationToken);
        if (nurse == null)
            return NotFound(new { message = "Nurse not found" });
        nurse.VerificationStatus = "Approved";
        nurse.RejectionReason = null;
        _adminActivityLogs.Add(new AdminActivityLog { Title = "Nurse verification approved", Description = $"{nurse.User.FullName} has been approved.", ActivityType = "Verification", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Nurse approved successfully" });
    }

    public async Task<OperationResult> RejectNurse(int nurseProfileId, RejectNurseDto dto)
    {
        var nurse = await _nurseProfiles.GetForVerificationAsync(nurseProfileId, _requestContext.CancellationToken);
        if (nurse == null)
            return NotFound(new { message = "Nurse not found" });
        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            return BadRequest(new { message = "Rejection reason is required" });
        nurse.VerificationStatus = "Rejected";
        nurse.RejectionReason = dto.RejectionReason;
        _adminActivityLogs.Add(new AdminActivityLog { Title = "Nurse verification rejected", Description = $"{nurse.User.FullName} has been rejected. Reason: {dto.RejectionReason}", ActivityType = "Verification", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Nurse rejected successfully" });
    }
}

public class RejectNurseDto
{
    public string RejectionReason { get; set; } = "";
}
