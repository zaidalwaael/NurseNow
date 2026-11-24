using NurseNow.Application.Repositories;
using NurseNow.DTOs;
using NurseNow.Models;
using System.Text;
using NurseNow.Services;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class AuthService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IIdentityService _identityService;
    private readonly IAccessTokenGenerator _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INurseProfileRepository _nurseProfiles;
    private readonly IPatientProfileRepository _patientProfiles;
    private readonly IAdminActivityLogRepository _adminActivityLogs;
    private readonly IEmailService _emailService;
    public AuthService(IIdentityService identityService, IAccessTokenGenerator jwtService, IUnitOfWork unitOfWork, INurseProfileRepository nurseProfiles, IPatientProfileRepository patientProfiles, IAdminActivityLogRepository adminActivityLogs, IEmailService emailService, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _identityService = identityService;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _nurseProfiles = nurseProfiles;
        _patientProfiles = patientProfiles;
        _adminActivityLogs = adminActivityLogs;
        _emailService = emailService;
    }

    public async Task<OperationResult> Register(RegisterDto model)
    {
        if (model.Role is not ("Patient" or "Nurse"))
            return BadRequest("Role must be Patient or Nurse.");
        model.Email = model.Email.Trim();
        await using var transaction = await _unitOfWork.BeginTransactionAsync(_requestContext.CancellationToken);
        var userExists = await _identityService.FindByEmailAsync(model.Email);
        if (userExists != null)
            return BadRequest("User already exists");
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            RoleType = model.Role,
            EmailConfirmed = true
        };
        var result = await _identityService.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        var roleResult = await _identityService.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);
        if (model.Role == "Nurse")
        {
            var nurseProfile = new NurseProfile
            {
                UserId = user.Id,
                VerificationStatus = "Pending"
            };
            _nurseProfiles.Add(nurseProfile);
            _adminActivityLogs.Add(new AdminActivityLog { Title = "New nurse registration", Description = $"{user.FullName} registered as a nurse.", ActivityType = "Registration", CreatedAt = DateTime.UtcNow });
        }
        else if (model.Role == "Patient")
        {
            var patientProfile = new PatientProfile
            {
                UserId = user.Id
            };
            _patientProfiles.Add(patientProfile);
        }

        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        await transaction.CommitAsync(_requestContext.CancellationToken);
        return Ok("User registered successfully");
    }

    public async Task<OperationResult> Login(LoginDto model)
    {
        var user = await _identityService.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");
        if (user.AccountStatus != "Active" || (user.RoleType == "Administrator" && user.AdminApprovalStatus != "Active"))
            return Unauthorized("Your account is suspended");
        if (await _identityService.IsLockedOutAsync(user))
            return Unauthorized("Invalid credentials");
        var validPassword = await _identityService.CheckPasswordAsync(user, model.Password);
        if (!validPassword)
        {
            await _identityService.AccessFailedAsync(user);
            await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
            return Unauthorized("Invalid credentials");
        }

        await _identityService.ResetAccessFailedCountAsync(user);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        var roles = await _identityService.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);
        return Ok(new { token, user.FullName, user.Email, roles });
    }

    public async Task<OperationResult> ForgotPassword(ForgotPasswordDto model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Email))
            return BadRequest("Email is required.");
        var email = model.Email.Trim().ToLower();
        var user = await _identityService.FindByEmailAsync(email);
        // Do not disclose whether the email exists.
        return Ok(new { message = "Reset request received. The Home Nurse team will contact you soon." });
    }

    public async Task<OperationResult> ResetPassword(ResetPasswordDto model)
    {
        var user = await _identityService.FindByEmailAsync(model.Email);
        if (user == null)
            return BadRequest("Invalid request.");
        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(DecodeToken(model.Token));
        }
        catch
        {
            return BadRequest("Invalid token.");
        }

        var result = await _identityService.ResetPasswordAsync(user, decodedToken, model.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Password has been reset successfully." });
    }
    private static byte[] DecodeToken(string token)
    {
        var value = token.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(value.PadRight((value.Length + 3) / 4 * 4, '='));
    }
}
