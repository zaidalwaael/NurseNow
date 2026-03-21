using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NurseNow.Models;

namespace NurseNow.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<NurseProfile> NurseProfiles { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCatalog> ServiceCatalogs { get; set; }
        public DbSet<WeeklyAvailability> WeeklyAvailabilities { get; set; }
        public DbSet<AvailabilityOverride> AvailabilityOverrides { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ✅ أضف هذا الجزء
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Precision for money
            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);



            builder.Entity<Service>()
           .HasOne(s => s.Nurse)
           .WithMany()
           .HasForeignKey(s => s.NurseId)
           .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

            builder.Entity<Service>()
            .HasOne(s => s.ServiceCatalog)
    .WithMany()
    .HasForeignKey(s => s.ServiceCatalogId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Service>()
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


        }


    }
}