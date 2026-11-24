using NurseNow.Application.DTOs.Persistence;
using NurseNow.Models;
using NurseNow.DTOs;
using NurseNow.DTOs.Nurse;
using NurseNow.DTOs.Patient;
using NurseNow.Application.Features.Admin;

namespace NurseNow.Application.Repositories;

/// <summary>Persistence operations; Add/Remove stage changes for IUnitOfWork.
/// Queries ending in ForUpdate (and verification/status update queries) return tracked entities.
/// Read projections are materialized inside Infrastructure.</summary>
public interface IPaymentRepository
{
    void Add(Payment entity);
    void Remove(Payment entity);
    Task<List<Payment>> ListForNurseSummaryAsync(string? nurseId, CancellationToken cancellationToken);
    Task<List<NursePaymentHistoryItemDto>> ListNurseHistoryAsync(string? tab, string? nurseId, CancellationToken cancellationToken);
    Task<NursePaymentHistoryDetailsDto?> GetNurseDetailsAsync(int id, string? nurseId, CancellationToken cancellationToken);
    Task<List<PatientPaymentHistoryItemDto>> ListPatientHistoryAsync(string? patientId, CancellationToken cancellationToken);
    Task<PatientPaymentDetailsDto?> GetPatientDetailsAsync(int id, string? patientId, CancellationToken cancellationToken);
    Task<Payment?> GetByBookingForUpdateAsync(int bookingId, CancellationToken cancellationToken);
    Task<Payment?> GetPaidByBookingAsync(int bookingId, CancellationToken cancellationToken);
    Task<Payment?> GetByBookingAsync(int bookingId, CancellationToken cancellationToken);
    Task<List<PaymentTransactionItem>> ListTransactionsAsync(CancellationToken cancellationToken);
    Task<Dictionary<string, decimal>> GetTotalsByStatusAsync(CancellationToken cancellationToken);
}
