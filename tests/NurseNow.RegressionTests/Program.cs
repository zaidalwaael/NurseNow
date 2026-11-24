using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using NurseNow.Application.Repositories;
using NurseNow.Infrastructure.Identity;
using NurseNow.Infrastructure.Persistence;
using NurseNow.Infrastructure.Persistence.Repositories;
using System.Security.Claims;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;
using NurseNow.Application.Features;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Models;

var passed = 0;
void Check(bool ok, string name)
{
    if (!ok)
        throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
    passed++;
}

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
await using var db = new RelationalTestDbContext(options);
await db.Database.EnsureCreatedAsync();
var patient = new ApplicationUser
{
    Id = "patient",
    UserName = "patient",
    Email = "p@example.test",
    FullName = "Patient"
};
var nurse = new ApplicationUser
{
    Id = "nurse",
    UserName = "nurse",
    Email = "n@example.test",
    FullName = "Nurse"
};
db.Users.AddRange(patient, nurse);
var catalog = new ServiceCatalog
{
    Name = "Care",
    DefaultDurationInMinutes = 60
};
var service = new NurseNow.Models.Service
{
    NurseId = nurse.Id,
    ServiceCatalog = catalog,
    Price = 40m
};
db.Services.Add(service);
db.NurseProfiles.Add(new NurseProfile { UserId = nurse.Id, VerificationStatus = "Approved" });
var day = DateTime.Today.AddDays(2);
db.WeeklyAvailabilities.Add(new WeeklyAvailability { NurseId = nurse.Id, DayOfWeek = day.DayOfWeek.ToString(), StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) });
var booking = new Booking
{
    PatientId = patient.Id,
    NurseId = nurse.Id,
    Service = service,
    BookingDate = day,
    StartTime = TimeSpan.FromHours(10),
    EndTime = TimeSpan.FromHours(11),
    ServiceAddress = "Address",
    Status = "Accepted"
};
db.Bookings.Add(booking);
db.Notifications.AddRange(new Notification { UserId = patient.Id, Title = "P", Message = "P", Type = "Account" }, new Notification { UserId = nurse.Id, Title = "N", Message = "N", Type = "Account" });
await db.SaveChangesAsync();
var context = new TestContext(patient.Id);
var gateway = new TestGateway();
var registrations = new ServiceCollection();
registrations.AddLogging();
registrations.AddSingleton<ApplicationDbContext>(db);
registrations.AddSingleton<IUnitOfWork, UnitOfWork>();
foreach (var implementation in typeof(BookingRepository).Assembly.GetTypes().Where(t => t.Namespace == typeof(BookingRepository).Namespace && t.IsClass && !t.IsAbstract))
    foreach (var contract in implementation.GetInterfaces().Where(t => t.Namespace == typeof(IBookingRepository).Namespace))
        registrations.AddSingleton(contract, implementation);
registrations.AddIdentityCore<ApplicationUser>(o => { o.Password.RequiredLength = 12; o.User.RequireUniqueEmail = true; })
    .AddRoles<IdentityRole>().AddUserStore<ApplicationUserStore>()
    .AddRoleStore<RoleStore<IdentityRole, ApplicationDbContext, string>>().AddDefaultTokenProviders();
