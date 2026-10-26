using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Application.Abstractions;

namespace NurseNow.API.Controllers;
[ApiController, Authorize, Route("api/files")]
public sealed class FilesController(IApplicationDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpGet("{name}")]
    [HttpGet("/uploads/{name}")]
    public async Task<IActionResult> Download(string name, CancellationToken cancellationToken)
    {
        if (name != Path.GetFileName(name) || name.Contains('/') || name.Contains('\\'))
            return BadRequest();
        var path = "api/files/" + name;
        var legacy = "uploads/" + name;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profile = await db.NurseProfiles.AsNoTracking().Where(n => n.ProfileImagePath == path || n.CertificatePath == path || n.NationalIdImagePath == path || n.ProfileImagePath == legacy || n.CertificatePath == legacy || n.NationalIdImagePath == legacy).Select(n => new { n.UserId, n.ProfileImagePath }).FirstOrDefaultAsync(cancellationToken);
        var document = profile is null ? await db.NurseDocuments.AsNoTracking().Where(d => d.FileUrl == path || d.FileUrl == legacy).Select(d => d.NurseId).FirstOrDefaultAsync(cancellationToken) : null;
        if (profile is null && document is null)
            return NotFound();
        var isPublicProfileImage = profile?.ProfileImagePath == path || profile?.ProfileImagePath == legacy;
        if (!isPublicProfileImage && (profile?.UserId ?? document) != userId && !User.IsInRole("Administrator"))
            return Forbid();
        var diskPath = Path.Combine(Path.GetFullPath(configuration["Storage:Root"] ?? "App_Data/uploads"), name);
        if (!System.IO.File.Exists(diskPath))
            return NotFound();
        var type = Path.GetExtension(name).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(diskPath, type, name);
    }
}
