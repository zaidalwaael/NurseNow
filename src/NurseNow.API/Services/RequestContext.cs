using System.Security.Claims;
using NurseNow.Application.Abstractions;

namespace NurseNow.API.Services;
public sealed class RequestContext(IHttpContextAccessor accessor, IConfiguration configuration) : IRequestContext
{
    public ClaimsPrincipal User => accessor.HttpContext?.User ?? new ClaimsPrincipal();
    public CancellationToken CancellationToken => accessor.HttpContext?.RequestAborted ?? CancellationToken.None;
    public string PublicBaseUrl => configuration["PublicBaseUrl"]!.TrimEnd('/');
}
