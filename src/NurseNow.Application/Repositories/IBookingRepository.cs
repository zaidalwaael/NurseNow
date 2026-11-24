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
public interface IBookingRepository
{
    void Add(Booking entity);
    void Remove(Booking entity);
    Task<List<ReservedTimeSlot>> ListReservedSlotsAsync(string nurseId, DateTime day, DateTime nextDay, CancellationToken cancellationToken);
    Task<bool> HasOverlapAsync(CreateBookingRequestDto model, TimeSpan endTime, CancellationToken cancellationToken);
    Task<List<Booking>> ListPatientAppointmentsAsync(string tab, int? limit, string? patientId, CancellationToken cancellationToken);
    Task<Booking?> GetPatientDetailsAsync(int bookingId, string? patientId, CancellationToken cancellationToken);
    Task<Booking?> GetPatientBookingForUpdateAsync(int bookingId, string? patientId, CancellationToken cancellationToken);
    Task<Booking?> GetPatientBookingWithServiceForUpdateAsync(int bookingId, string? patientId, CancellationToken cancellationToken);
    Task<Booking?> GetPatientBookingAsync(int bookingId, string? patientId, CancellationToken cancellationToken);
    Task<int> CountActiveForPatientAsync(string? patientId, CancellationToken cancellationToken);
    Task<int> CountForPatientAsync(string? patientId, CancellationToken cancellationToken);
    Task<Booking?> GetReviewBookingForUpdateAsync(SubmitReviewDto model, string? patientId, CancellationToken cancellationToken);
    Task<Booking?> GetPendingReviewAsync(string? patientId, DateTime now, CancellationToken cancellationToken);
    Task<List<NurseRequestItem>> ListNurseRequestsAsync(string? status, string? nurseId, CancellationToken cancellationToken);
    Task<Booking?> GetNurseDetailsAsync(int bookingId, string? nurseId, CancellationToken cancellationToken);
    Task<Booking?> GetNurseBookingForUpdateAsync(int bookingId, string? nurseId, CancellationToken cancellationToken);
    Task<List<NurseAppointmentItem>> ListNurseAppointmentsAsync(string tab, string? nurseId, DateTime today, CancellationToken cancellationToken);
    Task<List<WeeklyActivityItem>> GetWeeklyActivityAsync(DateTime today, DateTime weekStart, CancellationToken cancellationToken);
    Task<List<RequestStatusCount>> GetStatusCountsAsync(CancellationToken cancellationToken);
    Task<int> CountCreatedOnAsync(DateTime today, CancellationToken cancellationToken);
    Task<List<ServiceRequestItem>> SearchRequestsAsync(string? search, string? status, CancellationToken cancellationToken);
    Task<Booking?> GetDetailsAsync(int id, CancellationToken cancellationToken);
}
