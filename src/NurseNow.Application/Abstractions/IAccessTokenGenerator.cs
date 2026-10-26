using NurseNow.Models;

namespace NurseNow.Application.Abstractions;
public interface IAccessTokenGenerator
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
