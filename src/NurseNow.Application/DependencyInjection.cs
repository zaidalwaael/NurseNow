using Microsoft.Extensions.DependencyInjection;

namespace NurseNow.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<NurseNow.Application.Features.PatientService>();
        services.AddScoped<NurseNow.Application.Features.NursePaymentsService>();
        services.AddScoped<NurseNow.Application.Features.AuthService>();
        services.AddScoped<NurseNow.Application.Features.AccountService>();
        services.AddScoped<NurseNow.Application.Features.NotificationService>();
        services.AddScoped<NurseNow.Application.Features.PatientPaymentsService>();
        services.AddScoped<NurseNow.Application.Features.ReviewService>();
        services.AddScoped<NurseNow.Application.Features.PatientIssuesService>();
        services.AddScoped<NurseNow.Application.Features.NurseService>();
        services.AddScoped<NurseNow.Application.Features.TestService>();
        services.AddScoped<NurseNow.Application.Features.AdminService>();
        services.AddScoped<NurseNow.Application.Features.NurseProblemsService>();
        services.AddScoped<NurseNow.Application.Features.PatientProfileService>();
        services.AddScoped<NurseNow.Application.Features.Admin.AdminUsersService>();
        services.AddScoped<NurseNow.Application.Features.Admin.AdminComplaintsService>();
        services.AddScoped<NurseNow.Application.Features.Admin.AdminNotificationsListService>();
        services.AddScoped<NurseNow.Application.Features.Admin.AdminNurseVerificationService>();
        services.AddScoped<NurseNow.Application.Features.Admin.AdminNotificationsService>();
        services.AddScoped<NurseNow.Application.Features.Admin.AdminManagementService>();
        services.AddScoped<NurseNow.Application.Features.Admin.AdminPaymentsService>();
        services.AddScoped<NurseNow.Application.Features.Admin.ServiceRequestsService>();
        services.AddScoped<NurseNow.Application.Features.Admin.AdminDashboardService>();
        services.AddScoped<NurseNow.Application.Features.Admin.AdminProfileService>();
        return services;
    }
}
