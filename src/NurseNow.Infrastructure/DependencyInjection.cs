using NurseNow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NurseNow.Infrastructure.Persistence;
using NurseNow.Infrastructure.Persistence.Repositories;
using NurseNow.Application.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NurseNow.Application.Abstractions;
using NurseNow.Data;
using NurseNow.Helpers;
using NurseNow.Models;
using NurseNow.Services;
using NurseNow.Settings;
using NurseNow.Infrastructure.Storage;
using NurseNow.Infrastructure.Payments;

namespace NurseNow.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAdminActivityLogRepository, AdminActivityLogRepository>();
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        services.AddScoped<IAvailabilityOverrideRepository, AvailabilityOverrideRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IComplaintRepository, ComplaintRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INurseDocumentRepository, NurseDocumentRepository>();
        services.AddScoped<INurseProfileRepository, NurseProfileRepository>();
        services.AddScoped<IPatientProfileRepository, PatientProfileRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IServiceCatalogRepository, ServiceCatalogRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IWeeklyAvailabilityRepository, WeeklyAvailabilityRepository>();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
        }).AddRoles<IdentityRole>().AddUserStore<ApplicationUserStore>().AddRoleStore<RoleStore<IdentityRole, ApplicationDbContext, string>>().AddDefaultTokenProviders();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddDataProtection();
        services.AddScoped<IAccessTokenGenerator, JwtService>();
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        return services;
    }
}
