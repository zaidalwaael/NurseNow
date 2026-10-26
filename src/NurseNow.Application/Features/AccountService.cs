using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NurseNow.Models;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class AccountService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public AccountService(IApplicationDbContext context, UserManager<ApplicationUser> userManager, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
        _userManager = userManager;
    }

    public async Task<OperationResult> GetCurrentUser()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, _requestContext.CancellationToken);
        if (user == null)
            return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        // 🔵 إذا Nurse
        if (roles.Contains("Nurse"))
        {
            var nurseProfile = await _context.NurseProfiles.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == userId, _requestContext.CancellationToken);
            return Ok(new { userId = user.Id, fullName = user.FullName, email = user.Email, role = "Nurse", verificationStatus = nurseProfile?.VerificationStatus, specialization = nurseProfile?.Specialization, experienceYears = nurseProfile?.ExperienceYears, phoneNumber = nurseProfile?.PhoneNumber, address = nurseProfile?.Address, location = nurseProfile?.Location, nationalId = nurseProfile?.NationalId, profileImagePath = nurseProfile?.ProfileImagePath, certificatePath = nurseProfile?.CertificatePath });
        }

        // 🟣 Admin أو Patient
        return Ok(new { userId = user.Id, fullName = user.FullName, email = user.Email, role = roles.FirstOrDefault() });
    }
}
