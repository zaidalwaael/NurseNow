using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NurseNow.Models;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminUsersService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public AdminUsersService(IApplicationDbContext context, UserManager<ApplicationUser> userManager, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
        _userManager = userManager;
    }

    public async Task<OperationResult> GetUsers(string type = "Patient", string? search = null, string? status = null)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();
        if (type == "Nurse")
        {
            query = query.Where(u => u.RoleType == "Nurse" && _context.NurseProfiles.AsNoTracking().Any(n => n.UserId == u.Id && n.VerificationStatus == "Approved"));
        }
        else
        {
            query = query.Where(u => u.RoleType == "Patient");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.FullName.Contains(search) || (u.Email != null && u.Email.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            query = query.Where(u => u.AccountStatus == status);
        }

        var users = await query.OrderByDescending(u => u.CreatedAt).Select(u => new { userId = u.Id, name = u.FullName, email = u.Email, phone = u.PhoneNumber, joinDate = u.CreatedAt, status = u.AccountStatus, roleType = u.RoleType }).ToListAsync(_requestContext.CancellationToken);
        return Ok(users);
    }

    public async Task<OperationResult> GetUserDetails(string userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, _requestContext.CancellationToken);
        if (user == null)
            return NotFound(new { message = "User not found" });
        object? profile = null;
        if (user.RoleType == "Patient")
        {
            profile = await _context.PatientProfiles.AsNoTracking().Where(p => p.UserId == userId).Select(p => new { p.PhoneNumber, p.Gender, p.DateOfBirth, p.BloodType, p.Governorate, p.Area, p.Address, p.Conditions, p.Allergies, p.Notes }).FirstOrDefaultAsync(_requestContext.CancellationToken);
        }

        if (user.RoleType == "Nurse")
        {
            profile = await _context.NurseProfiles.AsNoTracking().Where(n => n.UserId == userId).Select(n => new { n.PhoneNumber, n.Location, n.Address, n.Specialization, n.ExperienceYears, n.LicenseNumber, n.VerificationStatus, n.Bio }).FirstOrDefaultAsync(_requestContext.CancellationToken);
        }

        return Ok(new { userId = user.Id, fullName = user.FullName, email = user.Email, phoneNumber = user.PhoneNumber, roleType = user.RoleType, accountStatus = user.AccountStatus, createdAt = user.CreatedAt, lastLoginAt = user.LastLoginAt, profile });
    }

    public async Task<OperationResult> ToggleUserStatus(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });
        user.AccountStatus = user.AccountStatus == "Suspended" ? "Active" : "Suspended";
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        _context.AdminActivityLogs.Add(new AdminActivityLog { Title = "User status updated", Description = $"{user.FullName} status changed to {user.AccountStatus}.", ActivityType = "UserManagement", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = $"User status changed to {user.AccountStatus}", status = user.AccountStatus });
    }

    public async Task<OperationResult> ResetUserPassword(string userId, ResetUserPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "New password is required" });
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        _context.AdminActivityLogs.Add(new AdminActivityLog { Title = "User password reset", Description = $"{user.FullName} password was reset by admin.", ActivityType = "UserManagement", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Password reset successfully" });
    }
}

public class ResetUserPasswordDto
{
    public string NewPassword { get; set; } = "";
}
