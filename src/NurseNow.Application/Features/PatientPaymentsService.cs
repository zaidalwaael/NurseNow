using NurseNow.Application.Repositories;
using System.Security.Claims;
using NurseNow.DTOs.Patient;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class PatientPaymentsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IPaymentRepository _payments;
    public PatientPaymentsService(IPaymentRepository payments, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _payments = payments;
    }

    // =========================================
    // GET: api/patient/payments/history
    // =========================================
    public async Task<OperationResult> GetPaymentHistory()
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var history = await _payments.ListPatientHistoryAsync(patientId, _requestContext.CancellationToken);
        return Ok(history);
    }

    // =========================================
    // GET: api/patient/payments/{id}
    // =========================================
    public async Task<OperationResult> GetPaymentDetails(int id)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var payment = await _payments.GetPatientDetailsAsync(id, patientId, _requestContext.CancellationToken);
        if (payment == null)
            return NotFound(new { message = "Transaction not found." });
        return Ok(payment);
    }
}
