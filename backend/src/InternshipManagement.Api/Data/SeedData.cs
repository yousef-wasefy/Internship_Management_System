using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Data;

public static class SeedData
{
    public const string AdminEmail = "admin@internship-system.local";

    // Dev-only seed credential, documented in docs/API_SPEC.md and docs/DECISIONS.md.
    // Never use a hardcoded password like this for a real deployment (Phase 17) -
    // rotate or remove this account before going live.
    private const string AdminPassword = "Admin@12345";

    public static async Task EnsureSeededAsync(AppDbContext context)
    {
        await EnsureAdminSeededAsync(context);
    }

    // Seeds one Admin account so there's something to log in with while testing
    // authentication (Phase 6) and the admin endpoints (Phase 7+).
    private static async Task EnsureAdminSeededAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            return; // already seeded
        }

        var now = DateTime.UtcNow;

        context.Users.Add(new User
        {
            Email = AdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword),
            Role = UserRole.Admin,
            CreatedAt = now,
            UpdatedAt = now
        });

        await context.SaveChangesAsync();
    }
}
