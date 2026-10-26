using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NurseNow.DTOs;
using NurseNow.Models;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using NurseNow.Services;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class AuthService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccessTokenGenerator _jwtService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    public AuthService(UserManager<ApplicationUser> userManager, IAccessTokenGenerator jwtService, IApplicationDbContext context, IEmailService emailService, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _userManager = userManager;
        _jwtService = jwtService;
        _context = context;
        _emailService = emailService;
    }

    public async Task<OperationResult> Register(RegisterDto model)
    {
        if (model.Role is not ("Patient" or "Nurse"))
            return BadRequest("Role must be Patient or Nurse.");
        model.Email = model.Email.Trim();
        await using var transaction = await _context.BeginTransactionAsync(_requestContext.CancellationToken);
        var userExists = await _userManager.FindByEmailAsync(model.Email);
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
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);
        if (model.Role == "Nurse")
        {
            var nurseProfile = new NurseProfile
            {
                UserId = user.Id,
                VerificationStatus = "Pending"
            };
            _context.NurseProfiles.Add(nurseProfile);
            _context.AdminActivityLogs.Add(new AdminActivityLog { Title = "New nurse registration", Description = $"{user.FullName} registered as a nurse.", ActivityType = "Registration", CreatedAt = DateTime.UtcNow });
        }
        else if (model.Role == "Patient")
        {
            var patientProfile = new PatientProfile
            {
                UserId = user.Id
            };
            _context.PatientProfiles.Add(patientProfile);
        }

        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        await transaction.CommitAsync(_requestContext.CancellationToken);
        return Ok("User registered successfully");
    }

    public async Task<OperationResult> Login(LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");
        if (user.AccountStatus != "Active" || (user.RoleType == "Administrator" && user.AdminApprovalStatus != "Active"))
            return Unauthorized("Your account is suspended");
        if (await _userManager.IsLockedOutAsync(user))
            return Unauthorized("Invalid credentials");
        var validPassword = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);
            return Unauthorized("Invalid credentials");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);
        return Ok(new { token, user.FullName, user.Email, roles });
    }

    public async Task<OperationResult> ForgotPassword(ForgotPasswordDto model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Email))
            return BadRequest("Email is required.");
        var email = model.Email.Trim().ToLower();
        var user = await _userManager.FindByEmailAsync(email);
        // Do not disclose whether the email exists.
        return Ok(new { message = "Reset request received. The Home Nurse team will contact you soon." });
    }

    public async Task<OperationResult> ResetPassword(ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return BadRequest("Invalid request.");
        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        }
        catch
        {
            return BadRequest("Invalid token.");
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return Ok(new { message = "Password has been reset successfully." });
    }
}
