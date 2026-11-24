using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NurseNow.Application;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;
using NurseNow.Application.Features;
using NurseNow.Application.Repositories;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Infrastructure;
using NurseNow.Models;

internal static class AuthFlowChecks
{
    public static async Task RunAsync(IServiceProvider provider, ApplicationDbContext db, Action<bool, string> check)
    {
        var roles = provider.GetRequiredService<RoleManager<IdentityRole>>();
        await roles.CreateAsync(new IdentityRole("Nurse"));
        var identity = provider.GetRequiredService<IIdentityService>();
        var uow = provider.GetRequiredService<IUnitOfWork>();
        var auth = new AuthService(identity, new TestTokens(), uow,
            provider.GetRequiredService<INurseProfileRepository>(), provider.GetRequiredService<IPatientProfileRepository>(),
            provider.GetRequiredService<IAdminActivityLogRepository>(), null!, new TestContext("patient"));
        var registration = new RegisterDto { Email = "flow@example.test", Password = "FlowPassword123!", FullName = "Registered Nurse", Role = "Nurse" };
        check((await auth.Register(registration)).Kind == ResultKind.Success, "Registration use case works through repositories and identity port");
        db.ChangeTracker.Clear();
        var user = (await identity.FindByEmailAsync(registration.Email))!;
        check((await identity.GetRolesAsync(user)).Contains("Nurse") && await db.NurseProfiles.AnyAsync(n => n.UserId == user.Id && n.VerificationStatus == "Pending") && await db.AdminActivityLogs.AnyAsync(a => a.Title == "New nurse registration"),
            "Nurse registration saves role, pending profile and admin activity");
        check((await auth.Register(registration)).Kind == ResultKind.Invalid, "Duplicate registration rejected");
        check((await auth.Login(new LoginDto { Email = registration.Email, Password = "WrongPassword123!" })).Kind == ResultKind.Unauthenticated,
            "Login rejects incorrect password");
        db.ChangeTracker.Clear();
        check((await identity.FindByIdAsync(user.Id))!.AccessFailedCount == 1, "Failed-login counter saved by Unit of Work");
        check((await auth.Login(new LoginDto { Email = registration.Email, Password = registration.Password })).Kind == ResultKind.Success,
            "Login accepts the registered password");
        db.ChangeTracker.Clear();
        user = (await identity.FindByIdAsync(user.Id))!;
        check(user.AccessFailedCount == 0, "Successful login persists reset of failed-login counter");
        var token = await identity.GeneratePasswordResetTokenAsync(user);
        var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        check((await auth.ResetPassword(new ResetPasswordDto { Email = user.Email!, Token = encoded, NewPassword = "ResetPassword123!" })).Kind == ResultKind.Success,
            "Reset-password use case accepts existing base64url token format");
        db.ChangeTracker.Clear();
        user = (await identity.FindByIdAsync(user.Id))!;
        check(await identity.CheckPasswordAsync(user, "ResetPassword123!"), "Reset-password use case persists the new password");
        user.FullName = "Updated Nurse";
        await identity.UpdateAsync(user);
        await uow.SaveChangesAsync();
        db.ChangeTracker.Clear();
        user = (await identity.FindByIdAsync(user.Id))!;
        check(user.FullName == "Updated Nurse", "Identity profile update persists through Unit of Work");
        await identity.DeleteAsync(user);
        await uow.SaveChangesAsync();
        db.ChangeTracker.Clear();
        check(await identity.FindByIdAsync(user.Id) == null && !await db.UserRoles.AnyAsync(r => r.UserId == user.Id) && !await db.NurseProfiles.AnyAsync(p => p.UserId == user.Id),
            "Identity delete and related role/profile cascades remain intact");

        // Production registrations must resolve every application use case in a real scope.
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=ModelOnly;Integrated Security=True;TrustServerCertificate=True" }).Build();
        var registrations = new ServiceCollection();
        registrations.AddLogging();
        registrations.AddSingleton<IConfiguration>(config);
        registrations.AddSingleton<IRequestContext>(new TestContext("patient"));
        registrations.AddApplication().AddInfrastructure(config);
        using var production = registrations.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var scope = production.CreateScope();
        var services = typeof(AuthService).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(ApplicationService).IsAssignableFrom(t)).ToArray();
        foreach (var service in services) scope.ServiceProvider.GetRequiredService(service);
        check(services.Length == 23, "All 23 application services resolve with production dependency injection");
    }
    private sealed class TestTokens : IAccessTokenGenerator
    {
        public string GenerateToken(ApplicationUser user, IList<string> roles) => "test-token";
    }
}
