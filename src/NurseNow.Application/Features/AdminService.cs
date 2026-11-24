using NurseNow.Application.Repositories;
using NurseNow.DTOs;
using NurseNow.Models;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class AdminService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INurseProfileRepository _nurseProfiles;
    private readonly INotificationRepository _notifications;
    public AdminService(IUnitOfWork unitOfWork, INurseProfileRepository nurseProfiles, INotificationRepository notifications, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _nurseProfiles = nurseProfiles;
        _notifications = notifications;
    }

    public async Task<OperationResult> GetNurseDetails(string userId)
    {
        var nurseProfile = await _nurseProfiles.GetWithUserAsync(userId, _requestContext.CancellationToken);
        if (nurseProfile == null)
            return NotFound("Nurse not found");
        var baseUrl = _requestContext.PublicBaseUrl;
        return Ok(new { nurseProfile.UserId, nurseProfile.User.FullName, nurseProfile.User.Email, nurseProfile.Specialization, nurseProfile.ExperienceYears, nurseProfile.PhoneNumber, nurseProfile.Address, nurseProfile.Location, nurseProfile.NationalId, nurseProfile.VerificationStatus, ProfileImageUrl = nurseProfile.ProfileImagePath != null ? $"{baseUrl}/{nurseProfile.ProfileImagePath}" : null, CertificateUrl = nurseProfile.CertificatePath != null ? $"{baseUrl}/{nurseProfile.CertificatePath}" : null, NationalIdImageUrl = nurseProfile.NationalIdImagePath != null ? $"{baseUrl}/{nurseProfile.NationalIdImagePath}" : null });
    }

    public async Task<OperationResult> UpdateNurseStatus(string userId, UpdateNurseStatusDto model)
    {
        var nurseProfile = await _nurseProfiles.GetForStatusUpdateAsync(userId, _requestContext.CancellationToken);
        if (nurseProfile == null)
            return NotFound("Nurse profile not found");
        model.Status = model.Status?.Trim() ?? string.Empty;
        if (model.Status != "Approved" && model.Status != "Rejected")
            return BadRequest("Invalid status");
        if (model.Status == "Approved")
        {
            if (string.IsNullOrEmpty(nurseProfile.Specialization) || nurseProfile.ExperienceYears == null || string.IsNullOrEmpty(nurseProfile.PhoneNumber) || string.IsNullOrEmpty(nurseProfile.Address) || string.IsNullOrEmpty(nurseProfile.Location) || string.IsNullOrEmpty(nurseProfile.NationalId) || string.IsNullOrEmpty(nurseProfile.NationalIdImagePath) || string.IsNullOrEmpty(nurseProfile.CertificatePath) || string.IsNullOrEmpty(nurseProfile.LicenseNumber))
            {
                return BadRequest("Nurse profile is incomplete. Cannot approve.");
            }
        }

        nurseProfile.VerificationStatus = model.Status;
        var notificationMessage = model.Status == "Approved" ? "Your account has been approved." : "Your account has been rejected.";
        _notifications.Add(new Notification { UserId = nurseProfile.UserId, Title = model.Status == "Approved" ? "Account Approved" : "Account Rejected", Message = notificationMessage, Type = "Account", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok($"Nurse status updated to {model.Status}");
    }

    public async Task<OperationResult> GetPendingNurses()
    {
        var pendingNurses = await _nurseProfiles.ListPendingSummariesAsync(_requestContext.CancellationToken);
        return Ok(pendingNurses);
    }
}
