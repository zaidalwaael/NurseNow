using Microsoft.AspNetCore.Identity;
using NurseNow.Models;

namespace NurseNow.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAndAdminAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            string[] roles = { "Patient", "Nurse", "Administrator" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string adminEmail = "admin@nursenow.com";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    RoleType = "Administrator",
                    AccountStatus = "Active",
                    AdminRole = "SuperAdmin",
                    AdminApprovalStatus = "Active",
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, "Admin@123");

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrator");
                }
            }
        }
    }
}