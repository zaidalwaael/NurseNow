using NurseNow.Application.Repositories;
using System.Security.Claims;
using NurseNow.DTOs.Patient;
using NurseNow.Models;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class PatientIssuesService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IComplaintRepository _complaints;
    private readonly IAdminActivityLogRepository _adminActivityLogs;
    public PatientIssuesService(IUnitOfWork unitOfWork, IComplaintRepository complaints, IAdminActivityLogRepository adminActivityLogs, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _complaints = complaints;
        _adminActivityLogs = adminActivityLogs;
    }

    // =========================================
    // POST: api/patient/issues
    // =========================================
    public async Task<OperationResult> SubmitIssue(SubmitPatientIssueDto dto)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var allowedCategories = new[]
        {
            "Technical",
            "Payment",
            "Service Issue",
            "Account",
            "Other"
        };
        if (string.IsNullOrWhiteSpace(dto.Category))
            return BadRequest(new { message = "Category is required." });
        if (!allowedCategories.Contains(dto.Category))
            return BadRequest(new { message = "Invalid category." });
        if (string.IsNullOrWhiteSpace(dto.Subject))
            return BadRequest(new { message = "Subject is required." });
        if (dto.Subject.Trim().Length > 200)
            return BadRequest(new { message = "Subject must not exceed 200 characters." });
        if (string.IsNullOrWhiteSpace(dto.Description))
            return BadRequest(new { message = "Description is required." });
        if (dto.Description.Trim().Length > 1000)
            return BadRequest(new { message = "Description must not exceed 1000 characters." });
        var complaint = new Complaint
        {
            UserId = patientId,
            Category = dto.Category.Trim(),
            Subject = dto.Subject.Trim(),
            Description = dto.Description.Trim(),
            IsUrgent = dto.IsUrgent,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };
        _complaints.Add(complaint);
        _adminActivityLogs.Add(new AdminActivityLog { Title = "New Patient Issue Report", Description = $"A patient submitted a new issue report: {dto.Subject}", ActivityType = "Complaint", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Issue submitted successfully.", complaintId = complaint.ComplaintId });
    }

    // =========================================
    // GET: api/patient/issues/my-reports
    // =========================================
    public async Task<OperationResult> GetMyReports()
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var reports = await _complaints.ListPatientIssuesAsync(patientId, _requestContext.CancellationToken);
        return Ok(reports);
    }

    // =========================================
    // GET: api/patient/issues/{id}
    // =========================================
    public async Task<OperationResult> GetIssueDetails(int id)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var report = await _complaints.GetPatientIssueAsync(id, patientId, _requestContext.CancellationToken);
        if (report == null)
            return NotFound(new { message = "Issue report not found." });
        return Ok(report);
    }
}
