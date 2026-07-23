using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InternshipManagement.Api.Helpers;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    public int? GetUserId(ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}