registrations.AddDataProtection();
registrations.AddSingleton<IIdentityService, IdentityService>();
registrations.AddSingleton<IRequestContext>(context);
registrations.AddSingleton<IPaymentGateway>(gateway);
using var provider = registrations.BuildServiceProvider();
var patients = ActivatorUtilities.CreateInstance<PatientService>(provider);
Check((await patients.BrowseNurses(new BrowseNursesQueryDto { PageSize = 0 })).Kind == ResultKind.Invalid, "Pagination rejects zero page size");
Check((await new AuthService(null!, null!, provider.GetRequiredService<IUnitOfWork>(), provider.GetRequiredService<INurseProfileRepository>(), provider.GetRequiredService<IPatientProfileRepository>(), provider.GetRequiredService<IAdminActivityLogRepository>(), null!, context).Register(new RegisterDto { Role = "Administrator" })).Kind == ResultKind.Invalid, "Public registration cannot create administrators");
var notifications = ActivatorUtilities.CreateInstance<NotificationService>(provider);
await notifications.MarkAllAsRead();
db.ChangeTracker.Clear();
Check(await db.Notifications.Where(n => n.UserId == patient.Id).AllAsync(n => n.IsRead) && await db.Notifications.Where(n => n.UserId == nurse.Id).AllAsync(n => !n.IsRead), "Bulk read preserves other users' notifications");
Check((await notifications.MarkAsRead(await db.Notifications.Where(n => n.UserId == nurse.Id).Select(n => n.NotificationId).SingleAsync())).Kind == ResultKind.NotFound, "Notification ownership enforced");
var slots = await patients.GetAvailableSlots(nurse.Id, service.ServiceId, day);
Check(slots.Kind == ResultKind.Success && slots.Value is List<string> list && !list.Contains("10:00") && list.Contains("11:00"), "Booked slots excluded; adjacent slots allowed");
Check((await patients.CreateBookingRequest(new CreateBookingRequestDto { NurseId = nurse.Id, ServiceId = service.ServiceId, Date = day, StartTime = TimeSpan.FromHours(10.5), ServiceAddress = "Address" })).Kind == ResultKind.Invalid && await db.Bookings.CountAsync() == 1, "Partially overlapping booking rejected");
Check((await patients.CreateBookingRequest(new CreateBookingRequestDto { NurseId = nurse.Id, ServiceId = service.ServiceId, Date = day, StartTime = TimeSpan.FromHours(12), ServiceAddress = "Address" })).Kind == ResultKind.Success && await db.Bookings.CountAsync() == 2, "Non-overlapping booking and notification saved");
Check((await patients.ConfirmPayment(booking.BookingId)).Kind == ResultKind.Invalid, "Payment without stored intent rejected");
Check((await patients.CreatePaymentIntent(booking.BookingId)).Kind == ResultKind.Success, "Intent creation stores payment intent");
await patients.CreatePaymentIntent(booking.BookingId);
Check(gateway.CreateCount == 1, "Repeated intent creation reuses stored intent");
Check((await patients.ConfirmPayment(booking.BookingId)).Kind == ResultKind.Invalid, "Unverified gateway payment rejected");
Check((await db.Payments.SingleAsync()).Status == "Pending", "Rejected confirmation does not mark payment paid");
gateway.Paid = true;
Check((await patients.ConfirmPayment(booking.BookingId)).Kind == ResultKind.Success, "Verified payment accepted");
Check((await db.Payments.SingleAsync()).Status == "Paid" && (await db.Bookings.SingleAsync(b => b.BookingId == booking.BookingId)).Status == "Active", "Payment and booking update together");
Check((await patients.ConfirmPayment(booking.BookingId)).Kind == ResultKind.Invalid, "Duplicate payment confirmation rejected");
Check((await ActivatorUtilities.CreateInstance<PatientService>(provider, new TestContext(nurse.Id)).ConfirmPayment(booking.BookingId)).Kind == ResultKind.NotFound, "Payment ownership enforced");
Check(!UploadPolicy.IsAllowed("payload.exe", 100, false) && !UploadPolicy.IsAllowed("photo.jpg", UploadPolicy.MaxBytes + 1, false), "Unsafe extension and oversized upload rejected");
await using var other = new RelationalTestDbContext(options);
var stale = await other.Bookings.SingleAsync(b => b.BookingId == booking.BookingId);
var current = await db.Bookings.SingleAsync(b => b.BookingId == booking.BookingId);
current.Status = "Completed";
await db.SaveChangesAsync();
stale.Status = "Cancelled";
var concurrencyRejected = false;
try
{
    await other.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    concurrencyRejected = true;
}

