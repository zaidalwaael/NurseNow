using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Repositories;
using NurseNow.Data;
using NurseNow.Models;

internal static class ArchitectureChecks
{
    public static async Task RunAsync(IServiceProvider provider, ApplicationDbContext db,
        DbContextOptions<ApplicationDbContext> options, Action<bool, string> check)
    {
        foreach (var assembly in new[] { typeof(ApplicationUser).Assembly, typeof(IUnitOfWork).Assembly })
            check(!assembly.GetReferencedAssemblies().Any(a => a.Name!.StartsWith("Microsoft.EntityFrameworkCore") || a.Name.StartsWith("Microsoft.AspNetCore")),
                assembly.GetName().Name + " has no EF Core or ASP.NET dependency");
        check(typeof(IBookingRepository).Assembly.GetTypes().Where(t => t.IsInterface && t.Namespace == typeof(IBookingRepository).Namespace)
            .SelectMany(t => t.GetMethods()).All(m => !ExposesQuery(m.ReturnType) && m.GetParameters().All(p => !ExposesQuery(p.ParameterType))),
            "Repository contracts expose materialized results, not IQueryable or DbSet");

        var uow = provider.GetRequiredService<IUnitOfWork>();
        var notifications = provider.GetRequiredService<INotificationRepository>();
        var profiles = provider.GetRequiredService<IPatientProfileRepository>();
        var notification = new Notification { UserId = "patient", Title = "Staged", Message = "Staged", Type = "Account" };
        notifications.Add(notification);
        profiles.Add(new PatientProfile { UserId = "patient" });
        check(!await db.Notifications.AnyAsync(n => n.Title == "Staged") && !await db.PatientProfiles.AnyAsync(),
            "Repository adds stay staged until Unit of Work saves");
        await uow.SaveChangesAsync();
        check(await db.Notifications.AnyAsync(n => n.Title == "Staged") && await db.PatientProfiles.AnyAsync(),
            "One Unit of Work persists changes from multiple repositories");
        await using (var transaction = await uow.BeginTransactionAsync())
        {
            notifications.Add(new Notification { UserId = "patient", Title = "Rollback", Message = "Rollback", Type = "Account" });
            profiles.Add(new PatientProfile { UserId = "nurse" });
            await uow.SaveChangesAsync();
        }
        db.ChangeTracker.Clear();
        check(!await db.Notifications.AnyAsync(n => n.Title == "Rollback") && !await db.PatientProfiles.AnyAsync(p => p.UserId == "nurse"),
            "Uncommitted transaction rolls back all repository writes");
        // EF bulk updates execute immediately, but participate in the shared UoW transaction.
        await using (var transaction = await uow.BeginTransactionAsync())
            await notifications.MarkAllReadAsync("patient", default);
        check(!await db.Notifications.Where(n => n.Title == "Staged").Select(n => n.IsRead).SingleAsync(),
            "Bulk notification updates participate in Unit of Work rollback");
        using (var cancelled = new CancellationTokenSource())
        {
            cancelled.Cancel();
            var rejected = false;
            try { await notifications.CountUnreadAsync("patient", cancelled.Token); }
            catch (OperationCanceledException) { rejected = true; }
            check(rejected, "Repository queries honor cancellation");
        }

        var roles = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var identity = provider.GetRequiredService<IIdentityService>();
        check((await roles.CreateAsync(new IdentityRole("Patient"))).Succeeded, "Identity role store works with the infrastructure context");
        var user = new ApplicationUser { UserName = "identity@example.test", Email = "identity@example.test", FullName = "Identity Test", RoleType = "Patient" };
        const string password = "TestPassword123!";
        check((await identity.CreateAsync(user, password)).Succeeded && !await db.Users.AnyAsync(u => u.Id == user.Id),
            "UserManager validates and hashes passwords without committing early");
        check((await identity.AddToRoleAsync(user, "Patient")).Succeeded, "Role assignment can be staged with a new user");
        profiles.Add(new PatientProfile { UserId = user.Id });
        await uow.SaveChangesAsync();
        db.ChangeTracker.Clear();
        user = (await identity.FindByEmailAsync(user.Email!))!;
        check(await identity.CheckPasswordAsync(user, password) && (await identity.GetRolesAsync(user)).Contains("Patient") && await db.PatientProfiles.AnyAsync(p => p.UserId == user.Id),
            "Identity user, role and business profile persist together");
        var stamp = user.SecurityStamp;
        var token = await identity.GeneratePasswordResetTokenAsync(user);
        check((await identity.ResetPasswordAsync(user, token, "ChangedPassword123!")).Succeeded, "Existing Identity password-reset token provider works");
        await uow.SaveChangesAsync();
        db.ChangeTracker.Clear();
        user = (await identity.FindByIdAsync(user.Id))!;
        check(await identity.CheckPasswordAsync(user, "ChangedPassword123!") && !await identity.CheckPasswordAsync(user, password) && user.SecurityStamp != stamp,
            "Password reset persists the new hash and revokes the previous security stamp");
        check(!(await identity.ResetPasswordAsync(user, token, "AnotherPassword123!")).Succeeded, "Password-reset token cannot be reused");
        for (var i = 0; i < 5; i++)
        {
            await identity.AccessFailedAsync(user);
            await uow.SaveChangesAsync();
        }
        db.ChangeTracker.Clear();
        user = (await identity.FindByIdAsync(user.Id))!;
        check(await identity.IsLockedOutAsync(user), "Identity lockout persists across queries");
        var rolledBack = new ApplicationUser { UserName = "rollback@example.test", Email = "rollback@example.test", FullName = "Rollback", RoleType = "Patient" };
        await using (var transaction = await uow.BeginTransactionAsync())
        {
            await identity.CreateAsync(rolledBack, password);
            await identity.AddToRoleAsync(rolledBack, "Patient");
            profiles.Add(new PatientProfile { UserId = rolledBack.Id });
            await uow.SaveChangesAsync();
        }
        db.ChangeTracker.Clear();
        check(await identity.FindByIdAsync(rolledBack.Id) == null && !await db.UserRoles.AnyAsync(r => r.UserId == rolledBack.Id) && !await db.PatientProfiles.AnyAsync(p => p.UserId == rolledBack.Id),
            "Registration transaction rolls back user, role and profile together");

        // Compare SQL Server relational metadata without opening a server connection.
        await using var sql = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=True;TrustServerCertificate=True").Options);
        var snapshot = sql.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
        var oldModel = sql.GetService<IModelRuntimeInitializer>().Initialize(snapshot, designTime: true);
        var currentModel = sql.GetService<IDesignTimeModel>().Model;
        var differences = sql.GetService<IMigrationsModelDiffer>().GetDifferences(oldModel.GetRelationalModel(), currentModel.GetRelationalModel());
        if (differences.Count != 0)
            Console.WriteLine("Schema differences: " + string.Join(", ", differences.Select(d => d.GetType().Name + " " + System.Text.Json.JsonSerializer.Serialize(d, d.GetType()))));
        check(differences.Count == 0, "SQL Server model matches the existing migration snapshot; no schema change required");
    }

    private static bool ExposesQuery(Type type) => type == typeof(IQueryable) ||
        type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IQueryable<>) || type.GetGenericTypeDefinition() == typeof(DbSet<>) || type.GetGenericArguments().Any(ExposesQuery));
}
