using NurseNow.Application.Repositories;
using NurseNow.Models;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class AccountService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly INurseProfileRepository _nurseProfiles;
    private readonly IApplicationUserRepository _users;
    private readonly IIdentityService _identityService;
    public AccountService(INurseProfileRepository nurseProfiles, IApplicationUserRepository users, IIdentityService identityService, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _nurseProfiles = nurseProfiles;
        _users = users;
        _identityService = identityService;
    }

    public async Task<OperationResult> GetCurrentUser()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();
        var user = await _users.GetProfileAsync(userId, _requestContext.CancellationToken);
        if (user == null)
            return NotFound();
        var roles = await _identityService.GetRolesAsync(user);
        // 🔵 إذا Nurse
        if (roles.Contains("Nurse"))
        {
            var nurseProfile = await _nurseProfiles.GetByUserAsync(userId, _requestContext.CancellationToken);
            return Ok(new { userId = user.Id, fullName = user.FullName, email = user.Email, role = "Nurse", verificationStatus = nurseProfile?.VerificationStatus, specialization = nurseProfile?.Specialization, experienceYears = nurseProfile?.ExperienceYears, phoneNumber = nurseProfile?.PhoneNumber, address = nurseProfile?.Address, location = nurseProfile?.Location, nationalId = nurseProfile?.NationalId, profileImagePath = nurseProfile?.ProfileImagePath, certificatePath = nurseProfile?.CertificatePath });
        }

        // 🟣 Admin أو Patient
        return Ok(new { userId = user.Id, fullName = user.FullName, email = user.Email, role = roles.FirstOrDefault() });
    }
}
