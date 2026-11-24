using NurseNow.Models;

namespace NurseNow.Application.Abstractions;

public sealed record IdentityOperationError(string Code, string Description);
public sealed record IdentityOperationResult(bool Succeeded, IReadOnlyList<IdentityOperationError> Errors);

public interface IIdentityService
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(string userId);
    Task<IdentityOperationResult> CreateAsync(ApplicationUser user, string password);
    Task<IdentityOperationResult> UpdateAsync(ApplicationUser user);
    Task<IdentityOperationResult> DeleteAsync(ApplicationUser user);
    Task<IdentityOperationResult> AddToRoleAsync(ApplicationUser user, string role);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<bool> IsLockedOutAsync(ApplicationUser user);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IdentityOperationResult> AccessFailedAsync(ApplicationUser user);
    Task<IdentityOperationResult> ResetAccessFailedCountAsync(ApplicationUser user);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityOperationResult> ResetPasswordAsync(ApplicationUser user, string token, string password);
}
