using NurseNow.Application.Repositories;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminComplaintsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IComplaintRepository _complaints;
    private readonly IAdminActivityLogRepository _adminActivityLogs;
    public AdminComplaintsService(IUnitOfWork unitOfWork, IComplaintRepository complaints, IAdminActivityLogRepository adminActivityLogs, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _complaints = complaints;
        _adminActivityLogs = adminActivityLogs;
    }

    public async Task<OperationResult> GetComplaints(string? search = "", string? status = "")
    {
        var complaints = await _complaints.SearchComplaintsAsync(search, status, _requestContext.CancellationToken);
        return Ok(complaints);
    }

    public async Task<OperationResult> GetComplaintDetails(int id)
    {
        var complaint = await _complaints.GetDetailsAsync(id, _requestContext.CancellationToken);
        if (complaint == null)
            return NotFound(new { message = "Complaint not found" });
        return Ok(new { complaintId = complaint.ComplaintId, submittedBy = new { userId = complaint.UserId, fullName = complaint.User.FullName, email = complaint.User.Email, role = complaint.User.RoleType, phone = complaint.User.PhoneNumber }, category = complaint.Category, subject = complaint.Subject, description = complaint.Description, isUrgent = complaint.IsUrgent, status = complaint.Status, adminResponse = complaint.AdminResponse, createdAt = complaint.CreatedAt, respondedAt = complaint.RespondedAt });
    }

    public async Task<OperationResult> SendResponse(int id, ComplaintResponseDto dto)
    {
        var complaint = await _complaints.GetForUpdateAsync(id, _requestContext.CancellationToken);
        if (complaint == null)
            return NotFound(new { message = "Complaint not found" });
        if (string.IsNullOrWhiteSpace(dto.AdminResponse))
            return BadRequest(new { message = "Response is required" });
        complaint.AdminResponse = dto.AdminResponse;
        complaint.RespondedAt = DateTime.UtcNow;
        complaint.Status = "In Progress";
        _adminActivityLogs.Add(new Models.AdminActivityLog { Title = "Complaint response sent", Description = $"Admin responded to complaint #{complaint.ComplaintId}: {complaint.Subject}", ActivityType = "Complaint", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Response sent successfully" });
    }

    public async Task<OperationResult> MarkAsResolved(int id)
    {
        var complaint = await _complaints.GetForUpdateAsync(id, _requestContext.CancellationToken);
        if (complaint == null)
            return NotFound(new { message = "Complaint not found" });
        complaint.Status = "Resolved";
        complaint.RespondedAt ??= DateTime.UtcNow;
        _adminActivityLogs.Add(new Models.AdminActivityLog { Title = "Complaint resolved", Description = $"Complaint #{complaint.ComplaintId} has been marked as resolved.", ActivityType = "Complaint", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Complaint marked as resolved" });
    }
}

public class ComplaintResponseDto
{
    public string AdminResponse { get; set; } = "";
}
