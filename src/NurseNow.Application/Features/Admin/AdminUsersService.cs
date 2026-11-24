using NurseNow.Application.Repositories;
using NurseNow.Models;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminUsersService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationUserRepository _users;
    private readonly INurseProfileRepository _nurseProfiles;
    private readonly IPatientProfileRepository _patientProfiles;
    private readonly IAdminActivityLogRepository _adminActivityLogs;
    private readonly IIdentityService _identityService;
    public AdminUsersService(IUnitOfWork unitOfWork, IApplicationUserRepository users, INurseProfileRepository nurseProfiles, IPatientProfileRepository patientProfiles, IAdminActivityLogRepository adminActivityLogs, IIdentityService identityService, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _users = users;
        _nurseProfiles = nurseProfiles;
        _patientProfiles = patientProfiles;
        _adminActivityLogs = adminActivityLogs;
        _identityService = identityService;
    }

    public async Task<OperationResult> GetUsers(string type = "Patient", string? search = null, string? status = null)
    {
        var users = await _users.SearchUsersAsync(type, search, status, _requestContext.CancellationToken);
        return Ok(users);
    }

    public async Task<OperationResult> GetUserDetails(string userId)
    {
        var user = await _users.GetDetailsAsync(userId, _requestContext.CancellationToken);
        if (user == null)
            return NotFound(new { message = "User not found" });
        object? profile = null;
        if (user.RoleType == "Patient")
        {
            profile = await _patientProfiles.GetAccountDetailsAsync(userId, _requestContext.CancellationToken);
        }

        if (user.RoleType == "Nurse")
        {
            profile = await _nurseProfiles.GetAccountDetailsAsync(userId, _requestContext.CancellationToken);
        }

        return Ok(new { userId = user.Id, fullName = user.FullName, email = user.Email, phoneNumber = user.PhoneNumber, roleType = user.RoleType, accountStatus = user.AccountStatus, createdAt = user.CreatedAt, lastLoginAt = user.LastLoginAt, profile });
    }

    public async Task<OperationResult> ToggleUserStatus(string userId)
    {
        var user = await _identityService.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });
        user.AccountStatus = user.AccountStatus == "Suspended" ? "Active" : "Suspended";
        var result = await _identityService.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        _adminActivityLogs.Add(new AdminActivityLog { Title = "User status updated", Description = $"{user.FullName} status changed to {user.AccountStatus}.", ActivityType = "UserManagement", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = $"User status changed to {user.AccountStatus}", status = user.AccountStatus });
    }

    public async Task<OperationResult> ResetUserPassword(string userId, ResetUserPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "New password is required" });
        var user = await _identityService.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });
        var token = await _identityService.GeneratePasswordResetTokenAsync(user);
        var result = await _identityService.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        _adminActivityLogs.Add(new AdminActivityLog { Title = "User password reset", Description = $"{user.FullName} password was reset by admin.", ActivityType = "UserManagement", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Password reset successfully" });
    }
}

public class ResetUserPasswordDto
{
    public string NewPassword { get; set; } = "";
}