Check(concurrencyRejected, "Stale booking transition rejected");
var folder = Path.Combine(Path.GetTempPath(), "nursenow-files-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(folder);
try
{
    await File.WriteAllTextAsync(Path.Combine(folder, "document.pdf"), "%PDF-test");
    (await db.NurseProfiles.SingleAsync()).CertificatePath = "api/files/document.pdf";
    await db.SaveChangesAsync();
    var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Storage:Root"] = folder }).Build();
    var files = new NurseNow.API.Controllers.FilesController(provider.GetRequiredService<INurseDocumentRepository>(), configuration)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = context.User
            }
        }
    };
    Check(await files.Download("document.pdf", default) is ForbidResult, "Patient cannot download another user's certificate");
    files.HttpContext.User = new TestContext(nurse.Id).User;
    Check(await files.Download("document.pdf", default) is PhysicalFileResult, "Nurse can download own certificate");
    Check(await files.Download("../document.pdf", default) is BadRequestResult, "File traversal rejected");
}
finally
{
    Directory.Delete(folder, true);
}

await RepositoryQueryChecks.RunAsync(provider, db, Check);
await ArchitectureChecks.RunAsync(provider, db, options, Check);
await AuthFlowChecks.RunAsync(provider, db, Check);
var assembly = typeof(NurseNow.Controllers.PatientController).Assembly;
var baseline = JsonDocument.Parse(await File.ReadAllTextAsync(args.FirstOrDefault() ?? "docs/endpoint-baseline.json"));
foreach (var row in baseline.RootElement.EnumerateArray())
{
    var type = assembly.GetTypes().Single(t => t.Name == row.GetProperty("controller").GetString());
    var method = type.GetMethod(row.GetProperty("action").GetString()!)!;
    foreach (var attr in row.GetProperty("attributes").EnumerateArray())
    {
        var text = attr.GetString()!;
        if (!text.StartsWith("[Http"))
            continue;
        var verb = text[5..].Split('(', ']')[0].ToUpperInvariant();
        var template = text.Contains('"') ? text.Split('"')[1] : null;
        if (!method.GetCustomAttributes<HttpMethodAttribute>().Any(a => a.HttpMethods.Contains(verb) && a.Template == template))
            throw new Exception("Route changed: " + type.Name + "." + method.Name);
    }

    var classAttrs = row.GetProperty("classAttributes").GetString()!;
    if (classAttrs.Contains("[Authorize"))
    {
        var expectedRole = classAttrs.Contains("Roles = ") ? classAttrs.Split("Roles = \"")[1].Split('"')[0] : null;
        if (!type.GetCustomAttributes<AuthorizeAttribute>().Any(a => a.Roles == expectedRole))
            throw new Exception("Authorization changed: " + type.Name);
    }
}

Check(true, "104 legacy HTTP actions and class authorization retained");
Console.WriteLine($"Completed: {passed} checks passed. SQLite tests do not certify SQL Server migrations or live Stripe.");
sealed class TestContext(string id) : IRequestContext
{
    public ClaimsPrincipal User { get; } = new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id) }, "Test"));
    public string PublicBaseUrl => "https://test.invalid";
    public CancellationToken CancellationToken => CancellationToken.None;
}

sealed class TestGateway : IPaymentGateway
{
    public bool Paid { get; set; }
    public int CreateCount { get; private set; }

    public Task<PaymentIntentData> CreateAsync(int bookingId, string patientId, decimal amount, CancellationToken cancellationToken)
    {
        CreateCount++;
        return Task.FromResult(new PaymentIntentData("pi_test", "test-secret"));
    }

    public Task<PaymentIntentData> GetAsync(string intentId, CancellationToken cancellationToken) => Task.FromResult(new PaymentIntentData(intentId, "test-secret"));
    public Task<bool> IsPaidAsync(string intentId, int bookingId, string patientId, decimal amount, CancellationToken cancellationToken) => Task.FromResult(Paid);
}

// SQLite has no native TimeSpan ordering; this fixture stores ticks. Production uses SQL Server time.
sealed class RelationalTestDbContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        foreach (var entity in builder.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
                if (property.ClrType == typeof(TimeSpan) || property.ClrType == typeof(TimeSpan? ))
                    builder.Entity(entity.ClrType).Property(property.Name).HasConversion<long>();
    }
}
