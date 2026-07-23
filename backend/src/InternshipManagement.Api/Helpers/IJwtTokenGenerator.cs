using InternshipManagement.Api.Entities;

namespace InternshipManagement.Api.Helpers;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
