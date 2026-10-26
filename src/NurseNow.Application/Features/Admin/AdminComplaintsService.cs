using Microsoft.EntityFrameworkCore;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminComplaintsService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    public AdminComplaintsService(IApplicationDbContext context, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
    }

    public async Task<OperationResult> GetComplaints(string? search = "", string? status = "")
    {
        var query = _context.Complaints.AsNoTracking().Include(c => c.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.User.FullName.Contains(search) || (c.User.Email != null && c.User.Email.Contains(search)) || c.Subject.Contains(search) || c.Category.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            query = query.Where(c => c.Status == status);
        }

        var complaints = await query.OrderByDescending(c => c.CreatedAt).Select(c => new { complaintId = c.ComplaintId, submittedBy = c.User.FullName, userRole = c.User.RoleType, category = c.Category, subject = c.Subject, date = c.CreatedAt, status = c.Status, isUrgent = c.IsUrgent }).ToListAsync(_requestContext.CancellationToken);
        return Ok(complaints);
    }

    public async Task<OperationResult> GetComplaintDetails(int id)
    {
        var complaint = await _context.Complaints.AsNoTracking().Include(c => c.User).FirstOrDefaultAsync(c => c.ComplaintId == id, _requestContext.CancellationToken);
        if (complaint == null)
            return NotFound(new { message = "Complaint not found" });
        return Ok(new { complaintId = complaint.ComplaintId, submittedBy = new { userId = complaint.UserId, fullName = complaint.User.FullName, email = complaint.User.Email, role = complaint.User.RoleType, phone = complaint.User.PhoneNumber }, category = complaint.Category, subject = complaint.Subject, description = complaint.Description, isUrgent = complaint.IsUrgent, status = complaint.Status, adminResponse = complaint.AdminResponse, createdAt = complaint.CreatedAt, respondedAt = complaint.RespondedAt });
    }

    public async Task<OperationResult> SendResponse(int id, ComplaintResponseDto dto)
    {
        var complaint = await _context.Complaints.Include(c => c.User).FirstOrDefaultAsync(c => c.ComplaintId == id, _requestContext.CancellationToken);
        if (complaint == null)
            return NotFound(new { message = "Complaint not found" });
        if (string.IsNullOrWhiteSpace(dto.AdminResponse))
            return BadRequest(new { message = "Response is required" });
        complaint.AdminResponse = dto.AdminResponse;
        complaint.RespondedAt = DateTime.UtcNow;
        complaint.Status = "In Progress";
        _context.AdminActivityLogs.Add(new Models.AdminActivityLog { Title = "Complaint response sent", Description = $"Admin responded to complaint #{complaint.ComplaintId}: {complaint.Subject}", ActivityType = "Complaint", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Response sent successfully" });
    }

    public async Task<OperationResult> MarkAsResolved(int id)
    {
        var complaint = await _context.Complaints.Include(c => c.User).FirstOrDefaultAsync(c => c.ComplaintId == id, _requestContext.CancellationToken);
        if (complaint == null)
            return NotFound(new { message = "Complaint not found" });
        complaint.Status = "Resolved";
        complaint.RespondedAt ??= DateTime.UtcNow;
        _context.AdminActivityLogs.Add(new Models.AdminActivityLog { Title = "Complaint resolved", Description = $"Complaint #{complaint.ComplaintId} has been marked as resolved.", ActivityType = "Complaint", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Complaint marked as resolved" });
    }
}

public class ComplaintResponseDto
{
    public string AdminResponse { get; set; } = "";
}
