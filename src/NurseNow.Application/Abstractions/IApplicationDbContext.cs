using Microsoft.EntityFrameworkCore;
using NurseNow.Models;

namespace NurseNow.Application.Abstractions;
public interface IApplicationDbContext
{
    DbSet<NurseProfile> NurseProfiles { get; }

    DbSet<Payment> Payments { get; }

    DbSet<Complaint> Complaints { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<NurseNow.Models.Service> Services { get; }

    DbSet<ServiceCatalog> ServiceCatalogs { get; }

    DbSet<WeeklyAvailability> WeeklyAvailabilities { get; }

    DbSet<AvailabilityOverride> AvailabilityOverrides { get; }

    DbSet<Booking> Bookings { get; }

    DbSet<PatientProfile> PatientProfiles { get; }

    DbSet<NurseNow.Models.Review> Reviews { get; }

    DbSet<AdminActivityLog> AdminActivityLogs { get; }

    DbSet<ScheduledNotification> ScheduledNotifications { get; }

    DbSet<NurseDocument> NurseDocuments { get; }

    DbSet<SystemSetting> SystemSettings { get; }

    DbSet<ApplicationUser> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IApplicationTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}

public interface IApplicationTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
