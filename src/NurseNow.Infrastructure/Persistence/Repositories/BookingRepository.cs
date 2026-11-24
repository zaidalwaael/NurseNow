using NurseNow.Application.DTOs.Persistence;
using NurseNow.Models;
using NurseNow.DTOs;
using NurseNow.DTOs.Nurse;
using NurseNow.DTOs.Patient;
using NurseNow.Application.Features.Admin;
using Microsoft.EntityFrameworkCore;
using NurseNow.Application.Repositories;
using NurseNow.Data;

namespace NurseNow.Infrastructure.Persistence.Repositories;
public sealed class BookingRepository(ApplicationDbContext context) : IBookingRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(Booking entity) => _context.Bookings.Add(entity);
    public void Remove(Booking entity) => _context.Bookings.Remove(entity);
    public async Task<List<ReservedTimeSlot>> ListReservedSlotsAsync(string nurseId, DateTime day, DateTime nextDay, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.NurseId == nurseId && b.BookingDate >= day && b.BookingDate < nextDay && b.Status != "Rejected" && b.Status != "Cancelled")
            .Select(b => new ReservedTimeSlot
            {
                StartTime = b.StartTime,
                EndTime = b.EndTime
            }).ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(CreateBookingRequestDto model, TimeSpan endTime, CancellationToken cancellationToken)
    {
        return await _context.Bookings.AnyAsync(b => b.NurseId == model.NurseId && b.BookingDate.Date == model.Date.Date && b.StartTime < endTime && b.EndTime > model.StartTime && b.Status != "Rejected" && b.Status != "Cancelled", cancellationToken);
    }

    public async Task<List<Booking>> ListPatientAppointmentsAsync(string tab, int? limit, string? patientId, CancellationToken cancellationToken)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Nurse)
            .Include(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .Where(b => b.PatientId == patientId).AsQueryable();
        if (tab.ToLower() == "upcoming")
        {
            query = query.Where(b => b.Status == "Pending" || b.Status == "Accepted" || b.Status == "Active");
        }
        else if (tab.ToLower() == "past")
        {
            query = query.Where(b => b.Status == "Cancelled" || b.Status == "Rejected" || b.Status == "Completed");
        }

        query = query.OrderBy(b => b.BookingDate).ThenBy(b => b.StartTime);
        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(Math.Clamp(limit.Value, 1, 100));
        }

        var bookings = await query.ToListAsync(cancellationToken);
        return bookings;
    }

    public async Task<Booking?> GetPatientDetailsAsync(int bookingId, string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Nurse)
            .Include(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, cancellationToken);
    }

    public async Task<Booking?> GetPatientBookingForUpdateAsync(int bookingId, string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, cancellationToken);
    }

    public async Task<Booking?> GetPatientBookingWithServiceForUpdateAsync(int bookingId, string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .Include(b => b.Service)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, cancellationToken);
    }

    public async Task<Booking?> GetPatientBookingAsync(int bookingId, string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, cancellationToken);
    }

    public async Task<int> CountActiveForPatientAsync(string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .AsNoTracking().CountAsync(b => b.PatientId == patientId && (b.Status == "Pending" || b.Status == "Accepted" || b.Status == "Active"), cancellationToken);
    }

    public async Task<int> CountForPatientAsync(string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Bookings.AsNoTracking().CountAsync(b => b.PatientId == patientId, cancellationToken);
    }

    public async Task<Booking?> GetReviewBookingForUpdateAsync(SubmitReviewDto model, string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == model.BookingId && b.PatientId == patientId, cancellationToken);
    }

    public async Task<Booking?> GetPendingReviewAsync(string? patientId, DateTime now, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Nurse)
            .Include(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .Where(b => b.PatientId == patientId && b.Status == "Completed" && !b.IsReviewSubmitted && !b.IsReviewDismissed && (b.ReviewRemindLaterAt == null || b.ReviewRemindLaterAt <= now))
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<NurseRequestItem>> ListNurseRequestsAsync(string? status, string? nurseId, CancellationToken cancellationToken)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .Where(b => b.NurseId == nurseId).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(b => b.Status == status);
        }

        var requests = await query.OrderByDescending(b => b.BookingDate).ThenBy(b => b.StartTime).Select(b => new NurseRequestItem
        {
            bookingId = b.BookingId,
            patientName = b.Patient.FullName,
            patientPhone = b.Patient.PhoneNumber,
            serviceName = b.Service.ServiceCatalog.Name,
            date = b.BookingDate.ToString("yyyy-MM-dd"),
            time = b.StartTime.ToString(@"hh\:mm"),
            location = b.ServiceAddress,
            durationInMinutes = b.Service.ServiceCatalog.DefaultDurationInMinutes,
            totalPrice = b.Service.Price,
            notes = b.AdditionalNotes,
            status = b.Status
        }).ToListAsync(cancellationToken);
        return requests;
    }

    public async Task<Booking?> GetNurseDetailsAsync(int bookingId, string? nurseId, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.NurseId == nurseId, cancellationToken);
    }

    public async Task<Booking?> GetNurseBookingForUpdateAsync(int bookingId, string? nurseId, CancellationToken cancellationToken)
    {
        return await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId && b.NurseId == nurseId, cancellationToken);
    }

    public async Task<List<NurseAppointmentItem>> ListNurseAppointmentsAsync(string tab, string? nurseId, DateTime today, CancellationToken cancellationToken)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .Where(b => b.NurseId == nurseId).AsQueryable();
        if (tab.ToLower() == "today")
        {
            query = query.Where(b => b.BookingDate.Date == today && (b.Status == "Accepted" || b.Status == "Active"));
        }
        else if (tab.ToLower() == "upcoming")
        {
            query = query
            .Where(b => b.BookingDate.Date > today && (b.Status == "Pending" || b.Status == "Accepted" || b.Status == "Active"));
        }
        else if (tab.ToLower() == "past")
        {
            query = query
            .Where(b => b.Status == "Completed" || b.Status == "Cancelled" || b.Status == "Rejected" || b.BookingDate.Date < today);
        }

        var appointments = await query.OrderBy(b => b.BookingDate).ThenBy(b => b.StartTime).Select(b => new NurseAppointmentItem
        {
            bookingId = b.BookingId,
            patientName = b.Patient.FullName,
            serviceName = b.Service.ServiceCatalog.Name,
            date = b.BookingDate.ToString("yyyy-MM-dd"),
            time = b.StartTime.ToString(@"hh\:mm"),
            address = b.ServiceAddress,
            totalPrice = b.Service.Price,
            status = b.Status
        }).ToListAsync(cancellationToken);
        return appointments;
    }

    public async Task<List<WeeklyActivityItem>> GetWeeklyActivityAsync(DateTime today, DateTime weekStart, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.CreatedAt >= weekStart && b.CreatedAt < today.AddDays(1))
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new WeeklyActivityItem
            {
                date = g.Key,
                requests = g.Count(),
                completed = g.Count(b => b.Status == "Completed")
            }).OrderBy(x => x.date).ToListAsync(cancellationToken);
    }

    public async Task<List<RequestStatusCount>> GetStatusCountsAsync(CancellationToken cancellationToken)
    {
        return await _context.Bookings.AsNoTracking().GroupBy(b => b.Status).Select(g => new RequestStatusCount
        {
            status = g.Key,
            count = g.Count()
        }).ToListAsync(cancellationToken);
    }

    public async Task<int> CountCreatedOnAsync(DateTime today, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .AsNoTracking().CountAsync(b => b.CreatedAt >= today && b.CreatedAt < today.AddDays(1), cancellationToken);
    }

    public async Task<List<ServiceRequestItem>> SearchRequestsAsync(string? search, string? status, CancellationToken cancellationToken)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.Nurse)
            .Include(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog).AsQueryable();
        // 🔍 Search
        if (!string.IsNullOrEmpty(search))
        {
            query = query
            .Where(b => b.Patient.FullName.Contains(search) || b.Nurse.FullName.Contains(search) || b.Service.ServiceCatalog.Name.Contains(search));
        }

        // 🎯 Filter by Status
        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            query = query.Where(b => b.Status == status);
        }

        var data = await query.OrderByDescending(b => b.CreatedAt).Select(b => new ServiceRequestItem
        {
            requestId = b.BookingId,
            patientName = b.Patient.FullName,
            nurseName = b.Nurse != null ? b.Nurse.FullName : "Unassigned",
            serviceType = b.Service.ServiceCatalog.Name,
            dateTime = b.BookingDate,
            status = b.Status
        }).ToListAsync(cancellationToken);
        return data;
    }

    public async Task<Booking?> GetDetailsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.Nurse)
            .Include(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.BookingId == id, cancellationToken);
    }
}
