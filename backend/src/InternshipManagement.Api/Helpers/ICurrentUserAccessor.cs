using System.Security.Claims;

namespace InternshipManagement.Api.Helpers;

// Resolves "who is making this request" from the JWT claims already validated by the
// authentication middleware. Reused wherever a controller needs "the logged-in user"
// (starting with /auth/me here; Phase 7's student/company "me" endpoints reuse this too).
public interface ICurrentUserAccessor
{
    int? GetUserId(ClaimsPrincipal principal);
}
