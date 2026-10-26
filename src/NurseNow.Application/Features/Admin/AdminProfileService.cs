using Microsoft.AspNetCore.Identity;
using NurseNow.Models;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminProfileService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly UserManager<ApplicationUser> _userManager;
    public AdminProfileService(UserManager<ApplicationUser> userManager, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _userManager = userManager;
    }

    public async Task<OperationResult> GetProfile()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "Admin not found" });
        return Ok(new { fullName = user.FullName, email = user.Email, address = user.Location, phoneNumber = user.PhoneNumber });
    }

    public async Task<OperationResult> UpdateProfile(UpdateAdminProfileDto dto)
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "Admin not found" });
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest(new { message = "Full name is required" });
        user.FullName = dto.FullName;
        user.Location = dto.Address;
        user.PhoneNumber = dto.PhoneNumber;
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match" });
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!passwordResult.Succeeded)
                return BadRequest(passwordResult.Errors);
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return Ok(new { message = "Profile updated successfully", fullName = user.FullName, email = user.Email, address = user.Location, phoneNumber = user.PhoneNumber });
    }
}

public class UpdateAdminProfileDto
{
    public string FullName { get; set; } = "";
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}
