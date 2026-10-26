using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NurseNow.Models;

namespace NurseNow.Seed;
public static class RoleSeeder
{
    public static async Task SeedRolesAndAdminAsync(RoleManager<IdentityRole> roles, UserManager<ApplicationUser> users, IConfiguration configuration)
    {
        foreach (var name in new[]
        {
            "Patient",
            "Nurse",
            "Administrator"
        }

        )
            if (!await roles.RoleExistsAsync(name))
            {
                var result = await roles.CreateAsync(new IdentityRole(name));
                if (!result.Succeeded)
                    throw new InvalidOperationException("Role seed failed.");
            }

        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;
        if (await users.FindByEmailAsync(email)is not null)
            return;
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "System Administrator",
            RoleType = "Administrator",
            AccountStatus = "Active",
            AdminRole = "SuperAdmin",
            AdminApprovalStatus = "Active",
            EmailConfirmed = true
        };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
            throw new InvalidOperationException("Admin seed failed: " + string.Join(",", created.Errors.Select(e => e.Code)));
        if (!(await users.AddToRoleAsync(user, "Administrator")).Succeeded)
            throw new InvalidOperationException("Admin role assignment failed.");
    }
}
