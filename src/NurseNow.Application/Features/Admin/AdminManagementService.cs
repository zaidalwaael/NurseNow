using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NurseNow.Models;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features.Admin;
public sealed class AdminManagementService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public AdminManagementService(IApplicationDbContext context, UserManager<ApplicationUser> userManager, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
        _userManager = userManager;
    }

    private bool IsSuperAdmin() => _requestContext.User.FindFirst("admin_role")?.Value == "SuperAdmin";
    public async Task<OperationResult> GetAdmins()
    {
        if (!IsSuperAdmin())
            return Forbid();
        var admins = await _context.Users.AsNoTracking().Where(u => u.RoleType == "Administrator").OrderByDescending(u => u.CreatedAt).Select(u => new { adminId = u.Id, fullName = u.FullName, email = u.Email, createdDate = u.CreatedAt }).ToListAsync(_requestContext.CancellationToken);
        return Ok(admins);
    }

    public async Task<OperationResult> CreateAdmin(CreateAdminDto dto)
    {
        await using var transaction = await _context.BeginTransactionAsync(_requestContext.CancellationToken);
        if (!IsSuperAdmin())
            return Forbid();
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest(new { message = "Full name is required" });
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Email is required" });
        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Password is required" });
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest(new { message = "Email already exists" });
        var admin = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            RoleType = "Administrator",
            EmailConfirmed = true,
            AccountStatus = "Active",
            CreatedAt = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(admin, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        var roleResult = await _userManager.AddToRoleAsync(admin, "Administrator");
        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);
        _context.AdminActivityLogs.Add(new AdminActivityLog { Title = "New admin created", Description = $"{dto.FullName} was added as an administrator.", ActivityType = "AdminManagement", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        await transaction.CommitAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Admin created successfully" });
    }

    public async Task<OperationResult> DeleteAdmin(string adminId)
    {
        await using var transaction = await _context.BeginTransactionAsync(_requestContext.CancellationToken);
        if (!IsSuperAdmin())
            return Forbid();
        var admin = await _userManager.FindByIdAsync(adminId);
        if (admin == null)
            return NotFound(new { message = "Admin not found" });
        if (admin.AdminRole == "SuperAdmin")
        {
            return BadRequest(new { message = "Main admin cannot be deleted" });
        }

        var result = await _userManager.DeleteAsync(admin);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        _context.AdminActivityLogs.Add(new AdminActivityLog { Title = "Admin deleted", Description = $"{admin.FullName} was deleted from admin accounts.", ActivityType = "AdminManagement", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        await transaction.CommitAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Admin deleted successfully" });
    }
}

public class CreateAdminDto
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
