using NurseNow.Application.Repositories;
using NurseNow.Models;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminProfileService : ApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestContext _requestContext;
    private readonly IIdentityService _identityService;
    public AdminProfileService(IIdentityService identityService, IUnitOfWork unitOfWork, IRequestContext requestContext)
    {
        _unitOfWork = unitOfWork;
        _requestContext = requestContext;
        _identityService = identityService;
    }

    public async Task<OperationResult> GetProfile()
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();
        var user = await _identityService.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "Admin not found" });
        return Ok(new { fullName = user.FullName, email = user.Email, address = user.Location, phoneNumber = user.PhoneNumber });
    }

    public async Task<OperationResult> UpdateProfile(UpdateAdminProfileDto dto)
    {
        var userId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();
        var user = await _identityService.FindByIdAsync(userId);
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
            var token = await _identityService.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _identityService.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!passwordResult.Succeeded)
                return BadRequest(passwordResult.Errors);
        }

        var result = await _identityService.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
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
