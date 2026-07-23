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
        await EnsurePlaceholderCompanySeededAsync(context);
        await EnsureAdminSeededAsync(context);
    }

    // TEMPORARY: seeds one placeholder company so InternshipPost.CompanyId has something
    // valid to point at before authentication/company registration exist. InternshipService
    // currently assigns every new post to this company. This method (only this one) is
    // removed in Phase 8, once posts are tied to the actual logged-in company instead.
    private static async Task EnsurePlaceholderCompanySeededAsync(AppDbContext context)
    {
        if (await context.CompanyProfiles.AnyAsync())
        {
            return; // already seeded
        }

        var now = DateTime.UtcNow;

        var companyProfile = new CompanyProfile
        {
            User = new User
            {
                Email = "placeholder-company@example.com",
                // A real (but nobody-knows-it) hash, so an accidental login attempt
                // against this seeded account fails cleanly with 401 instead of crashing
                // BCrypt.Verify on a non-bcrypt-formatted string.
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Role = UserRole.Company,
                CreatedAt = now,
                UpdatedAt = now
            },
            CompanyName = "Placeholder Company (temporary - see Phase 8)",
            IsApproved = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.CompanyProfiles.Add(companyProfile);
        await context.SaveChangesAsync();
    }

    // Seeds one Admin account so there's something to log in with while testing
    // authentication (this phase) and the admin endpoints (Phase 11). Permanent -
    // NOT removed in Phase 8, unlike the placeholder company above.
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
