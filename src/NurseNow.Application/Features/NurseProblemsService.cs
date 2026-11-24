using NurseNow.Application.Repositories;
using System.Security.Claims;
using NurseNow.DTOs.Nurse;
using NurseNow.Models;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class NurseProblemsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IComplaintRepository _complaints;
    private readonly IAdminActivityLogRepository _adminActivityLogs;
    public NurseProblemsService(IUnitOfWork unitOfWork, IComplaintRepository complaints, IAdminActivityLogRepository adminActivityLogs, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _complaints = complaints;
        _adminActivityLogs = adminActivityLogs;
    }

    // =========================================
    // POST: api/nurse/problems
    // =========================================
    public async Task<OperationResult> SubmitProblem(SubmitProblemDto dto)
    {
        var nurseId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nurseId))
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
            return BadRequest(new { message = "Problem category is required." });
        if (!allowedCategories.Contains(dto.Category))
            return BadRequest(new { message = "Invalid problem category." });
        if (string.IsNullOrWhiteSpace(dto.Subject))
            return BadRequest(new { message = "Subject is required." });
        if (dto.Subject.Trim().Length > 200)
            return BadRequest(new { message = "Subject must not exceed 200 characters." });
        if (string.IsNullOrWhiteSpace(dto.Description))
            return BadRequest(new { message = "Detailed description is required." });
        if (dto.Description.Trim().Length > 1000)
            return BadRequest(new { message = "Description must not exceed 1000 characters." });
        var complaint = new Complaint
        {
            UserId = nurseId,
            Category = dto.Category.Trim(),
            Subject = dto.Subject.Trim(),
            Description = dto.Description.Trim(),
            IsUrgent = dto.IsUrgent,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };
        _complaints.Add(complaint);
        _adminActivityLogs.Add(new AdminActivityLog { Title = "New Nurse Problem Report", Description = $"A nurse submitted a new problem report: {dto.Subject}", ActivityType = "Complaint", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Problem report submitted successfully.", complaintId = complaint.ComplaintId });
    }

    // =========================================
    // GET: api/nurse/problems/my-reports
    // =========================================
    public async Task<OperationResult> GetMyReports()
    {
        var nurseId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var reports = await _complaints.ListNurseProblemsAsync(nurseId, _requestContext.CancellationToken);
        return Ok(reports);
    }

    // =========================================
    // GET: api/nurse/problems/{id}
    // =========================================
    public async Task<OperationResult> GetProblemDetails(int id)
    {
        var nurseId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var report = await _complaints.GetNurseProblemAsync(id, nurseId, _requestContext.CancellationToken);
        if (report == null)
            return NotFound(new { message = "Problem report not found." });
        return Ok(report);
    }
}
