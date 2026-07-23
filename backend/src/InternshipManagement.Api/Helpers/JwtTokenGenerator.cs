using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternshipManagement.Api.Entities;
using Microsoft.IdentityModel.Tokens;

namespace InternshipManagement.Api.Helpers;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured - set it via dotnet user-secrets.");
        var expiryMinutes = jwtSection.GetValue("ExpiryMinutes", 60);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // ClaimTypes.Role (not a plain "role" string) is what [Authorize(Roles = "...")]
        // checks against by default - using it here means role-based authorization
        // works with zero extra configuration in Program.cs.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
