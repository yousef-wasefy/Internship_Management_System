using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Helpers;

namespace InternshipManagement.Tests.TestHelpers;

// AuthService only cares that it gets *a* token and expiry back to hand to the caller -
// how a real JWT gets signed (JwtTokenGenerator, Program.cs's Jwt:Key config) isn't a
// business rule AuthService owns, so it's stubbed out here rather than wiring up real
// signing-key configuration just to make these tests runnable.
public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public (string Token, DateTime ExpiresAt) GenerateToken(User user) =>
        ("fake-token", DateTime.UtcNow.AddMinutes(60));
}
