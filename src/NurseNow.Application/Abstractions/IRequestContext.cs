using System.Security.Claims;

namespace NurseNow.Application.Abstractions;
public interface IRequestContext
{
    ClaimsPrincipal User { get; }

    string PublicBaseUrl { get; }

    CancellationToken CancellationToken { get; }
}
