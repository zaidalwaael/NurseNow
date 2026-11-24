using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.Application.Repositories;
using NurseNow.Application.Abstractions;

namespace NurseNow.API.Controllers;
[ApiController, Authorize, Route("api/files")]
public sealed class FilesController(INurseDocumentRepository documents, IConfiguration configuration) : ControllerBase
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
        var owner = await documents.FindFileOwnerAsync(path, legacy, cancellationToken);
        if (owner is null)
            return NotFound();
        if (!owner.IsProfileImage && owner.UserId != userId && !User.IsInRole("Administrator"))
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
