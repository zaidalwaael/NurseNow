using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NurseNow.Models;
using Stripe;

namespace NurseNow.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<NurseProfile> NurseProfiles { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NurseNow.Models.Service> Services { get; set; }
        public DbSet<ServiceCatalog> ServiceCatalogs { get; set; }
        public DbSet<WeeklyAvailability> WeeklyAvailabilities { get; set; }
        public DbSet<AvailabilityOverride> AvailabilityOverrides { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<NurseNow.Models.Review> Reviews { get; set; }
        public DbSet<AdminActivityLog> AdminActivityLogs { get; set; }
        public DbSet<ScheduledNotification> ScheduledNotifications { get; set; }

        public DbSet<NurseDocument> NurseDocuments { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Entity<NurseNow.Models.Service>()
                .HasOne(s => s.Nurse)
                .WithMany()
                .HasForeignKey(s => s.NurseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<NurseNow.Models.Service>()
                .HasOne(s => s.ServiceCatalog)
                .WithMany()
                .HasForeignKey(s => s.ServiceCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<NurseNow.Models.Service>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            builder.Entity<WeeklyAvailability>()
                .HasOne(w => w.Nurse)
                .WithMany()
                .HasForeignKey(w => w.NurseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AvailabilityOverride>()
                .HasOne(o => o.Nurse)
                .WithMany()
                .HasForeignKey(o => o.NurseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Patient)
                .WithMany()
                .HasForeignKey(b => b.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Nurse)
                .WithMany()
                .HasForeignKey(b => b.NurseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany()
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PatientProfile>()
               .HasOne(p => p.User)
               .WithMany()
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<NurseNow.Models.Review>()
                .HasOne(r => r.Booking)
                .WithMany()
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<NurseNow.Models.Review>()
                .HasOne(r => r.Patient)
                .WithMany()
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<NurseNow.Models.Review>()
                .HasOne(r => r.Nurse)
                .WithMany()
                .HasForeignKey(r => r.NurseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SystemSetting>().ToTable("SystemSettings");

        }
    }
}