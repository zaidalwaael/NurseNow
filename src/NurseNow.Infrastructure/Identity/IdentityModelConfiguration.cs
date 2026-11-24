using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NurseNow.Models;

namespace NurseNow.Infrastructure.Identity;

internal static class IdentityModelConfiguration
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("AspNetUsers");
            b.HasKey(u => u.Id);
            b.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();
            b.Property(u => u.UserName).HasMaxLength(256);
            b.Property(u => u.NormalizedUserName).HasMaxLength(256);
            b.Property(u => u.Email).HasMaxLength(256);
            b.Property(u => u.NormalizedEmail).HasMaxLength(256);
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
            b.HasIndex(u => u.NormalizedUserName).IsUnique().HasDatabaseName("UserNameIndex");
            b.HasMany<IdentityUserClaim<string>>().WithOne().HasForeignKey(x => x.UserId).IsRequired();
            b.HasMany<IdentityUserLogin<string>>().WithOne().HasForeignKey(x => x.UserId).IsRequired();
            b.HasMany<IdentityUserToken<string>>().WithOne().HasForeignKey(x => x.UserId).IsRequired();
            b.HasMany<IdentityUserRole<string>>().WithOne().HasForeignKey(x => x.UserId).IsRequired();
        });
        builder.Entity<IdentityRole>(b =>
        {
            b.ToTable("AspNetRoles"); b.HasKey(r => r.Id);
            b.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();
            b.Property(r => r.Name).HasMaxLength(256); b.Property(r => r.NormalizedName).HasMaxLength(256);
            b.HasIndex(r => r.NormalizedName).IsUnique().HasDatabaseName("RoleNameIndex");
            b.HasMany<IdentityUserRole<string>>().WithOne().HasForeignKey(r => r.RoleId).IsRequired();
            b.HasMany<IdentityRoleClaim<string>>().WithOne().HasForeignKey(r => r.RoleId).IsRequired();
        });
        builder.Entity<IdentityUserRole<string>>(b =>
        {
            b.ToTable("AspNetUserRoles");
            b.HasKey(x => new
            {
                x.UserId,
                x.RoleId
            });
        });
        builder.Entity<IdentityUserClaim<string>>(b =>
        {
            b.ToTable("AspNetUserClaims");
            b.HasKey(x => x.Id);
        });
        builder.Entity<IdentityRoleClaim<string>>(b =>
        {
            b.ToTable("AspNetRoleClaims");
            b.HasKey(x => x.Id);
        });
        builder.Entity<IdentityUserLogin<string>>(b =>
        {
            b.ToTable("AspNetUserLogins");
            b.HasKey(x => new
            {
                x.LoginProvider,
                x.ProviderKey
            });
        });
        builder.Entity<IdentityUserToken<string>>(b =>
        {
            b.ToTable("AspNetUserTokens");
            b.HasKey(x => new
            {
                x.UserId,
                x.LoginProvider,
                x.Name
            });
        });
    }
}
