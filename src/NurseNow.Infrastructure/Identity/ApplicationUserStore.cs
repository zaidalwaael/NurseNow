using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.Models;

namespace NurseNow.Infrastructure.Identity;

// Identity's hashing, validation and token providers remain in UserManager.
// Store writes share the same tracked context as the business repositories.
public sealed class ApplicationUserStore(ApplicationDbContext db) : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserEmailStore<ApplicationUser>, IUserSecurityStampStore<ApplicationUser>, IUserRoleStore<ApplicationUser>, IUserLockoutStore<ApplicationUser>, IUserPhoneNumberStore<ApplicationUser>, IUserTwoFactorStore<ApplicationUser>
{
    public void Dispose() { }
    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.Id);
    }
    public Task<ApplicationUser?> FindByIdAsync(string id, CancellationToken cancellationToken) => db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    public Task<ApplicationUser?> FindByNameAsync(string name, CancellationToken cancellationToken) => db.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == name, cancellationToken);
    public Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken) => db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email, cancellationToken);
    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        db.Users.Add(user);
        return Task.FromResult(IdentityResult.Success);
    }
    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (db.Entry(user).State == EntityState.Detached) db.Users.Attach(user);
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        if (db.Entry(user).State != EntityState.Added) db.Entry(user).State = EntityState.Modified;
        return Task.FromResult(IdentityResult.Success);
    }
    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        db.Users.Remove(user);
        return Task.FromResult(IdentityResult.Success);
    }
    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.UserName);
    }
    public Task SetUserNameAsync(ApplicationUser user, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.UserName = value;
        return Task.CompletedTask;
    }
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.NormalizedUserName);
    }
    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NormalizedUserName = value;
        return Task.CompletedTask;
    }
    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PasswordHash);
    }
    public Task SetPasswordHashAsync(ApplicationUser user, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.PasswordHash = value;
        return Task.CompletedTask;
    }
    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.Email);
    }
    public Task SetEmailAsync(ApplicationUser user, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.Email = value;
        return Task.CompletedTask;
    }
    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.NormalizedEmail);
    }
    public Task SetNormalizedEmailAsync(ApplicationUser user, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NormalizedEmail = value;
        return Task.CompletedTask;
    }
    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.EmailConfirmed);
    }
    public Task SetEmailConfirmedAsync(ApplicationUser user, bool value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.EmailConfirmed = value;
        return Task.CompletedTask;
    }
    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.SecurityStamp);
    }
    public Task SetSecurityStampAsync(ApplicationUser user, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.SecurityStamp = value;
        return Task.CompletedTask;
    }
    public Task<string?> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PhoneNumber);
    }
    public Task SetPhoneNumberAsync(ApplicationUser user, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.PhoneNumber = value;
        return Task.CompletedTask;
    }
    public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PhoneNumberConfirmed);
    }
    public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.PhoneNumberConfirmed = value;
        return Task.CompletedTask;
    }
    public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.TwoFactorEnabled);
    }
    public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.TwoFactorEnabled = value;
        return Task.CompletedTask;
    }
    public Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.LockoutEnabled);
    }
    public Task SetLockoutEnabledAsync(ApplicationUser user, bool value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.LockoutEnabled = value;
        return Task.CompletedTask;
    }
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.LockoutEnd);
    }
    public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.LockoutEnd = value;
        return Task.CompletedTask;
    }
    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PasswordHash != null);
    }
    public Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.AccessFailedCount);
    }
    public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(++user.AccessFailedCount);
    }
    public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }
    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = await db.Roles.SingleAsync(r => r.NormalizedName == roleName, cancellationToken);
        db.UserRoles.Add(new IdentityUserRole<string>
        {
            UserId = user.Id,
            RoleId = role.Id
        });
    }
    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = await db.Roles.SingleAsync(r => r.NormalizedName == roleName, cancellationToken);
        var link = await db.UserRoles.FindAsync(new object[] {
 user.Id, role.Id
}, cancellationToken);
        if (link != null) db.UserRoles.Remove(link);
    }
    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        => await (from link in db.UserRoles join role in db.Roles on link.RoleId equals role.Id where link.UserId == user.Id select role.Name!).ToListAsync(cancellationToken);
    public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        => (from link in db.UserRoles join role in db.Roles on link.RoleId equals role.Id where link.UserId == user.Id && role.NormalizedName == roleName select link).AnyAsync(cancellationToken);
    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        => await (from user in db.Users join link in db.UserRoles on user.Id equals link.UserId join role in db.Roles on link.RoleId equals role.Id where role.NormalizedName == roleName select user).ToListAsync(cancellationToken);
}
