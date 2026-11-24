using Microsoft.AspNetCore.Identity;
using NurseNow.Application.Abstractions;
using NurseNow.Models;

namespace NurseNow.Infrastructure.Identity;

public sealed class IdentityService(UserManager<ApplicationUser> users) : IIdentityService
{
    public Task<ApplicationUser?> FindByEmailAsync(string email) => users.FindByEmailAsync(email);
    public Task<ApplicationUser?> FindByIdAsync(string userId) => users.FindByIdAsync(userId);
    public async Task<IdentityOperationResult> CreateAsync(ApplicationUser user, string password) => Map(await users.CreateAsync(user, password));
    public async Task<IdentityOperationResult> UpdateAsync(ApplicationUser user) => Map(await users.UpdateAsync(user));
    public async Task<IdentityOperationResult> DeleteAsync(ApplicationUser user) => Map(await users.DeleteAsync(user));
    public async Task<IdentityOperationResult> AddToRoleAsync(ApplicationUser user, string role) => Map(await users.AddToRoleAsync(user, role));
    public Task<IList<string>> GetRolesAsync(ApplicationUser user) => users.GetRolesAsync(user);
    public Task<bool> IsLockedOutAsync(ApplicationUser user) => users.IsLockedOutAsync(user);
    public Task<bool> CheckPasswordAsync(ApplicationUser user, string password) => users.CheckPasswordAsync(user, password);
    public async Task<IdentityOperationResult> AccessFailedAsync(ApplicationUser user) => Map(await users.AccessFailedAsync(user));
    public async Task<IdentityOperationResult> ResetAccessFailedCountAsync(ApplicationUser user) => Map(await users.ResetAccessFailedCountAsync(user));
    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) => users.GeneratePasswordResetTokenAsync(user);
    public async Task<IdentityOperationResult> ResetPasswordAsync(ApplicationUser user, string token, string password) => Map(await users.ResetPasswordAsync(user, token, password));

    private static IdentityOperationResult Map(IdentityResult result)
        => new(result.Succeeded, result.Errors.Select(e => new IdentityOperationError(e.Code, e.Description)).ToArray());
}
