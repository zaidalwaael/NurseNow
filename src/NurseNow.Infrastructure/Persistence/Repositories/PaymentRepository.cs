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
public sealed class PaymentRepository(ApplicationDbContext context) : IPaymentRepository
{
    private readonly ApplicationDbContext _context = context;
    public void Add(Payment entity) => _context.Payments.Add(entity);
    public void Remove(Payment entity) => _context.Payments.Remove(entity);
    public async Task<List<Payment>> ListForNurseSummaryAsync(string? nurseId, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .Where(p => p.Booking.NurseId == nurseId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NursePaymentHistoryItemDto>> ListNurseHistoryAsync(string? tab, string? nurseId, CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .ThenInclude(b => b.Patient)
            .Include(p => p.Booking)
            .ThenInclude(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .Where(p => p.Booking.NurseId == nurseId).AsQueryable();
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        if (tab == "thisMonth")
        {
            query = query.Where(p => p.CreatedAt >= monthStart && p.CreatedAt < nextMonthStart);
        }
        else if (tab == "history")
        {
            query = query.Where(p => p.Status == "Paid" || p.Status == "Completed");
        }

        var payments = await query.OrderByDescending(p => p.CreatedAt).Select(p => new NursePaymentHistoryItemDto
        {
            PaymentId = p.PaymentId,
            BookingId = p.BookingId,
            PatientName = p.Booking.Patient.FullName,
            ServiceName = p.Booking.Service.ServiceCatalog.Name,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status,
            CreatedAt = p.CreatedAt
        }).ToListAsync(cancellationToken);
        return payments;
    }

    public async Task<NursePaymentHistoryDetailsDto?> GetNurseDetailsAsync(int id, string? nurseId, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .ThenInclude(b => b.Patient)
            .Include(p => p.Booking)
            .ThenInclude(b => b.Nurse)
            .Include(p => p.Booking)
            .ThenInclude(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .Where(p => p.PaymentId == id && p.Booking.NurseId == nurseId)
            .Select(p => new NursePaymentHistoryDetailsDto
            {
                PaymentId = p.PaymentId,
                BookingId = p.BookingId,
                PatientName = p.Booking.Patient.FullName,
                NurseName = p.Booking.Nurse.FullName,
                ServiceName = p.Booking.Service.ServiceCatalog.Name,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                ServiceAddress = p.Booking.ServiceAddress,
                BookingDate = p.Booking.BookingDate,
                StartTime = p.Booking.StartTime,
                EndTime = p.Booking.EndTime
            }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PatientPaymentHistoryItemDto>> ListPatientHistoryAsync(string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .ThenInclude(b => b.Nurse)
            .Include(p => p.Booking)
            .ThenInclude(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .Where(p => p.Booking.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PatientPaymentHistoryItemDto
            {
                PaymentId = p.PaymentId,
                BookingId = p.BookingId,
                Amount = p.Amount,
                Status = p.Status,
                PaymentMethod = p.PaymentMethod,
                CreatedAt = p.CreatedAt,
                NurseName = p.Booking.Nurse.FullName,
                ServiceName = p.Booking.Service.ServiceCatalog.Name
            }).ToListAsync(cancellationToken);
    }

    public async Task<PatientPaymentDetailsDto?> GetPatientDetailsAsync(int id, string? patientId, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .ThenInclude(b => b.Patient)
            .Include(p => p.Booking)
            .ThenInclude(b => b.Nurse)
            .Include(p => p.Booking)
            .ThenInclude(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .Where(p => p.PaymentId == id && p.Booking.PatientId == patientId)
            .Select(p => new PatientPaymentDetailsDto
            {
                PaymentId = p.PaymentId,
                BookingId = p.BookingId,
                Amount = p.Amount,
                Status = p.Status,
                PaymentMethod = p.PaymentMethod,
                CreatedAt = p.CreatedAt,
                PatientName = p.Booking.Patient.FullName,
                NurseName = p.Booking.Nurse.FullName,
                ServiceName = p.Booking.Service.ServiceCatalog.Name,
                BookingDate = p.Booking.BookingDate,
                StartTime = p.Booking.StartTime,
                EndTime = p.Booking.EndTime,
                ServiceAddress = p.Booking.ServiceAddress
            }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Payment?> GetByBookingForUpdateAsync(int bookingId, CancellationToken cancellationToken)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
    }

    public async Task<Payment?> GetPaidByBookingAsync(int bookingId, CancellationToken cancellationToken)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId && p.Status == "Paid", cancellationToken);
    }

    public async Task<Payment?> GetByBookingAsync(int bookingId, CancellationToken cancellationToken)
    {
        return await _context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
    }

    public async Task<List<PaymentTransactionItem>> ListTransactionsAsync(CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .ThenInclude(b => b.Patient)
            .Include(p => p.Booking)
            .ThenInclude(b => b.Nurse)
            .Include(p => p.Booking)
            .ThenInclude(b => b.Service)
            .ThenInclude(s => s.ServiceCatalog)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentTransactionItem
            {

                id = p.PaymentId,

                transactionId = "TXN-" + p.PaymentId,
                // مؤقت
                patientName = p.Booking.Patient.FullName,

                nurseName = p.Booking.Nurse.FullName,

                service = p.Booking.Service.ServiceCatalog.Name,

                totalAmount = p.Amount,

                status = p.Status,

                paymentMethod = p.PaymentMethod,

                date = p.CreatedAt

            }).ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, decimal>> GetTotalsByStatusAsync(CancellationToken cancellationToken)
    {
        return await _context.Payments.AsNoTracking().GroupBy(p => p.Status).Select(g => new PaymentStatusTotal
        {
            Status = g.Key,
            Total = g.Sum(p => p.Amount)
        }).ToDictionaryAsync(x => x.Status, x => x.Total, cancellationToken);
    }
}
