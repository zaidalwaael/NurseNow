using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NurseNow.Application.Repositories;
using NurseNow.Data;
using NurseNow.DTOs;

internal static class RepositoryQueryChecks
{
    public static async Task RunAsync(IServiceProvider provider, ApplicationDbContext db, Action<bool, string> check)
    {
        db.ChangeTracker.Clear();
        var bookings = provider.GetRequiredService<IBookingRepository>();
        var id = await db.Bookings.OrderBy(b => b.BookingId).Select(b => b.BookingId).FirstAsync();
        var read = await bookings.GetPatientDetailsAsync(id, "patient", default);
        check(read?.Nurse.FullName == "Nurse" && read.Service.ServiceCatalog.Name == "Care" && !db.ChangeTracker.Entries().Any(),
            "Booking read loads related details without tracking entities");
        check(await bookings.GetPatientDetailsAsync(id, "nurse", default) == null,
            "Repository detail query enforces patient ownership");
        var tracked = await bookings.GetPatientBookingForUpdateAsync(id, "patient", default);
        check(tracked != null && db.Entry(tracked).State == EntityState.Unchanged,
            "Booking write query returns a tracked entity for Unit of Work");
        db.ChangeTracker.Clear();
        var upcoming = await bookings.ListPatientAppointmentsAsync("upcoming", 1, "patient", default);
        var past = await bookings.ListPatientAppointmentsAsync("past", null, "patient", default);
        check(upcoming.Count == 1 && upcoming[0].Status == "Pending" && past.Any(b => b.Status == "Completed"),
            "Patient appointment filtering and limits preserved");
        var requests = await bookings.ListNurseRequestsAsync("Pending", "nurse", default);
        check(requests.Count == 1 && requests[0].patientName == "Patient" && requests[0].totalPrice == 40m,
            "Nurse request projection preserves filter and related values");
        var nurses = provider.GetRequiredService<INurseProfileRepository>();
        var catalog = await db.ServiceCatalogs.Select(c => c.ServiceCatalogId).FirstAsync();
        var search = await nurses.SearchApprovedAsync(new BrowseNursesQueryDto { Search = "nurse", ServiceCatalogId = catalog, PageNumber = 1, PageSize = 1 }, "https://test.invalid", default);
        check(search.totalCount == 1 && search.nurses.Count == 1 && search.nurses[0].price == 40m && search.nurses[0].nurseId == "nurse",
            "Nurse search, service filter, pagination and price projection preserved");
        var empty = await nurses.SearchApprovedAsync(new BrowseNursesQueryDto { Search = "absent", ServiceCatalogId = catalog, PageNumber = 1, PageSize = 1 }, "https://test.invalid", default);
        check(empty.totalCount == 0 && empty.nurses.Count == 0, "Nurse search excludes nonmatching records");
        var notifications = await provider.GetRequiredService<INotificationRepository>().ListForUserAsync("patient", default);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(notifications.First()));
        check(json.RootElement.EnumerateObject().Select(p => p.Name).Order().SequenceEqual(new[] { "NotificationId", "Title", "Message", "Type", "BookingId", "IsRead", "CreatedAt", "targetScreen" }.Order()),
            "Named read models preserve original notification JSON property names");
    }
}
